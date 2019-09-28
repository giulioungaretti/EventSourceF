namespace EventSourcing

module Grains =

    open System
    open System.Threading.Tasks
    open FSharp.Control.Tasks

    open Microsoft.Extensions.Logging
    open Microsoft.Azure.Cosmos.Table

    open Orleans
    open Interfaces
    open Newtonsoft.Json

    open Data

    let inline notNull value = if (obj.ReferenceEquals(value, null))  then None else Some value

    let asyncQuery (table:CloudTable) (tableQ:TableQuery<Data.EventC>) (f: 'a->Data.EventC->'a) (a:'a) =
        let rec loop (cont: TableContinuationToken) a = async {
            let! ct = Async.CancellationToken
            let! result = table.ExecuteQuerySegmentedAsync(tableQ, cont, ct) |> Async.AwaitTask
            let res = Seq.fold f a result.Results
            match result.ContinuationToken with
            | null -> return res
            | cont -> return! loop cont  res
        }
        loop null a

    let foldEvents (table:CloudTable) (cutoff:System.DateTimeOffset option) (primaryKey:System.Guid) f a =
            let cutOff =
                match cutoff with
                | Some x ->
                    x.Ticks.ToString()
                | None ->
                    (new System.DateTime ()).ToString()

            let queryString =
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(
                        "PartitionKey", QueryComparisons.Equal, primaryKey.ToString()),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(
                        "RowKey", QueryComparisons.GreaterThan, cutOff))

            let tableQuery = TableQuery<Data.EventC>().Where(queryString)
            task {
                 let! res = asyncQuery table tableQuery f a
                 return res
            }

    type GrainListA<'T when 'T :> IEventAggregatorGrain> () =
        member val aggregators : list<'T>  = [] with  get, set
        member this.Add element =
            this.aggregators <- element :: this.aggregators

    type EventSourcedGrain(eventsTable: CloudTable, logger: ILogger<EventSourcedGrain>) =
        inherit Grain<GrainListA<IEventAggregatorGrain>> ()
        member _.logger = logger
        member _.eventsTable = eventsTable
        interface IEventSourcedGrain with

            member this.GetEvents(cutOff: System.DateTimeOffset option): Task<Data.EventC list> =
                let pk = this.GetPrimaryKey()
                let folder state thing =
                     thing :: state
                foldEvents this.eventsTable cutOff pk folder List.empty

            member this.RecordEventPayload(event: 'a): Task<'a> =
                let payload = JsonConvert.SerializeObject event
                task {
                    let! _ = Data.Store this.eventsTable (Data.EventC(payload, System.DateTimeOffset.UtcNow, this.GetPrimaryKey().ToString()))
                    return event
                }

            member this.RegisterAggregateGrain<'T when 'T :> IEventAggregatorGrain> ()=
                let ag = this.GrainFactory.GetGrain<'T>(this.GetPrimaryKey())
                this.State.Add ag |> ignore
                let write = this.WriteStateAsync();
                task {
                    do! write
                    return ag
                }

    [<AbstractClass>]
    type EventAggregatorGrain<'T, 'TGrain>(eventsTable: CloudTable, logger: ILogger<'TGrain>) =
        inherit Grain<Data.EventAggregatorState> ()

        member _.logger = logger
        member _.eventsTable = eventsTable

        abstract member ProcessEvent :  (Guid * DateTimeOffset * 'T) -> Data.EventC ->  (Guid * DateTimeOffset * 'T)
        abstract member InitialValue  : (Guid * DateTimeOffset * 'T)

        member private this.SetState(state) =
            this.State <- state

        member this.getAggregatorState (): EventAggregatorState  =
            this.State

        member this.getState<'S> (): AggregatorSate<'S> =
            this.State.Deserialize<'S>()

        member this.RefreshState()=
            let pk = this.GetPrimaryKey()
            let write = this.WriteStateAsync()
            let init = this.InitialValue
            let ts = this.State.LastEventAggregatedTimestamp
            task{
                let!  (lastID, lastTs, newState)  = foldEvents this.eventsTable ts pk this.ProcessEvent init
                this.SetState( { LastEventAggregatedID = Some lastID
                                 LastEventAggregatedTimestamp = Some lastTs
                                 State = Some (EventAggregatorState.Serialize newState)})
                return! write
            }

        member this.GetValue<'S> () =
                task {
                    let! _ = this.RefreshState()
                    return this.getState<'S>()
                }

        interface IEventAggregatorGrain with
            member this.GetValue () =
                task{
                    return! this.GetValue<'S>()
                }

            member this.RefreshState() =
                this.RefreshState ()

    type Event = Number of int
    type NewEvent =  Number of int
                    | NewNumber of int

    type AdderGrain(eventsTable: CloudTable, logger) =
        inherit EventAggregatorGrain<int, AdderGrain>(eventsTable, logger)
        interface IAdderGrain

        override __.ProcessEvent ((id, ts, state)) event =
            let (id, ts )=
                if event.TimeStamp > ts then
                    (event.ID, event.TimeStamp)
                else
                    (id, ts)

            let newState =
                let res =  event.GeTvalue<Event>()
                match res with
                    | Ok res ->
                        match res with
                        | Event.Number n ->
                            state + n
                    | Error error ->
                        state

            (id, ts, newState)

        override this.InitialValue =
            let state = this.getAggregatorState()
            let id = Option.defaultValue Guid.Empty state.LastEventAggregatedID
            let ts = Option.defaultValue  (new DateTimeOffset()) state.LastEventAggregatedTimestamp
            let innerState= match this.getState() with
                                    | Valid s ->
                                         s
                                    | _ ->
                                         0
            (id, ts, innerState)

