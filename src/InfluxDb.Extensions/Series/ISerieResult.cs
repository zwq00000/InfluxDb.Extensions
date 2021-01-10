using System.Collections.Generic;
using InfluxData.Net.InfluxDb.Models.Responses;

namespace InfluxDb.Extensions
{
    public interface ISerieResult<TModel>
    {
        /// <summary>
        /// 从 Influx QueryResult 转换为实体对象类型
        /// </summary>
        /// <param name="series"></param>
        /// <returns></returns>
        IEnumerable<TModel> ToResult(IEnumerable<Serie> series);
    }
}