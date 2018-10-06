using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SSTFormat.v3
{
    public class チップ : IComparable, ICloneable
    {
        public const int 最大音量 = 8;
		public const int 既定音量 = 7;

        public チップ種別 チップ種別 { get; set; } = チップ種別.Unknown;

        /// <summary>
        ///		チップの種別をより詳細に示すためのユーザ定義ID。
        ///		今のところ、DTXから変換した場合に、ここにオブジェクト値が格納される。
        /// </summary>
        public int チップサブID { get; set; } = 0;

        public int 小節番号 { get; set; } = -1;

        public int 小節内位置 { get; set; } = 0;

        public int 小節解像度 { get; set; } = 1;

        /// <summary>
        ///		チップの描画時刻[ms]。
        ///		譜面の先頭（小節番号 -1 の小節の先頭）からの時刻をミリ秒単位で表す。
        ///		未使用なら -1 。
        /// </summary>
        public long 描画時刻ms { get; set; } = -1;

        /// <summary>
        ///		チップの描画時刻[sec]。
        ///		譜面の先頭（小節番号 -1 の小節の先頭）からの時刻を秒単位で表す。
        /// </summary>
        public double 描画時刻sec
            => this.描画時刻ms / 1000.0;

        /// <summary>
        ///		チップの発声時刻[ms]。
        ///		譜面の先頭（小節番号 -1 の小節の先頭）からの時刻をミリ秒単位で表す。
        ///		未使用なら -1 。
        /// </summary>
        /// <remarks>
        ///		サウンドの発声遅延を考慮して、描画時刻よりも遅く設定すること。
        /// </remarks>
        public long 発声時刻ms { get; set; } = -1;

        /// <summary>
        ///		チップの発声時刻[sec]。
        ///		譜面の先頭（小節番号 -1 の小節の先頭）からの時刻を秒単位で表す。
        /// </summary>
        /// <remarks>
        ///		サウンドの発声遅延を考慮して、描画時刻よりも遅く設定すること。
        /// </remarks>
        public double 発声時刻sec
            => this.発声時刻ms / 1000.0;

        /// <summary>
        ///		チップの音量（小:1～8:大）。
        /// </summary>
        public int 音量
        {
            get
                => this._音量;
            set
                => this._音量 = ( ( 1 > value ) || ( チップ.最大音量 < value ) ) ?
                throw new ArgumentException( $"音量の値域(1～{チップ.最大音量})を超える値 '{value}' が指定されました。" ) :
                value;
        }

        /// <summary>
        ///		PAN。
        ///		左:-100 ～ 中央:0 ～ +100:右。
        /// </summary>
        public int 位置
        {
            get
                => this._位置;
            set
                => this._位置 = ( -100 > value || +100 < value ) ?
                throw new ArgumentOutOfRangeException( $"位置の値域(-100～+100)を超える値 '{value}' が指定されました。" ) :
                value;
        }

        /// <summary>
        ///		チップが BPM チップである場合は、その BPM 値。
        ///		それ以外の場合は無効。
        /// </summary>
        public double BPM { get; set; } = 120.0;


        // メンバに変更があれば修正すること。
        public static void Copy( チップ src, チップ dst )
        {
            dst.チップ種別 = src.チップ種別;
            dst.チップサブID = src.チップサブID;
            dst.小節番号 = src.小節番号;
            dst.小節内位置 = src.小節内位置;
            dst.小節解像度 = src.小節解像度;
            dst.描画時刻ms = src.描画時刻ms;
            dst.発声時刻ms = src.発声時刻ms;
            dst.音量 = src.音量;
            dst.位置 = src.位置;
            dst.BPM = src.BPM;
        }

        public チップ()
        {
        }
        public チップ( チップ コピー元チップ )
        {
            this.CopyFrom( コピー元チップ );
        }
        /// <summary>
        ///		v2 からのバージョンアップ。
        /// </summary>
        public チップ( SSTFormat.v2.チップ v2chip )
        {
            this.チップ種別 = this.チップ種別.FromV2( v2chip.チップ種別 );
            this.チップサブID = 0;   // [仕様追加] v2: なし → v3: 新設
            this.小節番号 = v2chip.小節番号;
            this.小節内位置 = v2chip.小節内位置;
            this.小節解像度 = v2chip.小節解像度;
            this.描画時刻ms = v2chip.描画時刻ms;
            this.発声時刻ms = v2chip.発声時刻ms;
            this.音量 = v2chip.音量;
            this.BPM = v2chip.BPM;
        }

        public void CopyFrom( チップ コピー元チップ )
        {
            チップ.Copy( コピー元チップ, this );
        }

        public チップ Clone()
        {
            return (チップ) this.MemberwiseClone();
        }
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #region " IComparable 実装 "
        //-----------------
        // 概要:
        //     現在のインスタンスを同じ型の別のオブジェクトと比較して、並べ替え順序において、現在のインスタンスの位置が同じ型の別のオブジェクトの前、後ろ、または同じのいずれであるかを示す整数を返します。
        //
        // パラメータ:
        //   obj:
        //     このインスタンスと比較するオブジェクト。
        //
        // 戻り値:
        //     比較対象オブジェクトの相対順序を示す 32 ビット符号付き整数。戻り値の意味は次のとおりです。 
        //
        //     値		説明
        //     --------------------
        //     負数		this ＜ obj
        //     0		this ＝ obj
        //     正数		this ＞ obj
        //
        // 例外:
        //   System.ArgumentException:
        //     obj の型がこのインスタンスの型と異なります。
        //
        public int CompareTo( object obj )
        {
            var other = obj as チップ;

            if( this.小節番号 < other.小節番号 ) { return -1; }
            if( this.小節番号 > other.小節番号 ) { return +1; }

            double dbThis = (double) this.小節内位置 / (double) this.小節解像度;
            double dbOther = (double) other.小節内位置 / (double) other.小節解像度;

            if( dbThis < dbOther ) { return -1; }
            if( dbThis > dbOther ) { return +1; }


            // グリッドが完全に等しいなら、チップの種類ごとに定義された深度で順序を決める。

            if( チップ.チップの深さ[ this.チップ種別 ] > チップ.チップの深さ[ other.チップ種別 ] ) { return -1; }
            if( チップ.チップの深さ[ this.チップ種別 ] < チップ.チップの深さ[ other.チップ種別 ] ) { return +1; }

            return 0;
        }
        //-----------------
        #endregion

        protected readonly static Dictionary<チップ種別, int> チップの深さ
        #region " *** "
            //-----------------
            = new Dictionary<チップ種別, int>() {
                { チップ種別.Ride_Cup, 50 },
                { チップ種別.HiHat_Open, 50 },
                { チップ種別.HiHat_HalfOpen, 50 },
                { チップ種別.HiHat_Close, 50 },
                { チップ種別.HiHat_Foot, 50 },
                { チップ種別.Snare, 50 },
                { チップ種別.Snare_OpenRim, 50 },
                { チップ種別.Snare_ClosedRim, 50 },
                { チップ種別.Snare_Ghost, 50 },
                { チップ種別.Tom1, 50 },
                { チップ種別.Tom1_Rim, 50 },
                { チップ種別.BPM, 50 },
                { チップ種別.Ride, 60 },
                { チップ種別.Splash, 60 },
                { チップ種別.Tom2, 60 },
                { チップ種別.Tom2_Rim, 60 },
                { チップ種別.LeftCrash, 70 },
                { チップ種別.China, 70 },
                { チップ種別.Tom3, 70 },
                { チップ種別.Tom3_Rim, 70 },
                { チップ種別.RightCrash, 70 },
                { チップ種別.Bass, 74 },
                { チップ種別.LeftBass, 75 },
                { チップ種別.LeftCymbal_Mute, 76 },
                { チップ種別.RightCymbal_Mute, 76 },
                { チップ種別.小節線, 80 },
                { チップ種別.拍線, 85 },
                { チップ種別.背景動画, 90 },
                { チップ種別.小節メモ, 99 },
                { チップ種別.小節の先頭, 99 },
                { チップ種別.BGM, 99 },
                { チップ種別.SE1, 99 },
                { チップ種別.SE2, 99 },
                { チップ種別.SE3, 99 },
                { チップ種別.SE4, 99 },
                { チップ種別.SE5, 99 },
                { チップ種別.GuitarAuto, 99 },
                { チップ種別.BassAuto, 99 },
                { チップ種別.Unknown, 99 },
            };
        //-----------------
        #endregion

        private int _音量 = チップ.最大音量;
        private int _位置 = 0;
    }
}
