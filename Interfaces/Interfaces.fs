namespace EventSourcing

module Interfaces =

    open System
    open System.Threading.Tasks

    open Data

    type IEventAggregatorGrain  =
        inherit Orleans.IGrainWithGuidKey
        abstract RefreshState : unit -> Task<unit>
        abstract GetValue : unit -> Task<AggregatorSate<'T>>

    type IEventSourcedGrain =
        inherit Orleans.IGrainWithGuidKey
        abstract RecordEventPayload: 'a -> Task<'a>
        abstract RegisterAggregateGrain<'T when 'T :> IEventAggregatorGrain> :  unit -> Task<'T>
        abstract GetEvents:  DateTimeOffset option -> Task< Data.EventC list>

    type IAdderGrain  =
        inherit IEventAggregatorGrain
