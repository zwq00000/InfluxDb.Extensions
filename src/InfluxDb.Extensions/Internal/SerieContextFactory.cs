using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxData.Net.InfluxDb;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfluxDb.Extensions {
    internal class SerieContextFactory : ISerieContextFactory {
        private readonly IOptions<InfluxDbOptions> _options;
        private readonly IMemoryCache _cache;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IInfluxDbClient _client;
        private readonly ILogger<SerieContextFactory> _logger;

        public SerieContextFactory (IOptions<InfluxDbOptions> options, IMemoryCache cache, ILoggerFactory loggerFactory) {
            this._options = options;
            this._cache = cache;
            this._loggerFactory = loggerFactory;
            this._logger = loggerFactory.CreateLogger<SerieContextFactory> ();
            this._client = _options.Value.CreateNewClient ();
        }

        /// <summary>
        /// Serie Context used database name
        /// </summary>
        /// <value></value>
        public string Database { get => _options.Value.Database; }

        public IInfluxDbClient Client => _client;

        /// <summary>
        /// 确认数据库是否已经创建
        /// </summary>
        /// <returns></returns>
        public async Task<bool> EnsureCreatedAsync () {
            var databases = await _client.Database.GetDatabasesAsync ();
            var isExist = databases.Any (d => string.Equals (d.Name, Database, System.StringComparison.CurrentCultureIgnoreCase));
            if (!isExist) {
                var result = await _client.Database.CreateDatabaseAsync (Database);
                _logger.LogInformation ("Create Influx database {dbname} {state},result {result} ", Database, result.Success, result.Body);
                return result.Success;
            }
            return true;
        }

        /// <summary>
        /// 获取 当前数据库 Measurement 集合
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetMeasurementsAsync () {
            return await _cache.GetOrCreateAsync ($"Influx_{Database}:Measurements", async e => {
                e.SetAbsoluteExpiration (TimeSpan.FromHours (1));
                var result = await _client.Serie.GetMeasurementsAsync (_options.Value.Database);
                return result.Select (m => m.Name);
            });
        }

        public async Task<ISerieContext> GetContextAsync (string measurement) {
            var measurements = await GetMeasurementsAsync ();
            var matched = measurements.FirstOrDefault (m => string.Equals (m, measurement, StringComparison.CurrentCultureIgnoreCase));
            if (matched == null) {
                throw new ArgumentException (nameof (measurement), $"Not Found Measurement '{measurement}'");
            }
            var info = await GetMeasurementInfoAsync(matched);
            return new SerieContext (_client, info, _loggerFactory.CreateLogger<SerieContext> ());
        }

        private async Task<MeasurementInfo> GetMeasurementInfoAsync (string measurement) {
            var cacheKey = $"Influx_{Database}:{measurement}:Info";
            return await _cache.GetOrCreateAsync (cacheKey, async e => {
                e.SetAbsoluteExpiration (TimeSpan.FromMinutes (10));
                var fieldKeys = await _client.Serie.GetFieldKeysAsync (Database, measurement);
                var fields = fieldKeys.Select (k => k.Name).ToArray ();
                var tags = await _client.Serie.GetTagKeysAsync (Database, measurement);
                return new MeasurementInfo (Database, measurement, _options.Value.TimeZone, fields, tags);
            });
        }
    }
}