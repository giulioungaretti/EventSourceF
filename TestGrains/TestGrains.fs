namespace Test

module Grains =
    open System
    open Microsoft.Azure.Cosmos.Table

    open EventSourcing.Grains
    open Test.Interfaces

    open Data

    type AdderGrain(eventsTable: CloudTable, logger) =
        inherit EventAggregatorGrain<int, AdderGrain>(eventsTable, logger)
        interface IAdderGrainFailure

        override this.ProcessEvent state event =
            let (id, ts, state) = state
            (id, ts, state)

        override this.InitialValue =
            match this.getState() with
            | Valid s ->
                (Guid.Empty, new DateTimeOffset(), s )
            | Empty ->
                (Guid.Empty, new DateTimeOffset(), 0 )
            | Invalid e ->
                (Guid.Empty, new DateTimeOffset(), 0 )


    open System.Threading.Tasks

    type IHello =
        inherit Orleans.IGrainWithIntegerKey
        abstract member SayHello : string -> Task<string>

    type HelloGrainConcreteDifferentProject() =
        inherit Orleans.Grain ()
        interface IHello with
            member this.SayHello (greeting : string) : Task<string> =
                greeting |> sprintf "You said: %s, I say: concrete differnt project!" |> Task.FromResult

