using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DTXmatixx.ステージ.演奏
{
    /// <summary>
    ///     チップの描画形態。
    /// </summary>
    enum 表示チップ種別
    {
        Unknown,
        LeftCymbal,
        RightCymbal,
        HiHat,
        HiHat_Open,
        HiHat_HalfOpen,
        Foot,
        LeftPedal,
        Snare,
        Snare_OpenRim,
        Snare_ClosedRim,
        Snare_Ghost,
        Bass,
        LeftBass,
        Tom1,
        Tom1_Rim,
        Tom2,
        Tom2_Rim,
        Tom3,
        Tom3_Rim,
        Ride,
        Ride_Cup,
        China,
        Splash,
        PartLine,
        BeatLine,
        LeftCymbal_Mute,
        RightCymbal_Mute,
    }
}
