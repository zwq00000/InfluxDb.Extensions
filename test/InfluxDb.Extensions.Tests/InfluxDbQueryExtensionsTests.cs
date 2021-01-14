using System;
using Xunit;
using Xunit.Abstractions;

namespace InfluxDb.Extensions.Tests {
    public class InfluxDbQueryExtensionsTests {
        private readonly ITestOutputHelper output;

        public InfluxDbQueryExtensionsTests (ITestOutputHelper outputHelper) {
            this.output = outputHelper;
        }

        [Fact]
        public void TestTimeToWhereClause () {
            var time = DateTime.Now;
            var whereClause = time.ToStartTimeWhereClause ();
            Assert.Equal ($"time >= '{time.ToRfc3339()}'", whereClause);

            var now = DateTime.Now;
            whereClause = time.ToWhereClause (now);
            Assert.Equal ($"time >= '{time.ToRfc3339()}' AND time < '{now.ToRfc3339()}'", whereClause);

            whereClause = time.ToWhereClause ();
            Assert.Equal ($"time >= '{time.ToRfc3339()}'", whereClause);
        }

        [Fact]
        public void TestToDuration () {

            var dur = TimeSpan.FromMilliseconds (100);
            Assert.Equal ("100ms", dur.ToDuration ());

            dur = TimeSpan.FromSeconds (1);
            Assert.Equal ("1s", dur.ToDuration ());

            dur = TimeSpan.FromMinutes (3);
            Assert.Equal ("3m", dur.ToDuration ());

            dur = TimeSpan.FromHours (1);
            Assert.Equal ("1h", dur.ToDuration ());

            dur = TimeSpan.FromDays (1);
            Assert.Equal ("1d", dur.ToDuration ());

            dur = TimeSpan.FromDays (7);
            Assert.Equal ("1w", dur.ToDuration ());

            dur = TimeSpan.FromDays (14);
            Assert.Equal ("2w", dur.ToDuration ());

            dur = TimeSpan.FromDays (13);
            Assert.Equal ("13d", dur.ToDuration ());

        }
    }
}