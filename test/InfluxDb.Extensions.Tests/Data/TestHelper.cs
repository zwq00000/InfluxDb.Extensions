using System;
using System.Collections.Generic;
using InfluxData.Net.InfluxDb.Models.Responses;
using Newtonsoft.Json;

namespace InfluxDb.Extensions.Tests {
    internal static class TestHelper {
        public static Serie GetSerie (int count = 1000) {

            var values = new List<IList<object>> ();
            for (int i = 0; i < count; i++) {
                var model = new TestModel () {
                    Id = i,
                    Name = "TEST",
                    FloatValue = i + 0.1f,
                    DoubleValue = i + 0.2d,
                    LongValue = i * 2,
                    IntValue = i * 3,
                    Time = DateTime.Now,
                };
                values.Add (GetInstance (model));
            }
            var tags = new Dictionary<string, string> ();
            tags.Add (nameof (TestModel.Type), MockType.Type1.ToString ());

            return new Serie () {
                Columns = new string[] {
                        "time",
                        "Id",
                        "Name",
                        "Time",
                        "FloatValue",
                        "DoubleValue",
                        "LongValue",
                        "IntValue",
                        "ShortValue",
                        },
                        Tags = tags,
                        Values = values
            };

            IList<object> GetInstance (TestModel model) {
                var values = new object[] {
                    model.Id,
                    model.Name,
                    model.Time,
                    model.FloatValue,
                    model.DoubleValue,
                    model.LongValue,
                    model.IntValue,
                    model.ShortValue
                };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject (values);
                return JsonConvert.DeserializeObject<object[]> (json);
            }
        }

        /// <summary>
        /// 生成指定日期 当日范围的 Where 子句, [date.Date,date.Date.AddDays(1)]
        /// </summary>
        /// <param name="date"></param>
        /// <param name="wholeDay">一整天,从当日0点开始</param>
        /// <returns></returns>
        public static string ToDateWhereClause (this DateTime date, bool wholeDay = true) {
            if (wholeDay) {
                return date.Date.ToWhereClause (date.Date.AddDays (1));
            } else {
                return date.ToWhereClause (date.Date.AddDays (1));
            }
        }

        /// <summary>
        /// 生成指定开始时间的 时间条件子句
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public static string ToStartTimeWhereClause (this DateTime start) {
            return $"time >= '{start.ToRfc3339()}'";
        }

        /// <summary>
        /// 生成指定时间和范围的 Where 子句, [date,date+offset)
        /// </summary>
        /// <param name="date"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string ToWhereClause (this DateTime date, TimeSpan offset) {
            return date.ToWhereClause (date + offset);
        }

        /// <summary>
        /// 生成日期范围 Where 子句
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static string ToWhereClause (this DateTime start, DateTime? end = null) {

            if (!end.HasValue) {
                var time = DateTime.Now - start;
                return $"time >= '{start.ToRfc3339()}'";
            }
            var endtime = end.Value;
            if (start > end) {
                var _ = start;
                start = endtime;
                end = _;
            }
            return $"time >= '{start.ToRfc3339()}' AND time < '{endtime.ToRfc3339()}'";
        }

        /// <summary>
        /// 生成 最近时间范围 Where 子句
        /// 'time >= now() - ns'
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToLastWhereClause (this TimeSpan time) {
            return $"time >= now() - {time.TotalSeconds}s";
        }

    }
}