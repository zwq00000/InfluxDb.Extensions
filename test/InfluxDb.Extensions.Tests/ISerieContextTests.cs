using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace InfluxDb.Extensions.Tests
{
    public class ISerieContextTests {
        private readonly ISerieContextFactory factory;
        private readonly ITestOutputHelper output;

        public ISerieContextTests (ITestOutputHelper outputHelper) {
            var testFactory = new TestFactory ();
            this.factory = testFactory.GetService<ISerieContextFactory>();
            Assert.NotNull(factory);
            this.output = outputHelper;
        }

        [Fact]
        public async Task TestGetQueryAsync(){
            var context = await factory.GetContextAsync("ShipTrack");
            var sql = context.BuildQuery().Start(TimeSpan.FromHours(-1));
            var result = await context.GetQueryAsync(sql);
            Assert.True(result.Success);
            // output.WriteLine(result.Body);
            InfluxJsonUtf8Extensions.ReadFromJson(result.Body);
        }
    }
}
