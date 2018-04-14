using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSCore;
using FDK;
using FDK.メディア.ビデオ;
using FDK.メディア.サウンド;
using FDK.メディア.HTTPストリーミング;

namespace DTXmatixx.曲
{
    /// <summary>
    ///     演奏用の BGV（背景動画）と BGM（SSTF専用）を管理する。
    /// </summary>
    /// <remarks>
    ///     BGVは演奏ステージだけでしか使用されないが、BGMは結果ステージまで使用される場合がある。
    ///     両者のライフサイクルの違いに注意。
    ///     なお、DTX の BGM は動画ファイルとは別になるので、ここでは管理しない。（<see cref="WAV管理"/> で管理する）
    /// </remarks>
    class BGMとBGV : Activity, IDisposable
    {
        public Video BGV
        {
            get;
            protected set;
        } = null;
        public Sound BGM
        {
            get;
            protected set;
        } = null;

        public BGMとBGV( VariablePath ファイルパス )
        {
            this.生成元 = E生成元.ファイル;

            this.子を追加する( this.BGV = new Video( ファイルパス ) );

            this._SampleSource = SampleSourceFactory.Create( App.サウンドデバイス, ファイルパス );
            this.BGM = new Sound( App.サウンドデバイス, this._SampleSource );
        }
        public BGMとBGV( string user_id, string password, string video_id )
        {
            this.生成元 = E生成元.ニコニコ動画;

            this.子を追加する( this._ニコ動 = new ニコニコ動画( user_id, password, video_id ) );

            this.BGV = this._ニコ動.ビデオ;
            this.BGM = this._ニコ動.オーディオ;
        }
        protected override void On活性化()
        {
            base.On活性化();
        }
        protected override void On非活性化()
        {
            base.On非活性化();
        }
        public void Dispose()
        {
            if( this.活性化している )
                this.非活性化する();

            switch( this.生成元 )
            {
                case E生成元.ファイル:
                    if( this.BGV?.活性化している ?? false )
                        this.BGV.非活性化する();
                    this.BGV = null;
                    this.BGM?.Dispose();
                    this.BGM = null;
                    this._SampleSource.Dispose();
                    this._SampleSource = null;
                    break;

                case E生成元.ニコニコ動画:
                    this.BGV = null;
                    this.BGM = null;
                    if( this._ニコ動?.活性化している ?? false )
                        this._ニコ動.非活性化する();
                    this._ニコ動 = null;
                    break;
            }
        }

        protected enum E生成元
        {
            Unknwon,
            ファイル,
            ニコニコ動画,
        }
        protected E生成元 生成元 = E生成元.Unknwon;

        private ニコニコ動画 _ニコ動 = null;
        private ISampleSource _SampleSource = null;
    }
}
