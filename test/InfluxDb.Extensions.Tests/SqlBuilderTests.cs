using Xunit;
using Xunit.Abstractions;

namespace InfluxDb.Extensions.Tests {
    public class SqlBuilderTests {
        private readonly ITestOutputHelper output;

        public SqlBuilderTests (ITestOutputHelper outputHelper) {
            this.output = outputHelper;
        }

        [Fact]
        public void TestToString () {
            var builder = new SqlBuilder ();
            builder.From ("TEST.autogen");
            output.WriteLine (builder.ToString ());

            builder.Select ("lat", "lng");
            output.WriteLine (builder.ToString ());

            builder.TimeZone ("Asia/Shanghai");
            output.WriteLine (builder.ToString ());
        }

    }
}