using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct2D1;

namespace FDK.UI
{
    public class ListView : Window
    {
        /// <summary>
        ///     項目の追加には、<see cref="項目を追加する(ListViewItem)"/>を使用すること。
        /// </summary>
        public SelectableList<ListViewItem> 項目リスト
        {
            get;
            protected set;
        } = new SelectableList<ListViewItem>();
        public ListViewItem 先頭に表示されている項目
        {
            get
                => this.項目リスト[ this._先頭に表示されている項目番号 ];
            set
                => this._先頭に表示されている項目番号 = this.項目リスト.IndexOf( value );
        }
        /// <summary>
        ///     どの項目もフォーカスされていない場合は null 。
        /// </summary>
        public ListViewItem フォーカスされている項目
        {
            get
                => ( 0 <= this._フォーカスされている項目番号 ) ? 
                    this.項目リスト[ this._フォーカスされている項目番号 ] : null;
            set
                => this._フォーカスされている項目番号 = this.項目リスト.IndexOf( value );
        }
        public ListViewItem 選択されている項目
            => this.項目リスト.SelectedItem;
        public int 選択されている項目番号
            => this.項目リスト.SelectedIndex;
        public override Size2F サイズdpx
        {
            get
                => base.サイズdpx;

            set
                => base.サイズdpx = value;
        }

        public bool 再選択で選択を解除する
        {
            get;
            set;
        } = true;
        public bool ウィンドウの高さを全項目数に合わせる
        {
            get;
            set;
        } = false;

        public ListView( VariablePath 画像ファイル, 矩形リスト セル矩形, Size2F サイズdpx, Vector2? 位置dpx = null )
            : base( 画像ファイル, セル矩形, サイズdpx, 位置dpx )
        {
            this.広い部分の描画方法 = 広い部分の描画方法種別.タイル描画;

            this._先頭に表示されている項目番号 = 0;
            this._フォーカスされている項目番号 = -1;

            this.子Activityを追加する( this.ウィンドウ画像 = new 描画可能画像( this.サイズdpx ) );
        }

        public void 項目を追加する( ListViewItem item )
        {
            item.サイズdpx = new Size2F( this.クライアント矩形dpx.Width, this.クライアント矩形dpx.Height );

            this.項目リスト.Add( item );
            this.子Activityを追加する( item );
        }

        /// <summary>
        ///     指定された番号の項目が表示されるまでスクロールする。
        /// </summary>
        /// <param name="index">表示を保証したい項目番号。</param>
        public void 項目の表示を保証する( int index )
        {
            if( 0 == this.項目リスト.Count )
                return;

            if( ( 0 > index ) || ( index >= this.項目リスト.Count ) )
                throw new IndexOutOfRangeException();

            while( index < this._先頭に表示されている項目番号 )
                this._先頭に表示されている項目番号--;

            while( index >= this._先頭に表示されている項目番号 + this.表示可能な行数_末尾はみ出しなし )
                this._先頭に表示されている項目番号++;
        }

        protected override void On活性化()
        {
            // 項目があるのに未選択状態なら、最初の項目を選択する。
            if( 0 > this.項目リスト.SelectedIndex && 0 < this.項目リスト.Count )
                this.項目リスト.SelectItem( 0 );

            base.On活性化();
        }

        protected 描画可能画像 ウィンドウ画像 = null;
        protected int _先頭に表示されている項目番号 = 0;
        protected int _フォーカスされている項目番号 = -1;

        /// <summary>
        ///     現在の サイズdpx とラベルの LineSpacing から、表示できる行の数を返す。
        ///     末尾の行が一部しか見えない場合もカウントされる。
        /// </summary>
        protected int 表示可能な行数
        {
            get
            {
                int n = 0;

                if( 0 < this.項目リスト.Count )
                {
                    this.項目リスト[ 0 ].画像を更新する();
                    n = (int) Math.Ceiling( this.サイズdpx.Height / this.項目リスト[ 0 ].サイズdpx.Height );    // 項目リスト[0] が代表
                }

                return n;
            }
        }

        /// <summary>
        ///     現在の サイズdpx とラベルの LineSpacing から、表示できる行の数を返す。
        ///     末尾の行が一部しか見えない場合はカウントされない。
        /// </summary>
        protected int 表示可能な行数_末尾はみ出しなし
        {
            get
            {
                int n = 0;

                if( 0 < this.項目リスト.Count )
                {
                    this.項目リスト[ 0 ].画像を更新する();
                    n = (int) Math.Floor( this.サイズdpx.Height / this.項目リスト[ 0 ].サイズdpx.Height );    // 項目リスト[0] が代表
                }

                return n;
            }
        }

