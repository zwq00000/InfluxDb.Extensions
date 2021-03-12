using System.Runtime.Serialization;

namespace InfluxDb.Extensions.Tests {
    /// <summary>
    /// IShip Track 参考实现
    /// </summary>
    [DataContract]
    public class ShipTrack {
        private string _mmsi;

        #region Implementation of IShip

        /// <summary>
        /// 船舶唯一标识号
        /// </summary>
        [DataMember (Name = "id")]
        public string ShipId { get; set; }

        /// <summary>
        /// MMSI号码
        /// </summary>
        [DataMember (Name = "mmsi")]
        public string MMSI {
            get => _mmsi;
            set {
                if (string.Equals (_mmsi, value)) {
                    return;
                }
                _mmsi = value;
                // this.MID = this.GetMid ();
            }
        }

        /// <summary>
        /// 海事识别数字 (Maritime Identification Digits)
        /// 2~7 开头的3位数字, 0 表示未知
        /// </summary>
        /// <remarks>
        /// 无线电通信设施使用海事识别数字在数字选择呼叫（DSC），自动发射机识别系统（ATIS）和自动识别系统（AIS）消息中识别其本国或基地区域，作为其海事移动业务标识的一部分。在国际电信联盟方便的MID向各国分配。[1]这是MID的综合列表[2]世界上每个国家都使用。请注意，并非所有国家都有MID; 那些没有的人通常是内陆的，无法进入国际水域。按数字顺序对MID分配进行排序会显示区域结构，
        /// 第一个数字：
        ///     2 欧洲，
        ///     3 北美和加勒比海，
        ///     4 亚洲（不是东南），
        ///     5 太平洋和东印度洋和东南亚，
        ///     6 非洲，大西洋和西印度洋，和
        ///     7 南美洲。
        /// </remarks>
        [DataMember (Name = "mid")]
        public int MID { get; private set; }

        #endregion

        #region Implementation of ICoordinates

        /// <summary>
        /// 经度(Longitude)
        /// 范围为-180°&lt;经度 &lt;=180°
        /// 1/10000分表示的经度（东= +，西= －）
        /// </summary>
        [DataMember (Name = "lng")]
        public double Lng { get; set; }

        /// <summary>
        /// 纬度(Latitude) 
        /// 范围为-90°&lt;纬度 &lt;=90°
        /// 用1/10000分表示的纬度（北= +，南= －）
        /// </summary>
        [DataMember (Name = "lat")]
        public double Lat { get; set; }

        #endregion

        #region Implementation of IShipTrack

        ///<summary>
        /// 对地航向 (Course Over Ground)
        ///</summary>
        [DataMember (Name = "cog")]
        public float Cog { get; set; }

        ///<summary>
        /// 对地航速 (Speed Over Ground)
        ///</summary>
        [DataMember (Name = "sog")]
        public float Sog { get; set; }

        ///<summary>
        /// 船艏向 (heading) 
        /// 取值范围 0°- 360°
        /// 真北线与船首线之间的夹角
        ///</summary>
        [DataMember (Name = "heading")]
        public int Heading { get; set; }

        ///<summary>
        /// 转向率 (Rate of turn/ROT) (-127 ~ +127)
        ///</summary>
        [DataMember (Name = "rot")]
        public float Rot { get; set; }

        /// <summary>
        /// 航行状态
        /// </summary>
        [DataMember (Name = "navStatus")]
        public NavStatus NavStatus { get; set; }

        /// <summary>
        /// 时间戳,基于 1970年1月1日0时的秒数
        /// 转换方法参见 <see cref="TimeStampExtensions.ToDateTime"/>
        /// 和 <seealso cref="TimeStampExtensions.ToTimestamp"/>
        /// </summary>
        [DataMember (Name = "timestamp")]
        public int Timestamp { get; set; }

        #endregion
    }
}