using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfluxDb.Extensions {
    /// <summary>
    /// Influx DB 查询构建器
    /// </summary>
    public class SqlBuilder {

        /// <summary>
        /// fields
        /// </summary>
        private IEnumerable<string> _fields;

        /// <summary>
        /// from table
        /// </summary>
        private string _table;

        /// <summary>
        /// Where 子句列表
        /// </summary>
        private IList<string> _whereClause;

        /// <summary>
        /// Order by fields
        /// </summary>
        private IList<string> _orderbyClause;

        /// <summary>
        /// Order by Desc fields
        /// </summary>
        private IList<string> _orderbyDescClause;

        /// <summary>
        /// group by fields
        /// </summary>
        private IList<string> _groups;

        /// <summary>
        /// 空值填充
        /// </summary>
        /// <value></value>
        private string _fillValue;

        /// <summary>
        /// 时区
        /// </summary>
        private string _timeZone;

        public SqlBuilder () { }

        public SqlBuilder (IEnumerable<string> fields, string from) : this () {
            this._fields = fields;
            this._table = from;
        }

        public SqlBuilder Select (params string[] fields) {
            this._fields = fields;
            return this;
        }

        public SqlBuilder From (string from) {
            _table = from;
            return this;
        }

        /// <summary>
        /// where 子句
        /// 支持 
        ///     Is null,
        ///     DateTime,
        ///     Timespna now()-{time.TotalSeconds}s
        ///     string op 'string'
        /// </summary>
        /// <param name="column"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SqlBuilder Where (string column, string op, object value) {
            return Where (ToWhereClause (column, op, value));
        }

        private string ToWhereClause (string column, string op, object value) {
            string valueStr;
            switch (value) {
                case null:
                    op = "IS";
                    valueStr = "NULL";
                    break;
                case DateTime date:
                    valueStr = $"'{date.ToRfc3339()}'";
                    break;
                case TimeSpan span:
                    valueStr = $"now() - {span.TotalSeconds:F0}s";
                    break;
                case string str:
                    valueStr = $"'{str}'";
                    break;
                default:
                    valueStr = value.ToString ();
                    break;
            }
            return $"{column} {op} {valueStr}";
        }

        public SqlBuilder Where (string whereClause) {
            if (_whereClause == null) {
                _whereClause = new List<string> ();
            }
            _whereClause.Add (whereClause);
            return this;
        }

        public SqlBuilder OrderBy (params string[] fields) {
            if (_orderbyClause == null) {
                _orderbyClause = new List<string> (fields.Where (f => !string.IsNullOrEmpty (f)));
            } else {
                foreach (var key in fields) {
                    _orderbyClause.Add (key);
                }
            }
            return this;
        }

        public SqlBuilder OrderByDesc (params string[] fields) {
            if (_orderbyDescClause == null) {
                _orderbyDescClause = new List<string> (fields.Where (f => !string.IsNullOrEmpty (f)));
            } else {
                foreach (var key in fields) {
                    _orderbyDescClause.Add (key);
                }
            }
            return this;
        }

        public SqlBuilder GroupBy (params string[] fields) {
            if (_groups == null) {
                _groups = new List<string> (fields.Where (f => !string.IsNullOrWhiteSpace (f)));
            } else if (fields.Any ()) {
                foreach (var field in fields.Where (f => !string.IsNullOrWhiteSpace (f))) {
                    _groups.Add (field);
                }
            }

            return this;
        }

        /// <summary>
        /// 根据 时间范围分组
        /// </summary>
        /// <remarks>
        /// InfluxDB使用预设的舍入数时间边界作为GROUP BY间隔，该间隔与WHERE子句中的任何时间条件无关。
        /// 计算结果时，所有返回的数据都必须在查询的显式时间范围内出现，但GROUP BY间隔将基于预设的时间范围。
        /// </remarks>
        /// <returns></returns>
        public SqlBuilder GroupByTime (int time, string timeunit) {
            if (_groups == null) {
                _groups = new List<string> ();
            }
            this._groups.Add ($"time({time}{timeunit})");
            return this;
        }

        /// <summary>
        /// 根据 时间范围分组
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public SqlBuilder GroupByTime (TimeSpan duration) {
            if (_groups == null) {
                _groups = new List<string> ();
            }
            this._groups.Add ($"time({duration.ToTimeInterval()})");
            return this;
        }

        /// <summary>
        /// group by 选项
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public SqlBuilder Fill (int? value) {
            if (value.HasValue) {
                this._fillValue = value.Value.ToString ();
            } else {
                this._fillValue = "NULL";
            }
            return this;
        }

        /// <summary>
        /// Fill with a specified value
        ///  To fill null values with a specified value, use the value parameter to specify the fill value.
        /// The fill value must match the data type of the column.
        /// </summary>
        /// <returns></returns>
        public SqlBuilder FillPrevious () {
            this._fillValue = "previous";
            return this;
        }

        /// <summary>
        /// 设置时区 Id
        /// </summary>
        /// <param name="timezone"></param>
        /// <returns></returns>
        public SqlBuilder TimeZone (string timezone) {
            this._timeZone = timezone;
            return this;
        }

        /// <summary>
        /// 使用本地时区,只能在 linux 下使用
        /// </summary>
        /// <returns></returns>
        public SqlBuilder LocalTimeZone () {
            this._timeZone = TimeZoneInfo.Local.Id;
            return this;
        }

        /// <summary>
        /// 生成 Count Sql
        /// </summary>
        /// <returns></returns>
        public string ToCount () {
            var field = this._fields.First ();
            return $"SELECT COUNT({field}) AS COUNT From {_table} {ToWhereClause()}";
        }

        public string ToLimitAndOffset (int count, int offset) {
            var builder = BuildSql ();
            builder.Append ($"  LIMIT {count} OFFSET {offset}").Append (ToTimeZoneClause ());
            return builder.ToString ();
        }

        private string ToWhereClause () {
            if (_whereClause != null && _whereClause.Any ()) {
                return $"\n\tWHERE {string.Join (" AND ", _whereClause)}";
            }
            return string.Empty;
        }

        /// <summary>
        /// get timezone clause, like " tz('Asia/shanghai')"
        /// if not define timezone, return empty string
        /// </summary>
        /// <returns></returns>
        private string ToTimeZoneClause () {
            if (!string.IsNullOrWhiteSpace (_timeZone)) {
                return $" tz('{_timeZone}')";
            }
            return string.Empty;
        }

        /// <summary>
        /// Get Order by Clause ,if not define order ,return empty string
        /// </summary>
        /// <returns></returns>
        private string ToOrderClause () {
            if (_orderbyClause != null && _orderbyClause.Any ()) {
                return $"\n\tOrder By {string.Join (",", _orderbyClause.Distinct ())}";
            }

            if (_orderbyDescClause != null && _orderbyDescClause.Any ()) {
                return $"\n\tOrder By {string.Join (",", _orderbyDescClause.Distinct ())} DESC ";
            }
            return string.Empty;
        }

        /// <summary>
        /// 构建 Sql 语句, 不包括 时区子句
        /// </summary>
        /// <returns></returns>
        private StringBuilder BuildSql () {
            var builder = new StringBuilder ("SELECT ");
            if (_fields == null || !_fields.Any ()) {
                builder.Append (" * ");
            } else {
                builder.Append (string.Join (",", _fields));
            }
            if (string.IsNullOrWhiteSpace (_table)) {
                throw new NullReferenceException (nameof (_table));
            }
            builder.Append ("\n\tFROM ").Append (_table);

            if (_whereClause != null && _whereClause.Any ()) {
                builder.Append ("\n\tWHERE ").Append (string.Join (" AND ", _whereClause));
            }
            if (_orderbyClause != null && _orderbyClause.Any ()) {
                builder.Append ("\n\tOrder By ").Append (string.Join (",", _orderbyClause.Distinct ()));
            }

            if (_orderbyDescClause != null && _orderbyDescClause.Any ()) {
                builder.Append ("\n\tOrder By ").Append (string.Join (",", _orderbyDescClause.Distinct ()));
                builder.Append (" DESC ");
            }

            if (_groups != null && _groups.Any ()) {
                builder.Append ("\n\tGROUP BY ").Append (string.Join (",", _groups));

                if (!string.IsNullOrWhiteSpace (_fillValue)) {
                    builder.AppendFormat (" FILL({0})", _fillValue);
                }
            }

            return builder;
        }

        public override string ToString () {
            var builder = BuildSql ().Append (ToTimeZoneClause ());
            return builder.ToString ();
        }
    }
}