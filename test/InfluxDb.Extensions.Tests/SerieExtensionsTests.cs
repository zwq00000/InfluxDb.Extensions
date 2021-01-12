using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace InfluxDb.Extensions.Tests {
    public class SerieExtensionsTests {
        private readonly ITestOutputHelper output;
        private readonly TaskPerformance prof;

        public SerieExtensionsTests (ITestOutputHelper outputHelper) {
            this.output = outputHelper;
            this.prof = new TaskPerformance (output);
        }

        [Fact]
        public void TestPropertyCache () {
            var properties = PropertiesCache<TestMock>.GetProperties ();
            Assert.NotEmpty (properties);
            Assert.Equal (3, properties.Count ());

            var setters = PropertiesCache<TestMock>.GetSetters ();

            Assert.NotEmpty (setters);
            var item = new TestMock ();
            setters["Id"] (item, 1);
            setters["Name"] (item, "TEST");
            setters["Time"] (item, DateTime.Now);
            Assert.Equal (1, item.Id);
        }

        [Fact]
        public void TestAs () {
            const int TestCount = 10000;
            var setters = PropertiesCache<TestMock>.GetSetters ();
            var serie = TestHelper.GetSerie (TestCount);
            using (prof.NewExec ("Serie to Instance")) {
                var entities = serie.As<TestMock> ();
                entities.ToArray ();
                Assert.NotEmpty (entities);
                Assert.Equal (TestCount, entities.Count ());
            }
        }
    }
}