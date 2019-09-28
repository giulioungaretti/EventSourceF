module Tests.ClusterFixture

open System
open Orleans.TestingHost
open Microsoft.Extensions.DependencyInjection

open Microsoft.Azure.Cosmos.Table
open Orleans.Hosting
open Orleans

let CloudTable =
        let storageConnectionString = "UseDevelopmentStorage=true"
        let cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString)
        let cloudTableClient = cloudStorageAccount.CreateCloudTableClient(new TableClientConfiguration())
        let cloudTable = cloudTableClient.GetTableReference("events")
        do cloudTable.CreateIfNotExists() |> ignore
        cloudTable

type TestSiloConf ()=
    interface ISiloBuilderConfigurator with
        member _.Configure(builder:ISiloHostBuilder) =
                builder
                    .AddMemoryGrainStorageAsDefault()
                    .UseLocalhostClustering()
                    .ConfigureApplicationParts(fun parts ->
                      //assemblies |> Seq.iter(fun assembly -> parts.AddApplicationPart(assembly).WithCodeGeneration() |> ignore))
                      parts.AddFromDependencyContext().WithCodeGeneration() |> ignore )
                    .ConfigureServices( fun services ->
                        services.AddSingleton(CloudTable) |> ignore
                    )
                    |> ignore

type ClusterFixture()  =
    let builder = new TestClusterBuilder()
    do builder.AddSiloBuilderConfigurator<TestSiloConf>()

    let cluster =  builder.Build()
    do cluster.Deploy()

    member _.Cluster = cluster

    interface IDisposable with
        member this.Dispose(): unit =
            this.Cluster.StopAllSilos()
