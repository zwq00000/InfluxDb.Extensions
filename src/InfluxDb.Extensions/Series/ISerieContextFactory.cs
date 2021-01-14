using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxData.Net.InfluxDb;

namespace InfluxDb.Extensions {
    /// <summary>
    /// Seire Context Factory
    /// </summary>
    public interface ISerieContextFactory {

        /// <summary>
        /// get influxdb client
        /// </summary>
        /// <value></value>
        IInfluxDbClient Client { get; }

        /// <summary>
        /// database name
        /// </summary>
        /// <value></value>
        string Database { get; }

        /// <summary>
        /// get <see cref="ISerieContext" /> by Measurement name
        /// </summary>
        /// <param name="measurement"></param>
        /// <exception cref="ArgumentException">Not Found <paramref name="measurement"/></exception>
        /// <returns></returns>
        Task<ISerieContext> GetContextAsync (string measurement);

        /// <summary>
        /// 获取 当前数据库 Measurement 集合
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<string>> GetMeasurementsAsync ();

        /// <summary>
        /// 确认数据库是否已经创建
        /// </summary>
        /// <returns></returns>
        Task<bool> EnsureCreatedAsync ();

    }
}