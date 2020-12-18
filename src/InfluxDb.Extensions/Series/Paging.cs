using System;
using System.Runtime.Serialization;
using System.Text;

namespace InfluxDb.Extensions {
    /// <summary>
    /// 分页数据
    /// </summary>
    [DataContract]
    public partial class Paging {
        ///<summary>
        /// 每页最小数量
        ///</summary>
        const int MIN_PAGE_SIZE = 5;

        /// <summary>
        /// 默认分页大小
        /// </summary>
        private const int DEFAULT_PAGE_SIZE = 20;

        /// <summary>
        /// 默认页码
        /// </summary>
        private const int DEFAULT_PAGE_NUMBER = 1;

        /// <summary>
        /// 默认构造方法
        /// </summary>
        public Paging () {
            Page = DEFAULT_PAGE_NUMBER;
            PageSize = DEFAULT_PAGE_SIZE;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Paging" /> class.
        /// </summary>
        /// <param name="total">总数量.</param>
        /// <param name="pageSize">页面大小.</param>
        /// <param name="pageNum">当前页码.</param>
        public Paging (int total, int? pageSize = DEFAULT_PAGE_SIZE, int? pageNum = DEFAULT_PAGE_NUMBER) {
            Total = total < 0 ? 0 : total;
            PageSize = pageSize??DEFAULT_PAGE_SIZE;
            PageSize = Math.Max (MIN_PAGE_SIZE, PageSize);
            Page = pageNum??DEFAULT_PAGE_NUMBER;
            Page = Math.Max (1, Page);
            Pages = (int) Math.Ceiling (total / (double) pageSize);

            var skip = PageSize * (Page - 1);
            if (skip > Total) {
                skip = 0;
                Page = 1;
            }
        }

        /// <summary>
        /// 总数量
        /// </summary>
        /// <value>总数量</value>
        [DataMember (Name = "total", IsRequired = false)]
        public int Total { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        /// <value>总页数</value>
        [DataMember (Name = "pages", IsRequired = false)]
        public int Pages { get; set; }

        /// <summary>
        /// 页面大小
        /// </summary>
        /// <value>页面大小</value>
        [DataMember (Name = "pageSize")]
        public int PageSize { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        /// <value>当前页码</value>
        [DataMember (Name = "page", IsRequired = true, EmitDefaultValue = true)]
        public int Page { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString () {
            var sb = new StringBuilder ();
            sb.Append ("class Paging {\n");
            sb.Append ("  Total: ").Append (Total).Append ("\n");
            sb.Append ("  Pages: ").Append (Pages).Append ("\n");
            sb.Append ("  PageSize: ").Append (PageSize).Append ("\n");
            sb.Append ("  Page: ").Append (Page).Append ("\n");
            sb.Append ("}\n");
            return sb.ToString ();
        }
    }

}