        /// <summary>
        ///     指定した位置がヒットしている項目の番号を返す。
        ///     いずれにもヒットしてなければ -1 を返す。
        /// </summary>
        /// <param name="位置dpx">スクリーン座標[dpx]。</param>
        protected int ヒットしている項目の番号を返す( Vector2 位置dpx )
        {
            for( int i = 0; ( i < this.表示可能な行数 ) && ( ( this._先頭に表示されている項目番号 + i ) < this.項目リスト.Count ); i++ )
            {
                int itemIndex = this._先頭に表示されている項目番号 + i;
                var item = this.項目リスト[ itemIndex ];
                var itemRect = new RectangleF( item.ウィンドウ矩形dpx.X, item.ウィンドウ矩形dpx.Y, this.クライアント矩形dpx.Width, item.ウィンドウ矩形dpx.Height );  // 幅は ListView のクライアント幅いっぱい

                if( item.可視 && itemRect.Contains( 位置dpx ) )
                    return itemIndex;
            }

            return -1;
        }

        protected override void OnPaint( DCEventArgs e )
        {
            base.OnPaint( e );

            if( this.ウィンドウの高さを全項目数に合わせる )   // オプション
            {
                float 高さdpx = 0f;
                foreach( var item in this.項目リスト )
                    高さdpx += item.サイズdpx.Height;
                var 高さに合わせたサイズdpx = new Size2F( base.サイズdpx.Width, 高さdpx );

                if( this.サイズdpx != 高さに合わせたサイズdpx )
                {
                    this.サイズdpx = 高さに合わせたサイズdpx;

                    this.ウィンドウ画像.非活性化する();
                    this.子Activityを削除する( this.ウィンドウ画像 );
                    this.子Activityを追加する( this.ウィンドウ画像 = new 描画可能画像( this.サイズdpx ) );
                    this.ウィンドウ画像.活性化する();
                }
            }

            if( 0 == this.項目リスト.Count )
                return;

            this.ウィンドウ画像.画像へ描画する( ( dcw ) => {

                dcw.Clear( Color.Transparent );

                // 全項目を不可視で初期化。
                foreach( var 子 in this.子Activityリスト )
                {
                    if( 子 is ListViewItem item )
                        item.可視 = false;
                }

                float y = 0;
                for( int i = 0; ( i < this.表示可能な行数 ) && ( ( this._先頭に表示されている項目番号 + i ) < this.項目リスト.Count ); i++ )
                {
                    var 表示する項目番号 = this._先頭に表示されている項目番号 + i;
                    var 表示する項目 = this.項目リスト[ 表示する項目番号 ];
                    var 表示する項目の矩形dpx = new RectangleF( 0f, y, this.クライアント矩形dpx.Width, 表示する項目.サイズdpx.Height );   // クライアント座標[dpx]

                    // 可視化。
                    表示する項目.可視 = true;

                    // 選択項目背景を描画。
                    if( this.項目リスト.SelectedIndex == 表示する項目番号 )
                    {
                        using( var brush = new SolidColorBrush( dcw, Color.Gray ) )
                            dcw.FillRectangle( 表示する項目の矩形dpx, brush );
                    }

                    // フォーカス枠を描画。
                    if( this._フォーカスされている項目番号 == 表示する項目番号 )
                    {
                        using( var brush = new SolidColorBrush( dcw, Color.White ) )
                        using( var style = new StrokeStyle( グラフィックデバイス.Instance.D2DFactory, new StrokeStyleProperties() {
                            StartCap = CapStyle.Flat,
                            EndCap = CapStyle.Flat,
                            DashCap = CapStyle.Round,
                            LineJoin = LineJoin.Miter,
                            MiterLimit = 10.0f,
                            DashStyle = DashStyle.DashDotDot,
                            DashOffset = 0.0f,
                        } ) )
                        {
                            dcw.DrawRectangle( 表示する項目の矩形dpx, brush, 0.5f, style );
                        }
                    }

                    // 項目を描画。
                    表示する項目.位置dpx = new Vector2( 0f, y );

                    y += 表示する項目.サイズdpx.Height;
                }

            } );

            this.ウィンドウ画像.描画する( e.dc, this.クライアント矩形のスクリーン座標dpx.X, this.クライアント矩形のスクリーン座標dpx.Y );
        }
        protected override void OnMouseLeave( UIEventArgs e )
        {
            base.OnMouseLeave( e );

            this._フォーカスされている項目番号 = -1;
        }
        protected override bool OnMouseMove( UIMouseEventArgs e )
        {
            int hit = this.ヒットしている項目の番号を返す( e.マウス位置dpx );

            if( 0 <= hit )
                this._フォーカスされている項目番号 = hit;

            return base.OnMouseMove( e );
        }
        protected override bool OnClick( UIEventArgs e )
        {
            int hit = this.ヒットしている項目の番号を返す( e.マウス位置dpx );

            if( 0 <= hit )
            {
                if( this.項目リスト.SelectedIndex != hit )
                {
                    this.項目リスト.SelectItem( hit );
                }
                else
                {
                    // 選択中の項目をクリックした場合
                    if( this.再選択で選択を解除する )
                        this.項目リスト.SelectItem( -1 );    // 選択解除
                }
            }

            return base.OnClick( e );
        }
    }
}
