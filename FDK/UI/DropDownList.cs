using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;

namespace FDK.UI
{
    public class DropDownList : Element
    {
        /// <summary>
        ///     項目の追加には、<see cref="項目を追加する(ListViewItem)"/>を使用すること。
        /// </summary>
        public SelectableList<ListViewItem> 項目リスト
            => this._選択欄.項目リスト;

        public DropDownList(
            VariablePath 表示欄画像ファイル,
            矩形リスト 表示欄セル矩形,
            VariablePath 選択欄画像ファイル,
            矩形リスト 選択欄セル矩形,
            VariablePath アローボタン通常時画像ファイル,
            VariablePath アローボタン押下時画像ファイル,
            VariablePath アローボタンフォーカス時画像ファイル,
            矩形リスト アローボタンセル矩形,
            Size2F サイズdpx,
            Vector2? 位置dpx = null )
            : base( サイズdpx, 位置dpx )
        {
            var 表示欄マージンdpx = 矩形リスト.Margin変換( 表示欄セル矩形[ "Margin" ].Value );
            this.子を追加する( this._表示欄 = new TextArea(
                表示欄画像ファイル,
                表示欄セル矩形,
                new Size2F(
                    this.サイズdpx.Width - 表示欄マージンdpx.Left - 表示欄マージンdpx.Right,
                    this.サイズdpx.Height - 表示欄マージンdpx.Top - 表示欄マージンdpx.Bottom ),
                new Vector2(
                    表示欄マージンdpx.Left,
                    表示欄マージンdpx.Top ) ) {
                テキスト = "",
            } );

            this._表示欄.MouseDown += this._表示欄_MouseDown;

            var 選択欄マージンdpx = 矩形リスト.Margin変換( 選択欄セル矩形[ "Margin" ].Value );
            this.子を追加する( this._選択欄 = new ListView(
                選択欄画像ファイル,
                選択欄セル矩形,
                new Size2F(
                    this.サイズdpx.Width - 選択欄マージンdpx.Left - 選択欄マージンdpx.Right,
                    200f ),
                new Vector2(
                    選択欄マージンdpx.Left,
                    選択欄マージンdpx.Top + this._表示欄.クライアント矩形dpx.Height + 表示欄マージンdpx.Bottom ) ) {
                可視 = false,
                再選択で選択を解除する = false,
                ウィンドウの高さを全項目数に合わせる = true,
            } );

            this._選択欄.Click += this._選択欄_Click;

            var ボタンマージンdpx = 矩形リスト.Margin変換( アローボタンセル矩形[ "Margin" ].Value );
            var ボタン幅dpx = Math.Max( 0f, this._表示欄.クライアント矩形dpx.Height - ボタンマージンdpx.Top - ボタンマージンdpx.Bottom );
            this.子を追加する( this._アローボタン = new Button(
                "-",
                アローボタン通常時画像ファイル,
                アローボタン押下時画像ファイル,
                アローボタンフォーカス時画像ファイル,
                アローボタンセル矩形,
                new Size2F( ボタン幅dpx, ボタン幅dpx ), // 正方形
                new Vector2(                           // 右寄せ
                    this._表示欄.位置dpx.X + this._表示欄.クライアント矩形dpx.Width - ボタン幅dpx - ボタンマージンdpx.Right,
                    表示欄マージンdpx.Top + ボタンマージンdpx.Top ) ) );

            this._アローボタン.MouseDown += this._アローボタン_MouseDown;

            this._表示欄と選択欄に合わせてサイズを更新する();
        }

        public void 項目を追加する( ListViewItem item )
        {
            item.サイズdpx = new Size2F( this._選択欄.クライアント矩形dpx.Width, this._選択欄.クライアント矩形dpx.Height );

            this._選択欄.項目を追加する( item );
            this._表示欄と選択欄に合わせてサイズを更新する();
        }


        protected TextArea _表示欄 = null;
        protected ListView _選択欄 = null;
        protected Button _アローボタン = null;
        
        protected override void On活性化()
        {
            // 項目があるのに未選択状態なら、最初の項目を選択する。
            if( 0 > this.項目リスト.SelectedIndex && 0 < this.項目リスト.Count )
                this._項目を選択する( 0 );

            base.On活性化();
        }


        private void _選択欄_Click( object sender, UIEventArgs e )
        {
            this._項目を選択する( this.項目リスト.SelectedIndex );
            this._選択欄を開閉する();
        }
        private void _アローボタン_MouseDown( object sender, UIMouseEventArgs e )
        {
            this._選択欄を開閉する();
        }
        private void _表示欄_MouseDown( object sender, UIMouseEventArgs e )
        {
            this._選択欄を開閉する();
        }

        private void _選択欄を開閉する()
        {
            this._選択欄.可視 = !( this._選択欄.可視 );
            this._表示欄と選択欄に合わせてサイズを更新する();
        }
        private void _項目を選択する( int index )
        {
            this.項目リスト.SelectItem( index );
            this._表示欄.テキスト = this.項目リスト.SelectedItem.テキスト;
        }
        private void _表示欄と選択欄に合わせてサイズを更新する()
        {
            this.サイズdpx = new Size2F(
                this.サイズdpx.Width,
                this._表示欄.サイズdpx_枠込み.Height + ( this._選択欄.可視 ? this._選択欄.サイズdpx_枠込み.Height : 0f ) );
        }
    }
}
