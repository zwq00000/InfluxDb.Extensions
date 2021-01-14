using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.Models.Responses;

namespace InfluxDb.Extensions {

    /// <summary>
    /// Influx Measurement Serie Context
    /// </summary>
    public interface ISerieContext {

        /// <summary>
        /// 获取 <see cref="IInfluxDbClient"/>
        /// </summary>
        /// <value></value>
        IInfluxDbClient Client { get; }

        ///<summary>
        /// Influx Database name
        ///</summary>
        string Database { get; }

        /// <summary>
        /// Measurement Name
        /// </summary>
        string Measurement { get; }

        /// <summary>
        /// 默认时区
        /// </summary>
        string TimeZone { get; }

        /// <summary>
        /// 字段列表
        /// </summary>
        IEnumerable<string> Fields { get; }

        /// <summary>
        /// Tag Keys
        /// </summary>
        IEnumerable<string> Tags { get; }

        /// <summary>
        /// First Field,use for Get Count
        /// </summary>
        /// <value></value>
        string FirstField { get; }

        /// <summary>
        /// initialize SerieContext ,fill Fields and Tags
        /// </summary>
        /// <returns></returns>
        Task InitSerieAsync ();

        /// <summary>
        /// Query Serie for influx database/measurement
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        Task<IEnumerable<Serie>> QueryAsync (string sql);

        /// <summary>
        /// Query Serie for influx database/measurement
        /// </summary>
        /// <param name="sqlBuilder"></param>
        /// <returns></returns>
        Task<IEnumerable<Serie>> QueryAsync (SqlBuilder sqlBuilder);
    }
}