using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SSTFormat.v3;

namespace SSTFEditor
{
    /// <summary>
    ///		SSTFormat.チップ の機能拡張。
    /// </summary>
    class 描画用チップ : チップ
    {
        public int 譜面内絶対位置grid { get; set; } = 0;

        public bool ドラッグ操作により選択中である { get; set; } = false;

        public bool 選択が確定している { get; set; } = false;
        public bool 選択が確定していない
        {
            get => !this.選択が確定している;
            set => this.選択が確定している = !value;
        }

        public bool 移動済みである { get; set; } = true;
        public bool 移動されていない
        {
            get => !this.移動済みである;
            set => this.移動済みである = !value;
        }

        public string チップ内文字列 { get; set; } = null;

        public int 枠外レーン数 { get; set; } = 0;


        private void _特別なチップ内文字列を設定する()
        {
            if( this.チップ種別 == チップ種別.China ) this.チップ内文字列 = "C N";
            if( this.チップ種別 == チップ種別.Splash ) this.チップ内文字列 = "S P";
            if( this.チップ種別 == チップ種別.BPM ) this.チップ内文字列 = this.BPM.ToString( "###.##" );
        }
    }
}
