using System.Collections.Generic;
using System.Runtime.Serialization;

namespace InfluxDb.Extensions
{
    /// <summary>
    /// 数据分页结果
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    [DataContract]
    public class PageResult<TData> {

        public PageResult (Paging page) {
            Page = page;
        }

        public PageResult (Paging page, IEnumerable<TData> data) : this (page) {
            Values = data;
        }

        /// <summary>
        /// 分页信息
        /// </summary>
        [DataMember (Name = "page")]
        public Paging Page { get; }

        /// <summary>
        /// 分页数据
        /// </summary>
        [DataMember (Name = "values")]
        public IEnumerable<TData> Values { get; }

    }
}