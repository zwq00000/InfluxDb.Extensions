using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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

        private async Task<ISerieContext> GetContextAsync () {
            var contextFactory = this.factory.GetService<ISerieContextFactory> ();
            var context = await contextFactory.GetContextAsync ("ShipTrack");
            Assert.NotNull (context);
            return context;
        }

        [Fact]
        public async Task TestToPageAsync () {
            var context = await GetContextAsync ();
            var startTime = DateTime.Now.AddHours (-1);
            var sql = context.BuildQuery ().Start (startTime);
            for (int i = 0; i < 10; i++) {
                using (prof.NewExec ("Influx 分页查询 " + i)) {
                    var result = await sql.ToPageSeriesAsync (context, i, 1000);
                    output.WriteLine ($"Query {i} Count:{result.Page}");
                }
            }

            using (prof.NewExec ("*** Influx 不分页查询")) {
                var result = await context.QueryAsync (sql);
                output.WriteLine ($"Query Count:{result.Sum(c=>c.Values.Count())}");
            }

        }

        [Fact]
        public async Task TestToPageJsonAsync () {
            var context = await GetContextAsync ();
            var startTime = DateTime.Now.AddDays (-1);
            var sql = context.BuildQuery ("MMSI").Start (startTime);
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
            var context = await GetContextAsync ();

            PageResult<Serie> result = null;
            var startTime = DateTime.Now.AddDays (-1);
            var sql = context.BuildQuery ("MMSI").Start (startTime);
            for (int i = 0; i < 10; i++) {
                using (prof.NewExec ("Influx 查询")) {
                    result = await sql.ToPageSeriesAsync (context, i, 200);
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

        [Fact]
        public async Task TestWriteToJsonUtf8Async () {
            var context = await GetContextAsync ();

            var startTime = DateTime.Now.AddHours (-1);
            var sql = context.BuildQuery ().Start (startTime).ToLimitAndOffset (1000, 0);
            IEnumerable<Serie> result;
            using (prof.NewExec ("Influx 查询")) {
                result = await context.QueryAsync (sql);
            }
            var namingStrategy = new CamelCaseNamingStrategy ();

            var stream = new MemoryStream ();
            using (prof.NewExec ("Json 序列化")) {
                var writer = new Utf8JsonWriter (stream);
                result.First ().WriteToJsonUtf8 (writer);
                await writer.FlushAsync ();
            }
            stream.Position = 0;
            var reader = new StreamReader (stream);
            var jsonStr = reader.ReadToEnd ();
            var deserializeObject = JsonConvert.DeserializeObject (jsonStr);
            Assert.NotNull (deserializeObject);
            // Assert.IsType<JObject> (deserializeObject);

            output.WriteLine (jsonStr);
        }

        private Serie GetSerie (int count = 1000) {

            var values = new List<IList<object>> ();
            for (int i = 0; i < count; i++) {
                values.Add (GetObjs (i));
            }

            return new Serie () {
                Columns = new string[] { "id", "name", "doubleVal", "time" },
                    Values = values
            };

            IList<object> GetObjs (int i) {
                return new List<object> (new object[] { i, "test", i + 1d, DateTime.Now });
            }
        }

        [Fact]
        public async Task TestJsonWriterProformAsync () {
            Serie serie = GetSerie ();
            var namingStrategy = new CamelCaseNamingStrategy ();

            using (prof.NewExec ("json Utf8 序列化")) {
                var buffer = new PooledByteBufferWriter (16 * 1024);
                var writer = new Utf8JsonWriter (buffer);
                serie.WriteToJsonUtf8 (writer);
                await writer.FlushAsync ();
            }

            using (prof.NewExec ("Newtonsoft.Json 异步序列化")) {
                var stream = new StringWriter ();
                var writer = new JsonTextWriter (stream);
                await serie.WriteToJsonAsync (writer, namingStrategy).ConfigureAwait (true);
                await writer.FlushAsync ();
            }

            using (prof.NewExec ("Newtonsoft.Json 同步序列化")) {
                var stream = new StringWriter ();
                var writer = new JsonTextWriter (stream);
                serie.WriteToJson (writer, namingStrategy);
                writer.Flush ();
            }
        }

        [Fact]
        public void TestConvertProform () {
            const int count = 10000;
            IList<object> list = new List<object> ();
            double[] array = new double[count];
            for (var i = 0; i < count; i++) {
                list.Add (i + 1d);
            }
            using (prof.NewExec ("隐式 类型转换")) {
                for (var i = 0; i < list.Count; i++) {
                    array[i] = (double) list[i];
                }
            }

            using (prof.NewExec ("convert 类型转换")) {
                foreach (var item in list) {
                    for (var i = 0; i < list.Count; i++) {
                        array[i] = Convert.ToDouble (list[i]);
                    }
                }
            }
        }

    }

    public class TestMock {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime Time { get; set; }
    }

    internal static class TestHelper {
        public static Serie GetSerie (int count = 1000) {

            var values = new List<IList<object>> ();
            for (int i = 0; i < count; i++) {
                values.Add (GetObjs (i));
            }

            return new Serie () {
                Columns = new string[] { "id", "name", "doubleVal", "time" },
                    Values = values
            };

            IList<object> GetObjs (int i) {
                return new List<object> (new object[] { i, "test", i + 1d, DateTime.Now });
            }
        }

    }
}