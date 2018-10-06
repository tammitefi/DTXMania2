using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FDK.メディア;
using FDK.カウンタ;
using System.Windows.Forms;

namespace FDK.UI
{
    public class TextBox : Window
    {
        public string テキスト
        {
            get
                => this._テキスト;
            set
            {
                var 更新前テキスト = this._テキスト;
                var 更新後テキスト = value?.Replace( "\n", "" ) ?? "";    // 念のため改行を削除, null 不可

                if( 更新前テキスト != 更新後テキスト )
                {
                    this._テキスト = 更新後テキスト;
                    this._TextLayoutを再構築せよ = true;  // 文字列が変わったら作り直す
                }
            }
        }

        public TextBox( string テキスト, VariablePath 画像ファイル, 矩形リスト セル矩形, Size2F サイズdpx, Vector2? 位置dpx = null, float fontSize = 18f )
            : base( 画像ファイル, セル矩形, サイズdpx, 位置dpx )
        {
            this.テキスト = テキスト;
            this._フォントサイズpt = fontSize;
        }

        protected override void On活性化()
        {
            this._TextFormat = new TextFormat(
                グラフィックデバイス.Instance.DWriteFactory,
                "メイリオ",
                FontWeight.Normal,
                FontStyle.Normal,
                FontStretch.Normal,
                this._フォントサイズpt );

            this._TextRenderer = new カスタムTextRenderer(
                グラフィックデバイス.Instance.D2DFactory,
                既定の文字色: Color.White,
                既定の背景色: Color.Transparent );

            this._TextLayoutを再構築せよ = true;

            this._通常文字のDrawingEffect = new カスタムTextRenderer.DrawingEffect() {
                文字の色 = Color.White,
                背景の色 = Color.Transparent,
            };

            this._選択文字のDrawingEffect = new カスタムTextRenderer.DrawingEffect() {
                文字の色 = Color.White,
                背景の色 = Color.Gray,
            };

            this._キャレット点滅アニメリセット();

            base.On活性化();
        }
        protected override void On非活性化()
        {
            this._選択文字のDrawingEffect?.Dispose();
            this._選択文字のDrawingEffect = null;

            this._通常文字のDrawingEffect?.Dispose();
            this._通常文字のDrawingEffect = null;

            this._TextRenderer?.Dispose();
            this._TextRenderer = null;

            this._TextLayout?.Dispose();
            this._TextLayout = null;

            this._TextFormat?.Dispose();
            this._TextFormat = null;

            base.On非活性化();
        }

