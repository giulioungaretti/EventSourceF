namespace Tests

//open System
//open Xunit
//open Orleans.Testing.Utilities
//open FSharp.Control.Tasks.V2.ContextInsensitive

//open EventSourcing.Interfaces
//open Test.Interfaces


//module Tests =


//    [<Literal>]
//    let Name = "ClusterCollection"

//    [<CollectionDefinition(Name)>]
//    type ClusterCollection()=
//        interface ICollectionFixture<ClusterFixture>

//    [<Collection(Name)>]
//    type Tests(fixture: ClusterFixture) =
//        member this.cluster = fixture.Cluster

//        //[<Fact>]
//        //member this.``Test much`` () =
//        //    let primaryKey = Guid.NewGuid();
//        //    let g = this.cluster.GrainFactory.GetGrain<ITestGrain>(primaryKey);
//        //    task {
//        //       let!  res =  g.Id()
//        //       Assert.Equal(res,primaryKey)
//        //    }

//        [<Fact>]
//        member this.``That much`` () =
//            let primaryKey = Guid.NewGuid();
//            let g = this.cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);
//            task {
//               let!  res =  g.RecordEventPayload("TEST")
//               Assert.Equal(res,"TEST")
//            }

//        [<Fact>]
//        member this.``That tittle`` () =
//            let primaryKey = Guid.NewGuid();
//            let g = this.cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);
//            task {
//               let! res =  g.GetEvents(None)
//               let empt = List.empty<Data.EventC>
//               Assert.True((empt=res))
//            }

//        [<Fact>]
//        member this.``all That much`` () =
//            let primaryKey = Guid.NewGuid();
//            let g = this.cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);
//            task {
//               let!  _ =  g.RecordEventPayload("TEST")
//               let! res =  g.GetEvents(None)
//               let empt = List.empty<Data.EventC> |> List.toSeq
//               Assert.Equal(res, empt)
//            }

//        [<Fact>]
//        member this.``Adder adds  `` () =
//            let primaryKey = Guid.NewGuid();
//            let g = this.cluster.GrainFactory.GetGrain<IAdderGrain>(primaryKey);
//            task {
//               let!  b =  g.GetValue<int>();
//               match b with
//               | Ok r ->
//                    Assert.Equal(r, 0)
//               | Error err ->
//                    Assert.Equal(err, "wut")
//            }

//        [<Fact>]
//        member this.``this should not fail but it does `` () =
//            let primaryKey = Guid.NewGuid()
//            let g = this.cluster.GrainFactory.GetGrain<IAdderGrainFailure>(primaryKey);
//            task {
//               let!  b =  g.GetValue<int>();
//               match b with
//               | Ok r ->
//                    Assert.Equal(r, 0)
//               | Error err ->
//                    Assert.Equal(err, "wut")
//            }
