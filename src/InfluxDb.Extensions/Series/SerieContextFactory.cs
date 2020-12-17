using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfluxDb.Extensions {
    internal class SerieContextFactory : ISerieContextFactory {
        private readonly IOptions<InfluxDbOptions> _options;
        private readonly ILoggerFactory _loggerFactory;
        private readonly InfluxData.Net.InfluxDb.IInfluxDbClient _client;
        private readonly ILogger<SerieContextFactory> _logger;

        public SerieContextFactory (IOptions<InfluxDbOptions> options, IMemoryCache cache, ILoggerFactory loggerFactory) {
            this._options = options;
            this._loggerFactory = loggerFactory;
            this._client = _options.Value.CreateNewClient ();
        }

        public string Database { get => _options.Value.Database; }

        /// <summary>
        /// 确认数据库是否已经创建
        /// </summary>
        /// <returns></returns>
        public async Task<bool> EnsureDatabaseCreatedAsync () {
            var databases = await _client.Database.GetDatabasesAsync ();
            var isExist = databases.Any (d => string.Equals (d.Name, Database, System.StringComparison.CurrentCultureIgnoreCase));
            if (!isExist) {
                var result = await _client.Database.CreateDatabaseAsync (Database);
                _logger.LogInformation ("Create Influx database {dbname} {state},result {result} ", Database, result.Success, result.Body);
                return result.Success;
            }
            return true;
        }

        public ISerieContext GetContext (string measurement) {
            return new SerieContext (_options.Value, measurement, _loggerFactory.CreateLogger<SerieContext> ());
        }

        private void CacheContext (string measurement) {

        }
    }
}