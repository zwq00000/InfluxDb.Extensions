using System;

namespace InfluxDb.Extensions {
    /// <summary>
    /// 序列分段数量
    /// </summary>
    public class Segment {
        public Segment () {

        }

        public Segment (Segment segment) : this (segment.Start, segment.End, segment.Count) { }

        public Segment (DateTime start, DateTime end, int count) {
            Start = start;
            End = end;
            Count = count;
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        /// <value></value>
        public DateTime Start { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        /// <value></value>
        public DateTime End { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        /// <value></value>
        public int Count { get; set; }
    }
}