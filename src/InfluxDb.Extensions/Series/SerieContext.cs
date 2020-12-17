using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.Models.Responses;
using Microsoft.Extensions.Logging;

namespace InfluxDb.Extensions {

    /// <summary>
    /// Serie query Context
    /// </summary>
    public class SerieContext : ISerieContext {
        private IInfluxDbClient _client;
        private readonly ILogger _logger;

        public SerieContext (InfluxDbOptions options, string measurement, ILogger logger) {
            this.Database = options.Database;
            this.Measurement = measurement;
            this.TimeZone = options.TimeZone;
            this._client = options.CreateNewClient ();
            this._logger = logger;
        }

        public SerieContext(IInfluxDbClient client, string database, string measurement, string timeZone, IEnumerable<string> fields, IEnumerable<string> tags, ILogger logger)
        {
            _client = client;
            _logger = logger;
            Database = database;
            Measurement = measurement;
            TimeZone = timeZone;
            Fields = fields;
            Tags = tags;
            FirstField = fields.FirstOrDefault();
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
        /// initialize SerieContext ,fill Fields and Tags
        /// </summary>
        /// <returns></returns>
        public async Task InitSerieAsync () {
            this.Fields = await GetFieldsAsync ();
            this.Tags = (await GetTagKeysAsync ()).ToArray ();
            FirstField = Fields.First ();
        }

        public void EnsureInit () {
            if (Fields == null || !Fields.Any ()) {
                InitSerieAsync ().Wait ();
            }
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
        /// 确认数据库是否被创建
        /// </summary>
        /// <returns></returns>
        public async Task EnsureCreated () {
            if (!await ValidateDatabase ()) {
                await _client.Database.CreateDatabaseAsync (Database);
            }
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

        public async Task<IEnumerable<Serie>> QueryAsync (string sql) {
            _logger.LogTrace ("influx database '{database}' exec\n {sql}", Database, sql);
            return await _client.Client.QueryAsync (sql, Database);
        }

        public async Task<IEnumerable<Serie>> QueryAsync (SqlBuilder sqlBuilder) {
            return await _client.Client.QueryAsync (sqlBuilder.ToString (), Database);
        }

    }
}