using System;

namespace SSTFormat.v3
{
    /// <summary>
    ///		チップの種別を表す整数値。
    /// </summary>
    /// <remarks>
    ///		互換性を維持するために、将来にわたって不変な int 型の数値を、明確に定義する。
    ///		増減した場合は、<see cref="チップ.チップの深さ"/> も更新すること。
    /// </remarks>
    public enum チップ種別 : int
    {
        Unknown = 0,
        LeftCrash = 1,
        Ride = 2,
        Ride_Cup = 3,
        China = 4,
        Splash = 5,
        HiHat_Open = 6,
        HiHat_HalfOpen = 7,
        HiHat_Close = 8,
        HiHat_Foot = 9,
        Snare = 10,
        Snare_OpenRim = 11,
        Snare_ClosedRim = 12,
        Snare_Ghost = 13,
        Bass = 14,
        Tom1 = 15,
        Tom1_Rim = 16,
        Tom2 = 17,
        Tom2_Rim = 18,
        Tom3 = 19,
        Tom3_Rim = 20,
        RightCrash = 21,
        BPM = 22,
        小節線 = 23,
        拍線 = 24,
        背景動画 = 25,
        小節メモ = 26,

        // v1.2
        LeftCymbal_Mute = 27,
        RightCymbal_Mute = 28,
        小節の先頭 = 29,

        // v3
        BGM = 30,
        SE1 = 31,
        SE2 = 32,
        SE3 = 33,
        SE4 = 34,
        SE5 = 35,   // SE は最大5つまで対応。
        GuitarAuto = 36,    // ギターチップ
        BassAuto = 37,  // ベースチップ

        // v3.1
        LeftBass = 38,
    }

    /// <summary>
    ///		拡張メソッド。
    /// </summary>
    public static class チップ種別Extensions
    {
        /// <summary>
        ///		SSTFormat.v1.チップ種別 を、SSTFormat.v2.チップ種別 に変換して返す。
        /// </summary>
        public static チップ種別 FromV1( this チップ種別 v2type, SSTFormat.v1.チップ種別 v1type )
        {
            return (チップ種別) ( (int) v1type );    // 仕様に変更なし。
        }

        /// <summary>
        ///		SSTFormat.v2.チップ種別 を、SSTFormat.v3.チップ種別 に変換して返す。
        /// </summary>
        public static チップ種別 FromV2( this チップ種別 v3type, SSTFormat.v2.チップ種別 v2type )
        {
            return (チップ種別) ( (int) v2type );    // 仕様に変更なし。
        }
    }
}
