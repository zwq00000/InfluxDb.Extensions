using System.Collections.Generic;

namespace InfluxDb.Extensions {
    internal class MeasurementInfo {
        public MeasurementInfo () { }

        public MeasurementInfo (string database, string measurement, string timeZone, IEnumerable<string> fields, IEnumerable<string> tags) {
            Database = database;
            Measurement = measurement;
            TimeZone = timeZone;
            Fields = fields;
            Tags = tags;
        }

        /// <summary>
        /// Influx Database name
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Measurement Name
        /// </summary>
        public string Measurement { get; set; }

        /// <summary>
        /// 默认时区
        /// </summary>
        public string TimeZone { get; set; }

        /// <summary>
        /// 字段列表
        /// </summary>
        public IEnumerable<string> Fields { get; set; }

        /// <summary>
        /// Tag Keys
        /// </summary>
        public IEnumerable<string> Tags { get; set; }
    }
}