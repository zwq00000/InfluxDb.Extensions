using InfluxData.Net.Common.Enums;
using InfluxData.Net.InfluxDb;
using Microsoft.Extensions.Options;

namespace InfluxDb.Extensions {
    public class InfluxDbOptions : IOptions<InfluxDbOptions> {

        /// <summary>
        /// 连接地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; } = "";

        /// <summary>
        /// 用户密码
        /// </summary>
        public string Password { get; set; } = "";

        /// <summary>
        /// 时区
        /// </summary>
        public string TimeZone { get; set; } = "Asia/Shanghai";

        #region Implementation of IOptions<out InfluxDbOptions>

        /// <summary>
        /// The default configured TOptions instance, equivalent to Get(string.Empty).
        /// </summary>
        public InfluxDbOptions Value {
            get {
                return this;
            }
        }

        #endregion

        /// <summary>
        /// 新建 <see cref="InfluxDbClient"/>
        /// </summary>
        /// <returns></returns>
        public IInfluxDbClient CreateNewClient () {
            return new InfluxDbClient (endpointUri: Url,
                username: Username,
                password: Password,
                influxVersion: InfluxDbVersion.Latest);
        }

        #region Overrides of Object

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString () {
            return $"InfluxDb(Url:{this.Url} , Database:{Database} TimeZone:{TimeZone} )";
        }

        #endregion
    }

}