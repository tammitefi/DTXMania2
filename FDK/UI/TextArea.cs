using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace FDK.UI
{
    public class TextArea : Window
    {
        public string テキスト
        {
            get
                => this.ラベル.テキスト;

            set
                => this.ラベル.テキスト = value;
        }

        public TextArea( VariablePath 画像ファイル, 矩形リスト セル矩形, Size2F サイズdpx, Vector2? 位置dpx = null, float fontSize = 18f )
            : base( 画像ファイル, セル矩形, サイズdpx, 位置dpx )
        {
            this.子を追加する( this.ラベル = new Label(
                "テキスト\nエリア",
                サイズdpx ) {
                フォントサイズpt = fontSize,
                ヒットチェックあり = false,
                複数行表示 = true,
            } );
        }

        protected Label ラベル = null;
    }
}
