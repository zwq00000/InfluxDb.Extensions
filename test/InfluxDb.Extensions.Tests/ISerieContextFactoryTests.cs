using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace InfluxDb.Extensions.Tests
{
    public class ISerieContextFactoryTests {
        private readonly TestFactory factory;
        private readonly ITestOutputHelper output;

        public ISerieContextFactoryTests (ITestOutputHelper outputHelper) {
            this.factory = new TestFactory ();
            this.output = outputHelper;
        }

        [Fact]
        public void TestResolve () {
            var contextFactory = this.factory.GetService<ISerieContextFactory> ();
            Assert.NotNull (contextFactory);
        }

        [Fact]
        public async Task TestEnsureDatabaseCreatedAsync () {
            var contextFactory = this.factory.GetService<ISerieContextFactory> ();
            var result = await contextFactory.EnsureCreatedAsync ();
            Assert.True (result);
        }

        [Fact]
        public async Task TestGetMeasurementsAsync () {
            var contextFactory = this.factory.GetService<ISerieContextFactory> ();
            var m = await contextFactory.GetMeasurementsAsync ();
            Assert.NotEmpty (m);
            output.WriteLine (string.Join (",", m));
        }

        [Fact]
        public async Task TestGetContextAsync () {
            var contextFactory = this.factory.GetService<ISerieContextFactory> ();
            var context = await contextFactory.GetContextAsync ("Trace");
            Assert.NotNull (context);
            await context.InitSerieAsync ();
            var fileds = context.Fields;
            Assert.NotEmpty (fileds);
        }
    }
}