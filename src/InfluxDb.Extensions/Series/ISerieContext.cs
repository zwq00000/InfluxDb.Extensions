using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxData.Net.InfluxDb.Models.Responses;

namespace InfluxDb.Extensions
{

    /// <summary>
    /// Influx Measurement Serie Context
    /// </summary>
    public interface ISerieContext {
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
        /// ensure influxdb database and measurement created
        /// </summary>
        /// <returns></returns>
        Task EnsureCreated ();

        /// <summary>
        /// ensure <c cref="SerieContext" /> initialized
        /// </summary>
        void EnsureInit ();

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

    /// <summary>
    /// Seire Context Factory
    /// </summary>
    public interface ISerieContextFactory {

        /// <summary>
        /// get <see cref="ISerieContext" /> by Measurement name
        /// </summary>
        /// <param name="measurement"></param>
        /// <returns></returns>
        ISerieContext GetContext (string measurement);

    }
}