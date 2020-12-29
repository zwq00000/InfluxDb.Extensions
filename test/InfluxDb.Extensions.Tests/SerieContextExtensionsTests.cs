using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using InfluxData.Net.InfluxDb.Models.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace InfluxDb.Extensions.Tests {
    public class SerieContextExtensionsTests {
        private readonly TestFactory factory;
        private readonly ITestOutputHelper output;
        private readonly TaskPerformance prof;

        public SerieContextExtensionsTests (ITestOutputHelper outputHelper) {
            this.factory = new TestFactory ();
            this.output = outputHelper;
            this.prof = new TaskPerformance (output);
        }

        [Fact]
        public async Task TestToPageAsync () {
            var contextFactory = this.factory.GetService<ISerieContextFactory> ();
            var context = await contextFactory.GetContextAsync ("ShipTrack");
            Assert.NotNull (context);
            var startTime = DateTime.Now.AddDays (-1);
            var sql = context.BuildQuery ().Where (startTime.ToWhereClause ());
            var result = await sql.ToPageSeriesAsync (context, 2, 20);
            var stream = new StringWriter ();
            await result.WriteToJsonAsync (new JsonTextWriter (stream));
            output.WriteLine (stream.ToString ());

        }

        [Fact]
        public async Task TestToPageJsonAsync () {
            var contextFactory = this.factory.GetService<ISerieContextFactory> ();
            var context = await contextFactory.GetContextAsync ("ShipTrack");
            Assert.NotNull (context);
            var startTime = DateTime.Now.AddDays (-1);
            var sql = context.BuildQuery ().Where (startTime.ToWhereClause ());
            var result = await sql.ToPageSeriesAsync (context, 2, 20);
            var namingStrategy = new CamelCaseNamingStrategy ();
            var stream = new StringWriter ();
            await result.WriteToJsonAsync (new JsonTextWriter (stream) {
                Formatting = Formatting.Indented
            }, namingStrategy);
            output.WriteLine (stream.ToString ());
        }

        [Fact]
        public async Task TestToPageJsonWithTagsAsync () {
            var contextFactory = this.factory.GetService<ISerieContextFactory> ();
            var context = await contextFactory.GetContextAsync ("ShipTrack");
            Assert.NotNull (context);

            PageResult<Serie> result = null;
            var startTime = DateTime.Now.AddDays (-1);
            var sql = context.BuildQuery ("MMSI").Where (startTime.ToWhereClause ());
            using (prof.NewExec ("Influx 查询")) {
                for (int i = 0; i < 10; i++) {
                    result = await sql.ToPageSeriesAsync (context, 2, 20);
                }
            }

            var stream = new StringWriter ();
            using (prof.NewExec ("Json 序列化")) {
                var namingStrategy = new CamelCaseNamingStrategy ();
                await result.WriteToJsonAsync (new JsonTextWriter (stream) {
                    Formatting = Formatting.Indented
                }, namingStrategy);
            }

            var deserializeObject = JsonConvert.DeserializeObject (stream.ToString ());
            Assert.NotNull (deserializeObject);
            Assert.IsType<JObject> (deserializeObject);
        }

    }

    public class TaskPerformance : IDisposable {
        private readonly ITestOutputHelper output;
        private readonly string taskName;
        private readonly DateTime start;

        public TaskPerformance (ITestOutputHelper outputHelper) {
            this.output = outputHelper;
        }

        public TaskPerformance (ITestOutputHelper outputHelper, string taskName) {
            this.output = outputHelper;
            this.taskName = taskName;
            output.WriteLine ($"start task {taskName}");
            this.start = DateTime.Now;
        }

        public IDisposable NewExec (string name) {
            return new TaskPerformance (this.output, name);
        }

        public void Dispose () {
            output.WriteLine ($"end task {taskName} with time {DateTime.Now-start}");
        }
    }
}