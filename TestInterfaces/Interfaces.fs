namespace Test

module Interfaces=

    open EventSourcing.Interfaces

    type IAdderGrainFailure  =
        inherit IEventAggregatorGrain
