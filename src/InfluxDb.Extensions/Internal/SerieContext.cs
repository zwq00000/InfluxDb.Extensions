using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxData.Net.Common.Infrastructure;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.ClientModules;
using InfluxData.Net.InfluxDb.Models.Responses;
using Microsoft.Extensions.Logging;

namespace InfluxDb.Extensions {

    /// <summary>
    /// Serie query Context
    /// </summary>
    internal class SerieContext : ISerieContext {
        private IInfluxDbClient _client;
        private readonly ILogger _logger;

        public SerieContext (InfluxDbOptions options, string measurement, ILogger logger) {
            this.Database = options.Database;
            this.Measurement = measurement;
            this.TimeZone = options.TimeZone;
            this._client = options.CreateNewClient ();
            this._logger = logger;
        }

        internal SerieContext (IInfluxDbClient client, MeasurementInfo info, ILogger logger) {
            _client = client;
            _logger = logger;
            Database = info.Database;
            Measurement = info.Measurement;
            TimeZone = info.TimeZone;
            Fields = info.Fields;
            Tags = info.Tags;
            FirstField = Fields.FirstOrDefault ();
        }

        /// <summary>
        /// Influx Database name
        /// </summary>
        public string Database { get; }

        /// <summary>
        /// Measurement Name
        /// </summary>
        public string Measurement { get; }

        /// <summary>
        /// 默认时区
        /// </summary>
        public string TimeZone { get; }

        /// <summary>
        /// 字段列表
        /// </summary>
        public IEnumerable<string> Fields { get; private set; }

        /// <summary>
        /// Tag Keys
        /// </summary>
        public IEnumerable<string> Tags { get; private set; }

        /// <summary>
        /// First Field,use for Get Count
        /// </summary>
        /// <value></value>
        public string FirstField { get; private set; }

        /// <summary>
        /// 获取 <see cref="IInfluxDbClient"/>
        /// </summary>
        /// <value></value>
        public IInfluxDbClient Client { get => _client; }

        /// <summary>
        /// initialize SerieContext ,fill Fields and Tags
        /// </summary>
        /// <returns></returns>
        public async Task InitSerieAsync () {
            this.Fields = await GetFieldsAsync ();
            this.Tags = (await GetTagKeysAsync ()).ToArray ();
            FirstField = Fields.First ();
        }

        /// <summary>
        /// 验证数据库是否存在
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ValidateDatabase () {
            var databases = await _client.Database.GetDatabasesAsync ();
            return databases.Any (d => d.Name == Database);
        }

        /// <summary>
        /// Async Get TagKeys for <see cref="Measurement"/>
        /// </summary>
        /// <returns></returns>
        protected async Task<IEnumerable<string>> GetTagKeysAsync () {
            return await _client.Serie.GetTagKeysAsync (Database, this.Measurement);
        }

        protected async Task<string[]> GetFieldsAsync () {
            var keys = await _client.Serie.GetFieldKeysAsync (Database, Measurement);
            return keys.Select (k => k.Name).ToArray ();
        }

        /// <summary>
        /// 执行sql 语句,返回查询结果
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Serie>> QueryAsync (string sql) {
            _logger.LogTrace ("influx database '{database}' exec\n {sql}", Database, sql);
            return await _client.Client.QueryAsync (sql, Database);
        }

        /// <summary>
        /// 执行sql 语句,返回查询结果
        /// </summary>
        /// <param name="sqlBuilder"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Serie>> QueryAsync (SqlBuilder sqlBuilder) {
            return await _client.Client.QueryAsync (sqlBuilder.ToString (), Database);
        }

        public async Task<IInfluxDataApiResponse> GetQueryAsync(SqlBuilder sqlBuilder){
            return await _client.RequestClient.GetQueryAsync(sqlBuilder.ToString(),Database);
        }

        internal MeasurementInfo ToMeasurementInfo () {
            return new MeasurementInfo (Database, Measurement, TimeZone, Fields, Tags);
        }
    }
}