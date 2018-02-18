using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Linq;
using System.Data.SQLite;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;
using DTXmatixx.曲;
using DTXmatixx.設定;
using DTXmatixx.ステージ.演奏;
using DTXmatixx.データベース.曲;

namespace DTXmatixx.ステージ.選曲
{
    class 曲ステータスパネル : Activity
    {
        public 曲ステータスパネル()
        {
            this.子を追加する( this._背景画像 = new 画像( @"$(System)images\選曲\曲ステータスパネル.png" ) );
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var dc = グラフィックデバイス.Instance.D2DDeviceContext;

                this._色 = new Dictionary<表示レーン種別, SolidColorBrush>() {
                    { 表示レーン種別.LeftCymbal, new SolidColorBrush( dc, new Color4( 0xff7b1fff ) ) },
                    { 表示レーン種別.HiHat, new SolidColorBrush( dc, new Color4( 0xffffc06a ) ) },
                    { 表示レーン種別.Foot, new SolidColorBrush( dc, new Color4( 0xffed4bff ) ) },
                    { 表示レーン種別.Snare, new SolidColorBrush( dc, new Color4( 0xff16fefc ) ) },
                    { 表示レーン種別.Tom1, new SolidColorBrush( dc, new Color4( 0xff00ff02 ) ) },
                    { 表示レーン種別.Bass, new SolidColorBrush( dc, new Color4( 0xffff819b ) ) },
                    { 表示レーン種別.Tom2, new SolidColorBrush( dc, new Color4( 0xff0000ff  ) ) },
                    { 表示レーン種別.Tom3, new SolidColorBrush( dc, new Color4( 0xff19a9ff ) ) },
                    { 表示レーン種別.RightCymbal, new SolidColorBrush( dc, new Color4( 0xffffb55e ) ) },
                };
            }
        }
        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                foreach( var kvp in this._色 )
                    kvp.Value.Dispose();
            }
        }

        public void 描画する( DeviceContext1 dc )
        {
            var 領域dpx = new RectangleF( 320f, 532f, 239f, 505f );

            #region " ノードが変更されていたら情報を更新する。"
            //----------------
            if( App.曲ツリー.フォーカス曲ノード != this._現在表示しているノード )
            {
                this._現在表示しているノード = App.曲ツリー.フォーカス曲ノード; // MusicNode 以外は null が返される

                this._ノーツ数 = null;

                if( null != this._現在表示しているノード )
                {
                    using( var songdb = new SongDB() )
                    {
                        var note = songdb.Songs.Where( ( r ) => ( r.HashId == this._現在表示しているノード.曲ファイルハッシュ ) ).SingleOrDefault();

                        if( null != note )
                        {
                            this._ノーツ数 = new Dictionary<表示レーン種別, int>() {
                                { 表示レーン種別.Unknown, 0 },
                                { 表示レーン種別.LeftCymbal, note.TotalNotes_LeftCymbal },
                                { 表示レーン種別.HiHat, note.TotalNotes_HiHat },
                                { 表示レーン種別.Foot, note.TotalNotes_LeftPedal },
                                { 表示レーン種別.Snare, note.TotalNotes_Snare },
                                { 表示レーン種別.Bass, note.TotalNotes_Bass },
                                { 表示レーン種別.Tom1, note.TotalNotes_HighTom },
                                { 表示レーン種別.Tom2, note.TotalNotes_LowTom },
                                { 表示レーン種別.Tom3, note.TotalNotes_FloorTom },
                                { 表示レーン種別.RightCymbal, note.TotalNotes_RightCymbal },
                            };
                        }
                    }
                }
            }
            //----------------
            #endregion

            bool 表示可能ノードである = ( this._現在表示しているノード is MusicNode );

            // 背景を表示。
            this._背景画像.描画する( dc, 左位置: 領域dpx.X, 上位置: 領域dpx.Y );

            // Total Notes を表示。
            if( 表示可能ノードである )
            {
                if( null != this._ノーツ数 )
                {
                    グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                        var Xオフセット = new Dictionary<表示レーン種別, float>() {
                            { 表示レーン種別.LeftCymbal, +70f },
                            { 表示レーン種別.HiHat, +88f },
                            { 表示レーン種別.Foot, +106f },
                            { 表示レーン種別.Snare, +124f },
                            { 表示レーン種別.Tom1, +142f },
                            { 表示レーン種別.Bass, +160f },
                            { 表示レーン種別.Tom2, +178f },
                            { 表示レーン種別.Tom3, +196f },
                            { 表示レーン種別.RightCymbal, +214f },
                        };
                        const float Yオフセット = +2f;

                        foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
                        {
                            if( lane == 表示レーン種別.Unknown )
                                continue;

                            var 矩形 = new RectangleF( 領域dpx.X + Xオフセット[ lane ], 領域dpx.Y + Yオフセット, 6f, 405f );
                            矩形.Top = 矩形.Bottom - ( 矩形.Height * Math.Min( this._ノーツ数[ lane ], 250 ) / 250f );
                            dc.FillRectangle( 矩形, this._色[ lane ] );
                        }

                    } );
                }
            }
        }

        private 画像 _背景画像 = null;
        private MusicNode _現在表示しているノード = null;
        private Dictionary<表示レーン種別, int> _ノーツ数 = null;
        private Dictionary<表示レーン種別, SolidColorBrush> _色 = null;
    }
}
