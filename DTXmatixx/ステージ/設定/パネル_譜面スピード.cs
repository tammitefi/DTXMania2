using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.設定
{
    /// <summary>
    ///		スクロール速度専用の数値ボックス。
    /// </summary>
    class パネル_譜面スピード : パネル
    {
        public パネル_譜面スピード( string パネル名 )
            : base( パネル名, null )
        {
            this.子リスト.Add( this._項目画像 = new 文字列画像() { 表示文字列 = "", フォントサイズpt = 34f, 前景色 = Color4.White } );
        }

        public override void 左移動キーが入力された()
        {
            // 譜面スクロールを減速
            const double 最小倍率 = 0.5;
            App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 = Math.Max( App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 - 0.5, 最小倍率 );
        }
        public override void 右移動キーが入力された()
        {
            // 譜面スクロールを加速
            const double 最大倍率 = 8.0;
            App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 = Math.Min( App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 + 0.5, 最大倍率 );
        }
        public override void 確定キーが入力された()
        {
            const double 最小倍率 = 0.5;
            const double 最大倍率 = 8.0;
            App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 += 0.5;

            if( 最大倍率 < App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 )
                App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 = 最小倍率;
        }

        protected override void On活性化( グラフィックデバイス gd )
        {
            base.On活性化( gd );   //忘れないこと
        }
        protected override void On非活性化( グラフィックデバイス gd )
        {
            base.On非活性化( gd );   //忘れないこと
        }

        public override void 進行描画する( グラフィックデバイス gd, DeviceContext1 dc, float left, float top, bool 選択中 )
        {
            // パネルの共通部分を描画。
            base.進行描画する( gd, dc, left, top, 選択中 );

            // 項目部分の描画。
            this._項目画像.表示文字列 = "×　" + App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度.ToString( "0.0" );
            this._項目画像.ビットマップを生成または更新する( gd );      // このあと画像のサイズが必要になるので、先に生成/更新する。

            float 拡大率Y = (float) this._パネルの高さ割合.Value;
            float 項目の上下マージン = this.項目領域.Height * ( 1f - 拡大率Y ) / 2f;

            var 項目矩形 = new RectangleF(
                x: this.項目領域.X + left,
                y: this.項目領域.Y + top + 項目の上下マージン,
                width: this.項目領域.Width,
                height: this.項目領域.Height * 拡大率Y );

            float 拡大率X = Math.Min( 1f, ( 項目矩形.Width - 20f ) / this._項目画像.サイズ.Width );    // -20 は左右マージンの最低値[dpx]

            this._項目画像.描画する(
                gd,
                dc,
                項目矩形.Left + ( 項目矩形.Width - this._項目画像.サイズ.Width * 拡大率X ) / 2f,
                項目矩形.Top + ( 項目矩形.Height - this._項目画像.サイズ.Height * 拡大率Y ) / 2f,
                X方向拡大率: 拡大率X,
                Y方向拡大率: 拡大率Y );
        }

        private 文字列画像 _項目画像 = null;
    }
}
