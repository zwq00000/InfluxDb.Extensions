using System;
using System.Collections.Generic;
using InfluxData.Net.InfluxDb.Models.Responses;

namespace InfluxDb.Extensions.Tests {
    internal static class TestHelper {
        public static Serie GetSerie (int count = 1000) {

            var values = new List<IList<object>> ();
            for (int i = 0; i < count; i++) {
                values.Add (GetObjs (i));
            }
            var tags = new Dictionary<string, string> ();
            tags.Add (nameof (TestModel.Type), MockType.Type1.ToString ());

            return new Serie () {
                Columns = new string[] { "id", "name", "doubleVal", "time" },
                    Tags = tags,
                    Values = values
            };

            IList<object> GetObjs (int i) {
                return new List<object> (new object[] { i, "test", i + 1d, DateTime.Now });
            }
        }
    }
}