using System;
using System.Threading.Tasks;
using Orleans.Testing.Utilities;
using Orleans.TestingHost;
using Xunit;

//namespace Test.Orleans
//{

//    [Collection(ClusterCollection.Name)]
//    public class BasicTests
//    {
//        private readonly TestCluster _cluster;
//        public BasicTests(ClusterFixture fixture) =>
//            _cluster = fixture?.Cluster ?? throw new ArgumentNullException(nameof(fixture));

//        [Fact]
//        public async Task Fail()
//        {
//            var g = _cluster.GrainFactory.GetGrain<ITestGrain>(System.Guid.NewGuid());
//            await g.Id();
//        }
//    }
//}