        protected override void OnPaint( DCEventArgs e )
        {
            // Window を描画。
            base.OnPaint( e );

            // テキストを描画。
            if( this._TextLayoutを再構築せよ )
            {
                this._TextLayout = new TextLayout(
                    グラフィックデバイス.Instance.DWriteFactory,
                    this.テキスト,
                    this._TextFormat,
                    this.クライアント矩形dpx.Width,
                    this.クライアント矩形dpx.Height );

                this._TextLayoutを再構築せよ = false;
            }

            this._通常文字のDrawingEffect.renderTarget = e.dc;
            this._選択文字のDrawingEffect.renderTarget = e.dc;

            this._TextLayout.Draw(
                this._TextRenderer,
                this.クライアント矩形のスクリーン座標dpx.X,
                this.クライアント矩形のスクリーン座標dpx.Y );

            // キャレットを描画。
            if( ( 0 <= this._キャレット位置 ) &&
                ( 0 == this._キャレット点滅アニメ用カウンタ.現在値 ) )    // 0 の時表示、1 の時非表示
            {
                var metrics = this._TextLayout.HitTestTextPosition(
                    this._キャレット位置,
                    false,
                    out float left,
                    out float top );

                var rc = new RectangleF(
                    this.クライアント矩形のスクリーン座標dpx.X + left,
                    this.クライアント矩形のスクリーン座標dpx.Y + top,
                    2f,
                    FDKUtilities.変換_pt単位からpx単位へ( e.dc.DotsPerInch.Width, this._フォントサイズpt ) );

                using( var brush = new SolidColorBrush( e.dc, Color.White ) )
                    e.dc.FillRectangle( rc, brush );
            }
        }
        protected override void OnGotFocus( UIEventArgs e )
        {
            this._文字列をすべて選択する();
            base.OnGotFocus( e );
        }
        protected override void OnLostFocus( UIEventArgs e )
        {
            this._文字列の選択を解除する();
            this._キャレット位置 = -1;

            base.OnLostFocus( e );
        }
        protected override bool OnMouseDown( UIMouseEventArgs e )
        {
            if( this.フォーカス中 )
            {
                // (A) フォーカス中である。

                double 現在時刻 = QPCTimer.生カウント相対値を秒へ変換して返す( QPCTimer.生カウント );
                bool ダブルクリックとみなす時間内である = ( SystemInformation.DoubleClickTime > ( 現在時刻 - this._前回のMouseDown時刻 ) * 1000.0 );
                bool ダブルクリックとみなす位置内である =
                    ( Math.Abs( e.マウス位置dpx.X - this._前回のMouseDown位置.X ) <= SystemInformation.DoubleClickSize.Width ) &&
                    ( Math.Abs( e.マウス位置dpx.Y - this._前回のMouseDown位置.Y ) <= SystemInformation.DoubleClickSize.Height );

                if( ダブルクリックとみなす時間内である && ダブルクリックとみなす位置内である )
                {
                    // (A-a) ダブルクリックの２回目。

                    this._文字列をすべて選択する();

                    this._前回のMouseDown時刻 = 0;
                    this._前回のMouseDown位置 = new Vector2( -1000f, -1000f );
                }
                else
                {
                    // (A-b) シングルクリック、またはダブルクリックの１回目。

                    this._前回のMouseDown時刻 = 現在時刻;
                    this._前回のMouseDown位置 = e.マウス位置dpx;

                    this._文字列の選択を解除する();

                    var pointMetrics = this._TextLayout.HitTestPoint(
                        e.マウス位置dpx.X - this.クライアント矩形のスクリーン座標dpx.X,
                        e.マウス位置dpx.Y - this.クライアント矩形のスクリーン座標dpx.Y,
                        out SharpDX.Mathematics.Interop.RawBool isTrailingHit,
                        out SharpDX.Mathematics.Interop.RawBool isInside );

                    this._キャレット位置 = ( isInside ) ? pointMetrics.TextPosition : pointMetrics.TextPosition + 1;

                    this._キャレット点滅アニメリセット();
                }
            }
            else
            {
                // (B) フォーカスされていない。
                // → 何もしない。このあと OnGetFocus が呼び出され、フォーカスを得るだろう。
            }

            return base.OnMouseDown( e );
        }
        protected override void OnKeyDown( KeyEventArgs e )
        {
            // キャレット位置が無効なら先頭へ。
            if( 0 > this._キャレット位置 )
                this._キャレット位置 = 0;

            // 選択している部分があるなら、選択範囲を削除して選択を解除。
            if( 0 < this._選択範囲.Length )
            {
                this.テキスト = this.テキスト.Remove( this._選択範囲.StartPosition, this._選択範囲.Length );
                this._キャレット位置 = this._選択範囲.StartPosition;

                this._文字列の選択を解除する();
            }

            

            // １文字挿入。
            this.テキスト = this.テキスト.Insert( this._キャレット位置, e.KeyCode.ToString() );
            this._キャレット位置++;

            base.OnKeyDown( e );
        }

        private string _テキスト = "";  // null 不可
        private float _フォントサイズpt;
        private TextFormat _TextFormat;
        private TextLayout _TextLayout;
        private カスタムTextRenderer _TextRenderer;
        private bool _TextLayoutを再構築せよ = true;
        private カスタムTextRenderer.DrawingEffect _通常文字のDrawingEffect;
        private カスタムTextRenderer.DrawingEffect _選択文字のDrawingEffect;

        private LoopCounter _キャレット点滅アニメ用カウンタ;
        private double _前回のMouseDown時刻 = 0.0;
        private Vector2 _前回のMouseDown位置 = new Vector2( -1000f, -1000f );

        /// <summary>
        ///     (0,0) で未選択。
        /// </summary>
        private TextRange _選択範囲;

        /// <summary>
        ///     キャレットのある文字位置。(0～ _テキスト.Length-1)
        ///     負数でカーソル表示なし。
        /// </summary>
        private int _キャレット位置 = -1;

        private void _文字列をすべて選択する()
        {
            this._選択範囲 = new TextRange( 0, this._テキスト.Length );
            this._TextLayout.SetDrawingEffect( _選択文字のDrawingEffect, this._選択範囲 );

            this._キャレット位置 = -1;
        }
        private void _文字列の選択を解除する()
        {
            this._選択範囲 = new TextRange( 0, 0 );
            this._TextLayout.SetDrawingEffect( _通常文字のDrawingEffect, new TextRange( 0, this._テキスト.Length ) );
        }

        private void _キャレット点滅アニメリセット()
        {
            this._キャレット点滅アニメ用カウンタ = new LoopCounter( 0, 1, 400 );   // 400ms で点滅
        }
    }
}
