using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.ステージ.オプション設定
{
    /// <summary>
    ///		スクロール速度専用の数値ボックス。
    /// </summary>
    class パネル_譜面スピード : パネル
    {
        protected const double 最小倍率 = 0.5;
        protected const double 最大倍率 = 8.0;


        public パネル_譜面スピード( string パネル名 )
            : base( パネル名, null )
        {
            //using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._項目画像 = new 文字列画像() { 表示文字列 = "", フォントサイズpt = 34f, 前景色 = Color4.White } );
                Log.Info( $"譜面スピードパネルを生成しました。[{this}]" );
            }
        }

        protected override void On活性化()
        {
            base.On活性化();   //忘れないこと
        }

        protected override void On非活性化()
        {
            base.On非活性化();   //忘れないこと
        }

        public override void 左移動キーが入力された()
        {
            // 譜面スクロール値を減らす。
            App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 = Math.Max( App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 - 0.5, 最小倍率 );

            base.左移動キーが入力された(); // 忘れないこと
        }

        public override void 右移動キーが入力された()
        {
            // 譜面スクロール値を増やす。
            App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 = Math.Min( App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 + 0.5, 最大倍率 );

            base.右移動キーが入力された(); // 忘れないこと
        }

        public override void 確定キーが入力された()
        {
            // 譜面スクロール値を増やす。

            App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 += 0.5;

            // 最大値を超えたら最小値に戻る。

            if( 最大倍率 < App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 )
                App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 = 最小倍率;

            base.確定キーが入力された();
        }

        public override void 進行描画する( DeviceContext1 dc, float left, float top, bool 選択中 )
        {
            // (1) パネルの下地と名前を描画。

            base.進行描画する( dc, left, top, 選択中 );


            // (2) 値を描画。

            this._項目画像.表示文字列 = "×　" + App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度.ToString( "0.0" );
            this._項目画像.ビットマップを生成または更新する();      // このあと画像のサイズが必要になるので、先に生成/更新する。

            float 拡大率Y = (float) this._パネルの高さ割合.Value;
            float 項目の上下マージン = this.項目領域.Height * ( 1f - 拡大率Y ) / 2f;

            var 項目矩形 = new RectangleF(
                x: this.項目領域.X + left,
                y: this.項目領域.Y + top + 項目の上下マージン,
                width: this.項目領域.Width,
                height: this.項目領域.Height * 拡大率Y );

            float 拡大率X = Math.Min( 1f, ( 項目矩形.Width - 20f ) / this._項目画像.画像サイズdpx.Width );    // -20 は左右マージンの最低値[dpx]

            this._項目画像.描画する(
                dc,
                項目矩形.Left + ( 項目矩形.Width - this._項目画像.画像サイズdpx.Width * 拡大率X ) / 2f,
                項目矩形.Top + ( 項目矩形.Height - this._項目画像.画像サイズdpx.Height * 拡大率Y ) / 2f,
                X方向拡大率: 拡大率X,
                Y方向拡大率: 拡大率Y );
        }


        private 文字列画像 _項目画像 = null;
    }
}
