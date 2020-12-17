using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxData.Net.InfluxDb.Models.Responses;

namespace InfluxDb.Extensions {
    /// <summary>
    /// Influx Serial Query 扩展方法
    /// </summary>
    internal static class SerieContextExtensions {
        /// <summary>
        /// 构建查询
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static SqlBuilder BuildQuery (this SerieContext context, params string[] tags) {
            context.EnsureInit ();

            IEnumerable<string> fields;
            if (tags.Length > 0) {
                fields = tags.Concat (context.Fields);
            } else {
                fields = new [] { "*" };
            }
            return new SqlBuilder (fields, context.Measurement)
                .TimeZone (context.TimeZone);
        }

        public static SqlBuilder BuildMeanQuery (this SerieContext context, TimeSpan interval) {
            context.EnsureInit ();
            return new SqlBuilder (context.Fields.Select (f => $"MEAN({f}) AS {f}"), context.Measurement)
                .GroupBy (context.BuildTimeGroup (interval))
                .TimeZone (context.TimeZone);
        }

        public static SqlBuilder BuildMutlTagsQuery (this SerieContext context, string whereClause, params string[] tags) {
            if (string.IsNullOrEmpty (whereClause)) {
                throw new ArgumentNullException (nameof (whereClause));
            }
            return new SqlBuilder ().Select ($"Count({context.FirstField})")
                .From (context.Measurement)
                .Where (whereClause)
                .GroupBy (tags)
                .TimeZone (context.TimeZone);
        }

        private static string BuildTimeGroup (this SerieContext context, TimeSpan interval) {
            if (interval.TotalSeconds <= 1) {
                return "time(1s)";
            }
            return $"time({interval.TotalSeconds:F0}s)";
        }

        /// <summary>
        /// 计算数量
        /// </summary>
        /// <param name="sqlBuilder"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<int> GetCountAsync (this SqlBuilder sqlBuilder, SerieContext context) {
            var sql = sqlBuilder.ToCount ();
            var series = await context.QueryAsync (sql);
            var serie = series.FirstOrDefault ();
            if(serie==null){
                return 0;
            }
            var value = serie.Values.FirstOrDefault();
            if(value==null){
                return 0;
            }
            return (int) Convert.ChangeType (value[1] ?? 0, TypeCode.Int32);
        }

        /// <summary>
        /// 计算数量
        /// </summary>
        /// <param name="sqlBuilder"></param>
        /// <param name="context"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Serie>> ToPageSeriesAsync (this SqlBuilder sqlBuilder, SerieContext context,int page=1,int pageSize = 100) {
            if(page<1){
                page = 1;
            }
            if(pageSize<1){
                pageSize = 10;
            }
            var count = await sqlBuilder.GetCountAsync(context);
            var sql = sqlBuilder.ToLimitAndOffset(pageSize,(page-1)*pageSize);
            return await context.QueryAsync(sql);
        }

                /// <summary>
        /// 计算数量
        /// </summary>
        /// <param name="sqlBuilder"></param>
        /// <param name="context"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static async Task<PageResult<Serie>> ToPageSeriesAsync (this SqlBuilder sqlBuilder, SerieContext context,Paging paging = default) {
            if(paging==null){
                paging = new Paging();
            }
            var count = await sqlBuilder.GetCountAsync(context);
            paging.Total = count;
            var sql = sqlBuilder.ToLimitAndOffset(paging.PageSize,(paging.Page-1)*paging.PageSize);
            var values = await context.QueryAsync(sql);
            return new PageResult<Serie>(paging,values);
        }
    }
}