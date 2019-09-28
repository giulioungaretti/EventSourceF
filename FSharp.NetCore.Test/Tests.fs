namespace Tests

module Facts =
    open System
    open FSharp.Control.Tasks

    open Xunit
    open Xunit.Abstractions
    open Tests.ClusterFixture

    open EventSourcing.Interfaces
    open Test.Interfaces
    open Test.Grains
    open EventSourcing

    open Data

    type Event = Number of int
    type NewEvent =  Number of int
                    | NewNumber of int
    [<Literal>]
    let Name = "ClusterCollection"

    [<CollectionDefinition(Name)>]
    type ClusterCollection()=
      interface ICollectionFixture<ClusterFixture>

    [<Collection(Name)>]
    type Tests(fixture: ClusterFixture, outputHelper: ITestOutputHelper) =
        member this.cluster = fixture.Cluster
        member this.OutputHelper  = outputHelper

        [<Fact>]
        member this.``Test grain assembly is loaded!`` () =
            let g = this.cluster.GrainFactory.GetGrain<IHello> 0L
            task {
                let!  res =  g.SayHello("Hi")
                Assert.Equal("You said: Hi, I say: concrete differnt project!", res)
            }

        [<Fact>]
        member this.``This should not fail but it does`` () =
            let primaryKey = Guid.NewGuid();
            let g = this.cluster.GrainFactory.GetGrain<IAdderGrainFailure>(primaryKey)
            task {
                let!  res =  g.GetValue<int>()
                match res with
                   | Valid r ->
                        Assert.Equal(r, 0)
                   | Empty ->
                        Assert.Equal(0, 0)
                   | Invalid err ->
                        Assert.Equal(err, "wut")
          }

        [<Fact>]
        member this.``Events raised are recorded `` () =
            let primaryKey = Guid.NewGuid();
            let g = this.cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);
            task {
               let!  _ =  g.RecordEventPayload("TEST")
               let! res =  g.GetEvents(None)
               let empt = List.empty<Data.EventC> |> List.toSeq
               Assert.Equal(res, empt)
            }

        [<Fact>]
        member this.``Adder adds  `` () =
            let primaryKey = Guid.NewGuid();
            let numberGrain = this.cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey)
            Seq.iter (fun elem -> numberGrain.RecordEventPayload(Event.Number elem) |>Async.AwaitTask |> Async.RunSynchronously |> ignore )  { 0 .. 1 .. 100 }
            task {
               let! numberAgg = numberGrain.RegisterAggregateGrain<IAdderGrain>()
               let!  b =  numberAgg.GetValue<int>();
               match b with
               | Valid r ->
                    Assert.Equal(r, 5050)
               | Empty ->
                    Assert.True(false, "expected 5050, got empty")
               | Invalid err ->
                    Assert.Equal(err, "wut")
            }

