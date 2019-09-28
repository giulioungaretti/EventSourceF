using System;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using Test.Orleans.Patterns.EventSourcing;
using Orleans.TestingHost;
using Xunit;

using static EventSourcing.Grains;
using static EventSourcing.Interfaces;
using static Test.Grains;
using static Test.Interfaces;


namespace Orleans.Testing.Utilities
{

    public interface ITestGrain : IGrainWithGuidKey
    {
        Task<System.Guid> Id();
    }
    public class TestGrain : Grain, ITestGrain
    {
        public Task<Guid> Id()
        {
            return Task.FromResult(this.GetPrimaryKey());
        }
    }
    public sealed class ClusterFixture : IDisposable
    {
        public ClusterFixture()
        {
            var builder =
                new TestClusterBuilder()
                    .ConfigureHostConfiguration(configurationBuilder =>
                        configurationBuilder.AddEnvironmentVariables());

            builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();

            Cluster = builder.Build();
            Cluster.Deploy();
        }

        public void Dispose()
        {
            Cluster.StopAllSilos();
        }

        public TestCluster Cluster { get; private set; }
    }

    public class TestSiloConfigurations : ISiloBuilderConfigurator
    {
        public IConfiguration Configuration { get; private set; }

        public void Configure(ISiloHostBuilder hostBuilder)
        {
            Configuration = hostBuilder.GetConfiguration();

            hostBuilder
                .AddMemoryGrainStorageAsDefault()
                .UseLocalhostClustering()
                .ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory()
                    .WithCodeGeneration().WithReferences()
                )
                .ConfigureServices(services =>
                {
                    services.AddSingleton(CloudTableFactory(Configuration));
                });
        }

        private static Func<IServiceProvider, CloudTable> CloudTableFactory(IConfiguration configuration) =>
            _ =>
            {
                var storageConnectionString = configuration.EventStorageConnectionString();
                var cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
                var cloudTableClient = cloudStorageAccount.CreateCloudTableClient(new TableClientConfiguration());
                var cloudTable = cloudTableClient.GetTableReference(configuration.EventStorageTableName());
                cloudTable.CreateIfNotExists();
                return cloudTable;
            };
    }

    [CollectionDefinition(Name)]
    public class ClusterCollection : ICollectionFixture<ClusterFixture>
    {
        public const string Name = nameof(ClusterCollection);
    }
}
