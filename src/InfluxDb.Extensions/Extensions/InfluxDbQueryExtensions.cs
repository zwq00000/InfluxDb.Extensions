using System;
using System.Collections.Generic;
using System.Text;
using InfluxData.Net.InfluxDb.Models.Responses;
using static System.Convert;

namespace InfluxDb.Extensions
{
    /// <summary>
    /// Infflux DB 数据查询扩展方法
    /// </summary>
    public static class InfluxDbQueryExtensions
    {
        /// <summary>
        /// 生成指定日期 当日范围的 Where 子句, [date.Date,date.Date.AddDays(1)]
        /// </summary>
        /// <param name="date"></param>
        /// <param name="wholeDay">一整天,从当日0点开始</param>
        /// <returns></returns>
        public static string ToDateWhereClause(this DateTime date, bool wholeDay = true)
        {
            if (wholeDay)
            {
                return date.Date.ToWhereClause(date.Date.AddDays(1));
            }
            else
            {
                return date.ToWhereClause(date.Date.AddDays(1));
            }
        }

        /// <summary>
        /// 生成指定开始时间的 时间条件子句
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public static string ToStartTimeWhereClause(this DateTime start)
        {
            return $"time >= '{start.ToRfc3339()}'";
        }

        /// <summary>
        /// 生成指定时间和范围的 Where 子句, [date,date+offset)
        /// </summary>
        /// <param name="date"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static string ToWhereClause(this DateTime date, TimeSpan offset)
        {
            return date.ToWhereClause(date + offset);
        }

        /// <summary>
        /// 生成日期范围 Where 子句
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static string ToWhereClause(this DateTime start, DateTime? end = null)
        {

            if (!end.HasValue)
            {
                var time = DateTime.Now - start;
                return $"time >= '{start.ToRfc3339()}'";
            }
            var endtime = end.Value;
            if (start > end)
            {
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
        public static string ToLastWhereClause(this TimeSpan time)
        {
            return $"time >= now() - {time.TotalSeconds}s";
        }

        /// <summary>
        /// 持续时间字面量指定时间长度。紧跟着（无空格）后跟下面列出的持续时间单位的整数文字将被解释为持续时间文字。
        /// 持续时间可以混合单位指定。
        /// </summary>
        /// <param name="time"></param>
        /// <remarks>
        /// duration_lit        = int_lit duration_unit .
        /// duration_unit       = "ns" | "u" | "µ" | "ms" | "s" | "m" | "h" | "d" | "w" .
        /// </remarks>
        /// <returns></returns>
        public static string ToDuration(this TimeSpan time)
        {
            var builder = new StringBuilder();
            if (time.Days > 0)
            {
                builder.Append($"{time.Days}d");
            }
            if (time.Hours > 0)
            {
                builder.Append($"{time.Hours}h");
            }

            if (time.Minutes > 0)
            {
                builder.Append($"{time.Minutes}m");
            }
            if (time.Seconds > 0)
            {
                builder.Append($"{time.Seconds}s");
            }
            if (time.Milliseconds > 0)
            {
                builder.Append($"{time.Milliseconds}ms");
            }
            if (time.Ticks > 0)
            {
                builder.Append($"{time.Milliseconds}ms");
            }
            return builder.ToString();
        }

        private static int GetColumnIndex(this IList<string> list, string name)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i], name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        public static IEnumerable<TValue> ToSeriesValues<TValue>(this IEnumerable<Serie> series, string columnName, TypeCode typeCode)
        {
            if (series == null)
            {
                throw new ArgumentNullException(nameof(series));
            }
            foreach (var s in series)
            {
                var valueColume = s.Columns.GetColumnIndex(columnName);
                foreach (var list in s.Values)
                {
                    var value = list[valueColume];
                    if (value != null)
                    {
                        yield return (TValue)ChangeType(value, typeCode);
                    }
                };
            }
        }

        const string TimeColumnName = "Time";

        public static IEnumerable<Segment> ToSeriesCounts(this IEnumerable<Serie> series, TimeSpan duration, string countColumnName = "COUNT")
        {
            if (series == null)
            {
                throw new ArgumentNullException(nameof(series));
            }
            foreach (var s in series)
            {
                var timeIndex = s.Columns.GetColumnIndex(TimeColumnName);
                var countIndex = s.Columns.GetColumnIndex(countColumnName);
                foreach (var value in s.Values)
                {
                    var count = (int)ChangeType(value[countIndex] ?? 0, TypeCode.Int32);
                    if (count > 0)
                    {
                        var time = ((DateTime)value[timeIndex]);
                        yield return new Segment()
                        {
                            Start = time,
                            End = time + duration,
                            Count = count
                        };
                    }
                };
            }
        }
    }
}