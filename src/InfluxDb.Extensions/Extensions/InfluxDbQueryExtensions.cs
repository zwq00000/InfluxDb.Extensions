using System;
using System.Collections.Generic;
using System.Text;
using InfluxData.Net.InfluxDb.Models.Responses;
using static System.Convert;

namespace InfluxDb.Extensions {
    /// <summary>
    /// Infflux DB 数据查询扩展方法
    /// </summary>
    internal static class InfluxDbQueryExtensions {
       
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
        public static string ToDuration (this TimeSpan time) {
            var builder = new StringBuilder ();
            if (time.Days > 0) {
                builder.Append ($"{time.Days}d");
            }
            if (time.Hours > 0) {
                builder.Append ($"{time.Hours}h");
            }

            if (time.Minutes > 0) {
                builder.Append ($"{time.Minutes}m");
            }
            if (time.Seconds > 0) {
                builder.Append ($"{time.Seconds}s");
            }
            if (time.Milliseconds > 0) {
                builder.Append ($"{time.Milliseconds}ms");
            }
            if (time.Ticks > 0) {
                builder.Append ($"{time.Milliseconds}ms");
            }
            return builder.ToString ();
        }

        private static int GetColumnIndex (this IList<string> list, string name) {
            for (int i = 0; i < list.Count; i++) {
                if (string.Equals (list[i], name, StringComparison.CurrentCultureIgnoreCase)) {
                    return i;
                }
            }
            return -1;
        }

        // public static IEnumerable<TValue> ToSeriesValues<TValue> (this IEnumerable<Serie> series, string columnName, TypeCode typeCode) {
        //     if (series == null) {
        //         throw new ArgumentNullException (nameof (series));
        //     }
        //     foreach (var s in series) {
        //         var valueColume = s.Columns.GetColumnIndex (columnName);
        //         foreach (var list in s.Values) {
        //             var value = list[valueColume];
        //             if (value != null) {
        //                 yield return (TValue) ChangeType (value, typeCode);
        //             }
        //         };
        //     }
        // }

        // const string TimeColumnName = "Time";

        // public static IEnumerable<Segment> ToSeriesCounts (this IEnumerable<Serie> series, TimeSpan duration, string countColumnName = "COUNT") {
        //     if (series == null) {
        //         throw new ArgumentNullException (nameof (series));
        //     }
        //     foreach (var s in series) {
        //         var timeIndex = s.Columns.GetColumnIndex (TimeColumnName);
        //         var countIndex = s.Columns.GetColumnIndex (countColumnName);
        //         foreach (var value in s.Values) {
        //             var count = (int) ChangeType (value[countIndex] ?? 0, TypeCode.Int32);
        //             if (count > 0) {
        //                 var time = ((DateTime) value[timeIndex]);
        //                 yield return new Segment () {
        //                     Start = time,
        //                         End = time + duration,
        //                         Count = count
        //                 };
        //             }
        //         };
        //     }
        // }
    }
}