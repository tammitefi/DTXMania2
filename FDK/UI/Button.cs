using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.DirectWrite;

namespace FDK.UI
{
    /// <summary>
    ///     ボタン。
    ///     画像ファイルの仕様については<see cref="Window"/>を継承する。
    /// </summary>
    public class Button : Window
    {
        /// <summary>
        ///     ボタンのラベルとして表示する文字列。
        /// </summary>
        public string ラベル
        {
            get;
            set;
        } = "";

        /// <summary>
        ///     ラベル文字の色。
        /// </summary>
        public Color 文字色
        {
            get;
            set;
        } = Color.White;


        /// <summary>
        ///		ボタンの画像とサイズを設定する。
        ///		ボタン画像はいずれも同じサイズであること。
        /// </summary>
        /// <param name="通常時画像ファイル">ボタンを3x3に9分割したベース画像。</param>
        /// <param name="押下時画像ファイル">ボタンが押された状態のベース画像。</param>
        /// <param name="フォーカス時画像ファイル">ボタンの上にマウスカーソルが来るか、フォーカスされている状態のベース画像。</param>
        /// <param name="幅dpx">ボタンの幅。</param>
        /// <param name="高さdpx">ボタンの高さ。</param>
        public Button( string ラベル, VariablePath 通常時画像ファイル, VariablePath 押下時画像ファイル, VariablePath フォーカス時画像ファイル, 矩形リスト セル矩形, Size2F サイズdpx, Vector2? 位置dpx = null )
            : base( 通常時画像ファイル, セル矩形, サイズdpx, 位置dpx, 押下時画像ファイル, フォーカス時画像ファイル )
        {
            this.広い部分の描画方法 = 広い部分の描画方法種別.拡大縮小描画;
            this.ラベル = ラベル;

            this.子Activityを追加する( this._ボタンラベル = new Label(
                this.ラベル,
                サイズdpx ) {
                ParagraphAlignment = ParagraphAlignment.Center,
                TextAlignment = TextAlignment.Center,
                ヒットチェックあり = false,  // ボタンラベルはヒット処理しない
            } );
        }


        protected Label _ボタンラベル = null;


        protected override void OnPaint( DCEventArgs e )
        {
            base.OnPaint( e );

            this._ボタンラベル.テキスト = this.ラベル;
        }
    }
}
