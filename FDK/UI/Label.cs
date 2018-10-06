using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK.メディア;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace FDK.UI
{
    public class Label : Element
    {
        public string テキスト
        {
            get
                => this._文字列画像.表示文字列;
            set
                => this._文字列画像.表示文字列 = value;
        }
        public Color4 文字色
        {
            get
                => this._文字列画像.前景色;
            set
                => this._文字列画像.前景色 = value;
        }
        /// <summary>
        ///     true にすると、<see cref="テキスト"/> に含まれる '\n' が改行文字として作用する。
        ///     false にすると、'\n' は単純に無視される（単一行表示）。
        /// </summary>
        public bool 複数行表示
        {
            get;
            set;
        } = true;
        public override Size2F サイズdpx
        {
            get
                => this._文字列画像.画像サイズdpx;
        }

        public string フォント名
        {
            get
                => this._文字列画像.フォント名;
            set
                => this._文字列画像.フォント名 = value;
        }
        public float フォントサイズpt
        {
            get
                => this._文字列画像.フォントサイズpt;
            set
                => this._文字列画像.フォントサイズpt = value;
        }
        public FontWeight フォント幅
        {
            get
                => this._文字列画像.フォント幅;
            set
                => this._文字列画像.フォント幅 = value;
        }
        public FontStyle フォントスタイル
        {
            get
                => this._文字列画像.フォントスタイル;
            set
                => this._文字列画像.フォントスタイル = value;
        }
        public WordWrapping WordWrapping
        {
            get
                => this._文字列画像.WordWrapping;
            set
                => this._文字列画像.WordWrapping = value;
        }
        public ParagraphAlignment ParagraphAlignment
        {
            get 
                => this._文字列画像.ParagraphAlignment;
            set
                => this._文字列画像.ParagraphAlignment = value;
        }
        public TextAlignment TextAlignment
        {
            get
                => this._文字列画像.TextAlignment;
            set
                => this._文字列画像.TextAlignment = value;
        }
        public float LineSpacing
            => this._文字列画像.LineSpacing;
        public float Baseline
            => this._文字列画像.Baseline;

        public bool 加算合成
        {
            get
                => this._文字列画像.加算合成;
            set
                => this._文字列画像.加算合成 = value;
        }
        public 文字列画像.効果 描画効果
        {
            get
                => this._文字列画像.描画効果;
            set
                => this._文字列画像.描画効果 = value;
        }
        /// <summary>
        ///		効果が縁取りのときのみ有効。
        /// </summary>
        public float 縁のサイズdpx
        {
            get
                => this._文字列画像.縁のサイズdpx;
            set
                => this._文字列画像.縁のサイズdpx = value;
        }
        public InterpolationMode 補正モード
        {
            get
                => this._文字列画像.補正モード;
            set
                => this._文字列画像.補正モード = value;
        }
        public RectangleF? 転送元矩形
        {
            get
                => this._文字列画像.転送元矩形;
            set
                => this._文字列画像.転送元矩形 = value;
        }

        public Label( string テキスト, Size2F サイズdpx, Vector2? 位置dpx = null )
            :base( サイズdpx, 位置dpx )
        {
            this.子を追加する( this._文字列画像 = new 文字列画像() {
                フォント名 = "メイリオ",
                フォント幅 = FontWeight.Normal,
                フォントスタイル = FontStyle.Normal,
                フォントサイズpt = 18f,
                WordWrapping = WordWrapping.EmergencyBreak,
                ParagraphAlignment = ParagraphAlignment.Near,
                TextAlignment = TextAlignment.Leading,
                レイアウトサイズdpx = サイズdpx,
            } );

            this.テキスト = テキスト;
        }

        public void 画像を更新する()
        {
            this._文字列画像.ビットマップを生成または更新する();
        }


        protected 文字列画像 _文字列画像 = null;

        protected override void OnPaint( DCEventArgs e )
        {
            base.OnPaint( e );

            // 複数行表示じゃないなら改行を削除。
            this._文字列画像.表示文字列 = ( this.複数行表示 ) ? this.テキスト : this.テキスト.Replace( "\n", "" );

            this._文字列画像.描画する( e.dc, this.クライアント矩形のスクリーン座標dpx.X, this.クライアント矩形のスクリーン座標dpx.Y );
        }
    }
}
