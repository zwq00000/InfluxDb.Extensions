using System;

namespace InfluxDb.Extensions.Tests {

    ///<summary>
    /// AIS 航行状态代码
    ///</summary>
    ///<remarks>
    /// Navigational status as used in AisMessages of type 1, 2, and 3 and defined in Rec. ITU-R M.1371-4, table 45.
    ///     0 = under way using engine,
    ///     1 = at anchor,
    ///     2 = not under command,
    ///     3 = restricted manoeuvrability
    ///     4 = constrained by her draught,
    ///     5 = moore
    ///     6 = aground
    ///     7 = engaged in fishing
    ///     8 = under way sailing
    ///     9 = reserved for future amendment of navigational status for ships carrying DG, HS, or MP, or IMO hazard or pollutant category C, high speed craft (HSC),
    ///     10 = reserved for future amendment of navigational status for ships carrying dangerous goods (DG), harmful substances (HS) or marine pollutants (MP), or IMO hazard or pollutant category A, wing in grand (WIG);
    ///     11-13 = reserved for future use,
    ///     14 = AIS-SART (active),
    ///     15 = not defined = default (also used by AIS-SART under test)
    ///</remarks>
    public enum NavStatus {
        ///<summary>
        /// 发动机使用中/正在使用引擎的方式
        /// Under way using engine
        ///</summary>
        UsingEngine = 0,

        ///<summary>
        /// 抛锚
        /// At anchor
        ///</summary>
        AtAnchor = 1,

        ///<summary>
        /// 失控
        /// Not under command
        ///</summary>
        NotUnderCommand = 2,

        ///<summary>
        /// 操作受限/受限制的机动性
        /// Restricted manoeuverability
        ///</summary>
        Restricted = 3,

        ///<summary>
        /// 吃水受限
        /// Constrained by her draught
        ///</summary>
        Constrained = 4,

        ///<summary>
        /// 停泊
        /// Moored
        ///</summary>
        Moored = 5,

        ///<summary>
        /// 搁浅
        /// Aground
        ///</summary>
        Aground = 6,

        ///<summary>
        /// 从事钓鱼
        /// Engaged in Fishing
        ///</summary>
        Fishing = 7,

        ///<summary>
        /// 正在航行中
        /// Under way sailing
        ///</summary>
        Sailing = 8,

        ///<summary>
        /// AIS-SART处于活动状态
        /// AIS-SART is active
        ///</summary>
        SartMobOrEpirb = 14,

        ///<summary>
        /// 未定义（默认）
        /// Not defined (default)
        ///</summary>
        Undefined = 15,

        /// <summary>
        /// Reserved For Future Use 9
        /// </summary>
        Reserved9 = 9,

        /// <summary>
        /// Reserved For Future Use 10
        /// </summary>
        Reserved10 = 10,

        /// <summary>
        /// Power Driven Vessel Towing Astern
        /// </summary>
        PowerDrivenVesselTowingAstern = 11,

        /// <summary>
        /// Power Driven Vessel Pushing Ahead Or Towing Alongside
        /// </summary>
        PowerDrivenVesselPushingAheadOrTowingAlongside = 12,

        /// <summary>
        /// Reserved For Future Use 13
        /// </summary>
        Reserved13 = 13
    }

}