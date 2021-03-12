using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using InfluxData.Net.InfluxDb.Helpers;
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
        public void TestPropertySetter () {
            var properties = PropertiesCache<TestModel>.GetProperties ();
            Assert.NotEmpty (properties);
            Assert.Equal (9, properties.Count ());

            var setters = PropertiesCache<TestModel>.GetSetters ();

            Assert.NotEmpty (setters);
            var item = new TestModel ();
            var time = new DateTime (2020, 1, 1);
            setters["Id"] (item, 1);
            setters["Name"] (item, "TEST");
            setters["Time"] (item, time);
            setters[nameof (TestModel.FloatValue)] (item, (object) 1.2d);
            setters["Type"] (item, MockType.Type1.ToString ());

            Assert.Equal (1, item.Id);
            Assert.Equal ("TEST", item.Name);
            Assert.Equal (time, item.Time);
            Assert.Equal (MockType.Type1, item.Type);

        }

        [Fact]
        public void TestEnumParser () {
            Assert.Equal (MockType.Type1, Enum.Parse<MockType> ("Type1"));
            Assert.Equal (MockType.Type1, Enum.Parse<MockType> ("1"));

            GetExp<MockType> (s => Enum.Parse<MockType> (s));
        }

        private void GetExp<TEnum> (Expression<Func<string, TEnum>> expression) {
            output.WriteLine (expression.ToString ());
            var method = typeof (Enum).GetMethod (nameof (Enum.Parse), new [] { typeof (string) }).MakeGenericMethod (typeof (MockType));
            var param_instance = Expression.Parameter (typeof (string));
            var exp = Expression.Call (null, method, param_instance);
            output.WriteLine (exp.ToString ());
        }

        [Fact]
        public void TestAs () {
            const int TestCount = 10000;
            var setters = PropertiesCache<TestModel>.GetSetters ();
            var serie = TestHelper.GetSerie (TestCount);
            TestModel[] entities;
            using (prof.NewExec ("Serie to Instance")) {
                entities = serie.As<TestModel> ().ToArray ();
            }
            Assert.NotEmpty (entities);
            Assert.Equal (TestCount, entities.Count ());
            foreach (var item in entities) {
                Assert.Equal (MockType.Type1, item.Type);
                Assert.Equal ("test", item.Name);
            }
        }

        [Fact]
        public async Task TestShipTrackAsAsync () {
            var factory = new TestFactory ();
            var contextFactory = factory.GetService<ISerieContextFactory> ();
            var context = await contextFactory.GetContextAsync ("ShipTrack");
            var sql = context.BuildQuery ().Start (TimeSpan.FromMinutes (5))
                .GroupBy ("NavStatus");
            var series = await context.QueryAsync (sql);
            Assert.NotEmpty (series);
            var entities = series.As<ShipTrack> ().ToArray ();
            Assert.NotEmpty (entities);
        }

        [Fact]
        public async Task TestCountAsAsync () {
            var factory = new TestFactory ();
            var contextFactory = factory.GetService<ISerieContextFactory> ();
            var context = await contextFactory.GetContextAsync ("ShipTrack");
            var sql = context.BuildQuery ()
                .Select ("Count(Lat) as Count")
                .StartDate(DateTime.Today);

            var series = await context.QueryAsync (sql);
            series.As<SeriesCount> ().Sum (s => s.Count);
        }

        [Fact]
        public async Task TestShipTrackToJsonUtf8Async () {
            var factory = new TestFactory ();
            var contextFactory = factory.GetService<ISerieContextFactory> ();
            var context = await contextFactory.GetContextAsync ("ShipTrack");
            var sql = context.BuildQuery ().Start (TimeSpan.FromMinutes (5))
                .GroupBy ("NavStatus");
            var series = await context.QueryAsync (sql.ToLimitAndOffset (10, 0));
            Assert.NotEmpty (series);

            var writer = new PooledByteBufferWriter (1024 * 64);
            series.WriteToJsonUtf8 (new System.Text.Json.Utf8JsonWriter (writer), null);

            var stream = new MemoryStream ();
            await writer.WriteToStreamAsync (stream, CancellationToken.None);
            stream.Position = 0;
            output.WriteLine (new StreamReader (stream).ReadToEnd ());

        }
    }

    /// <summary>
    /// 序列数量
    /// </summary>
    internal class SeriesCount {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Count
        /// </summary>
        public int Count { get; set; }
    }
}