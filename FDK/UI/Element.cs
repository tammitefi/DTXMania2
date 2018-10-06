using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace FDK.UI
{
    public class Element : Activity
    {
        /// <summary>
        ///     親要素の左上隅から自要素のクライアント矩形の左上隅までの相対位置[dpx]。
        /// </summary>
        public virtual Vector2 位置dpx
        {
            get;
            set;
        } = new Vector2( 0f, 0f );

        /// <summary>
        ///     自要素のクライアント矩形の幅と高さ[dpx]。
        /// </summary>
        public virtual Size2F サイズdpx
        {
            get;
            set;
        } = new Size2F( 0f, 0f );
        
        /// <summary>
        ///     自要素の背景色。
        /// </summary>
        public virtual Color4 背景色
        {
            get;
            set;
        } = Color.Transparent;
        
        /// <summary>
        ///     true にすると、自分とその子孫が非表示かつヒットしないようになる。
        /// </summary>
        public virtual bool 可視
        {
            get;
            set;
        } = true;
        
        /// <summary>
        ///     各種マウスイベントに反応するなら true。
        ///     false にすると、これらのイベントの対象にならない。
        /// </summary>
        public virtual bool ヒットチェックあり
        {
            get;
            set;
        } = true;
        
        /// <summary>
        ///     自要素のクライアント矩形。
        ///     スクリーン座標[dpx]。
        /// </summary>
        public virtual RectangleF クライアント矩形のスクリーン座標dpx
        {
            get
            {
                var rc = new RectangleF( this.位置dpx.X, this.位置dpx.Y, this.サイズdpx.Width, this.サイズdpx.Height );

                if( ( null != this.親 ) && ( this.親 is Element 親要素 ) )
                {
                    var parentRect = 親要素.クライアント矩形のスクリーン座標dpx;
                    rc.Offset( parentRect.X, parentRect.Y );
                }

                return rc;
            }
        }
        
        /// <summary>
        ///     自要素のクライアント矩形。
        ///     親要素の左上隅を原点とする相対位置[dpx]。
        /// </summary>
        public virtual RectangleF クライアント矩形dpx
        {
            get
                => new RectangleF( this.位置dpx.X, this.位置dpx.Y, this.サイズdpx.Width, this.サイズdpx.Height );
        }
        
        /// <summary>
        ///     自要素のウィンドウ矩形。
        ///     スクリーン座標[dpx]。
        /// </summary>
        public virtual RectangleF ウィンドウ矩形dpx
        {
            get
                => this.クライアント矩形のスクリーン座標dpx;

            // ウィンドウ矩形がクライアント矩形と異なる場合は、このメソッドをオーバーライドすること。
        }

        /// <summary>
        ///     現在フォーカスされていれば true 。
        /// </summary>
        public bool フォーカス中
        {
            get;
            protected set;
        } = false;

        public event EventHandler<DCEventArgs> Paint;
        public event EventHandler<UIMouseEventArgs> MouseDown;
        public event EventHandler<UIEventArgs> Click;
        public event EventHandler<UIMouseEventArgs> MouseUp;
        public event EventHandler<UIMouseEventArgs> MouseMove;
        public event EventHandler<UIEventArgs> MouseEnter;
        public event EventHandler<UIEventArgs> MouseLeave;
        public event EventHandler<UIEventArgs> GotFocus;
        public event EventHandler<UIEventArgs> LostFocus;
        public event KeyEventHandler KeyDown;

        public Element()
        {
        }
        public Element( Size2F サイズdpx, Vector2? 位置dpx = null )
            : this()
        {
            this.サイズdpx = サイズdpx;
            this.位置dpx = 位置dpx ?? Vector2.Zero;
        }

        /// <remarks>
        ///     このクラスだけでなくこのクラスからの派生クラスすべてで実行する必要があるコードは、<see cref="On活性化(グラフィックデバイス)"/> ではなくここで実行する。
        /// </remarks>
        public override void 活性化する()
        {
            this._設計画面サイズdpx = グラフィックデバイス.Instance.設計画面サイズ;

            base.活性化する();
        }

        /// <summary>
        ///     指定した位置が自要素のクライアント矩形に収まっていれば true を返す。
        /// </summary>
        /// <param name="位置dpx">
        ///     調査する位置。スクリーン座標[dpx]。
        /// </param>
        public virtual bool ヒットチェック( Vector2 位置dpx )
        {
            return this.クライアント矩形のスクリーン座標dpx.Contains( 位置dpx );
            // ヒット範囲がクライアント矩形と異なる場合は、このメソッドをオーバーライドすること。
        }

        /// <summary>
        ///     自要素をフォーカスする。
        /// </summary>
        public void フォーカスする( UIEventArgs e )
        {
            // Element ツリーのルート要素を取得。
            var Root = this;
            while( ( null != Root.親 ) && ( Root.親 is Element 親要素 ) )
                Root = 親要素;

            // 自分をフォーカスすることをルート要素に通知。
            Root._フォーカスを通知する( this, e );
        }

        /// <summary>
        ///     自要素と子孫要素の描画を行う。
        /// </summary>
        public virtual void 描画する( DCEventArgs e )
        {
            if( this.活性化していない || !( this.可視 ) )
                return;

            // ウィンドウ矩形を親のクライアント矩形でクリッピングする。

            var クリッピング後のウィンドウ矩形dpx = this.ウィンドウ矩形dpx;

            if( ( null != this.親 ) && ( this.親 is Element 親要素 ) )
            {
                // 親があるなら、親のクライアント矩形（スクリーン座標）でクリッピング。
                クリッピング後のウィンドウ矩形dpx = RectangleF.Intersect( クリッピング後のウィンドウ矩形dpx, 親要素.クライアント矩形のスクリーン座標dpx );
            }

            // クリッピング後のウィンドウ矩形内に、自分を描画する。
            グラフィックデバイス.Instance.D2DBatchDraw( e.dc, () => {

                e.dc.PushAxisAlignedClip( クリッピング後のウィンドウ矩形dpx, AntialiasMode.PerPrimitive );

                // 背景色で塗りつぶす。
                if( this.背景色 != Color.Transparent ) // 一応効率化
                    e.dc.Clear( new Color( this.背景色 ), this.ウィンドウ矩形dpx );

                // 自分を描画する。
                this.OnPaint( e );

                // Paint イベント発行。
                this.Paint?.Invoke( this, e );

                e.dc.PopAxisAlignedClip();

            } );

            // クライアント矩形内に、子要素を昇順に描画する。
            for( int i = 0; i < this.子リスト.Count; i++ )
            {
                if( this.子リスト[ i ] is Element 子要素 )
                {
                    if( 子要素.活性化していない )
                        子要素.活性化する();

                    子要素.描画する( e );
                }
            }
        }

        /// <summary>
        ///     自分または子がフォーカスされていれば、<see cref="OnKeyDown(KeyEventArgs)"/>を実行する。
        /// </summary>
        public void InvokeKeyDown( KeyEventArgs e )
        {
            if( this.フォーカス中 && this.可視 )
            {
                this.OnKeyDown( e );
            }
            else
            {
                // 子要素を降順（描画順の逆）にチェックする。
                for( int i = this.子リスト.Count - 1; i >= 0; i-- )
                {
                    if( this.子リスト[ i ] is Element 子要素 )
                        子要素.InvokeKeyDown( e );
                }
            }
        }

        protected virtual void OnPaint( DCEventArgs e )
        {
        }
        protected virtual bool OnMouseDown( UIMouseEventArgs e )
        {
            return this._要素の深さ優先ヒットチェック(

                e.マウス位置dpx,

                要素ヒット時の処理: ( 要素 ) => {

                    if( 要素 == this )
                    {
                        Debug.WriteLine( $"MouseDown: {this.GetType().ToString()}, {e.マウス位置dpx}" );

                        // MouseDown イベント発行。
                        this.MouseDown?.Invoke( this, e );

                        // フォーカス取得。
                        if( !( this.フォーカス中 ) )
                            this.フォーカスする( e );

                        return true;
                    }
                    else
                    {
                        return 要素.OnMouseDown( e );
                    }

                } );
        }
        protected virtual bool OnClick( UIEventArgs e )
        {
            return this._要素の深さ優先ヒットチェック(

                e.マウス位置dpx,

                要素ヒット時の処理: ( 要素 ) => {

                    if( 要素 == this )
                    {
                        Debug.WriteLine( $"Click: {this.GetType().ToString()}" );
                        this.Click?.Invoke( this, e );
                        return true;
                    }
                    else
                    {
                        return 要素.OnClick( e );
                    }

                } );
        }
        protected virtual bool OnMouseUp( UIMouseEventArgs e )
        {
            return this._要素の深さ優先ヒットチェック(

                e.マウス位置dpx,

                要素ヒット時の処理: ( 要素 ) => {

                    if( 要素 == this )
                    {
                        Debug.WriteLine( $"MouseUp: {this.GetType().ToString()}, {e.マウス位置dpx}" );
                        this.MouseUp?.Invoke( this, e );
                        return true;
                    }
                    else
                    {
                        return 要素.OnMouseUp( e );
                    }

                } );
        }
        protected virtual bool OnMouseMove( UIMouseEventArgs e )
        {
            // (1) MouseEnter/Leave は、マウス位置にヒットするすべての要素に発生する。
            this._MouseEnterまたはMouseLeaveが発生するかチェックする( e );

            // (2) MouseMove は、マウス位置にヒットする要素つのうち、一番深い（上位の）要素にのみ発生する。
            var キャッチした = this._MouseMoveのヒットチェック( e );

            return キャッチした;
        }
        protected virtual void OnMouseEnter( UIEventArgs e )
        {
            if( this.ヒットチェックあり )
            {
                Debug.WriteLine( $"MouseEnter: {this.GetType().ToString()}" );
                this.MouseEnter?.Invoke( this, e );
            }
        }
        protected virtual void OnMouseLeave( UIEventArgs e )
        {
            if( this.ヒットチェックあり )
            {
                Debug.WriteLine( $"MouseLeave: {this.GetType().ToString()}" );
                this.MouseLeave?.Invoke( this, e );
            }
        }
        protected virtual void OnGotFocus( UIEventArgs e )
        {
            Debug.WriteLine( $"GotFocus: {this.GetType().ToString()}" );
            this.GotFocus?.Invoke( this, e );
        }
        protected virtual void OnLostFocus( UIEventArgs e )
        {
            Debug.WriteLine( $"LostFocus: {this.GetType().ToString()}" );
            this.LostFocus?.Invoke( this, e );
        }
        protected virtual void OnKeyDown( KeyEventArgs e )
        {
            Debug.WriteLine( $"KeyDown: {this.GetType().ToString()}: {e.KeyCode}" );
            this.KeyDown?.Invoke( this, e );
        }

        private Vector2 _前回のマウス位置dpx = new Vector2( float.MinValue );
        private Size2F _設計画面サイズdpx;

        /// <param name="マウス位置dpx">
        ///     マウス位置。スクリーン座標[dpx]。
        /// </param>
        /// <param name="子要素ヒット時の処理">
        ///     ヒット処理した場合は true を返す。
        /// </param>
        private bool _要素の深さ優先ヒットチェック( Vector2 マウス位置dpx, Func<Element, bool> 要素ヒット時の処理 )
        {
            return すべての子要素のヒットチェック( this );

            bool すべての子要素のヒットチェック( Element 要素 )
            {
                bool キャッチした = false;

                // 子要素を降順（描画順の逆）にチェックする。
                for( int i = 要素.子リスト.Count - 1; i >= 0; i-- )
                {
                    if( 要素.子リスト[ i ] is Element 子要素 &&
                        子要素.可視 &&
                        //子要素.ヒットチェックあり &&      // ヒットチェックなしの場合でも、その子孫はチェック対象。
                        子要素.ヒットチェック( マウス位置dpx ) )
                    {
                        if( すべての子要素のヒットチェック( 子要素 ) )   // 深さ優先（子優先）
                        {
                            キャッチした = true;
                            break;
                        }
                    }
                }

                // すべての子がヒットしなかった場合のみ、自分をヒットチェックする。
                if( !( キャッチした ) &&
                    this.ヒットチェックあり &&
                    this.ヒットチェック( マウス位置dpx ) )
                {
                    if( 要素ヒット時の処理( 要素 ) )
                        キャッチした = true;
                }

                return キャッチした;
            }
        }

        private void _MouseEnterまたはMouseLeaveが発生するかチェックする( UIMouseEventArgs e )
        {
            // (1) 自分についてチェック。

            if( this.可視 )
            {
                if( this.ヒットチェックあり )
                {
                    bool 前回ヒットした = this.ヒットチェック( this._前回のマウス位置dpx );
                    bool 今回ヒットした = this.ヒットチェック( e.マウス位置dpx );

                    this._前回のマウス位置dpx = e.マウス位置dpx; // このメソッド内で更新すること。

                    if( 今回ヒットした )
                    {
                        if( 前回ヒットした )
                        {
                            // (A) 前回も今回も自要素内 → 何もしない
                        }
                        else
                        {
                            // (B) 前回自要素外、今回自要素内 → MouseEnter 発生
                            Debug.WriteLine( $"MouseEnter: {this.GetType().ToString()}, {e.マウス位置dpx}" );
                            this.OnMouseEnter( e );
                            this.MouseEnter?.Invoke( this, e );
                        }
                    }
                    else
                    {
                        if( 前回ヒットした )
                        {
                            // (C) 前回自要素内、今回自要素外 → MouseLeave 発生
                            Debug.WriteLine( $"MouseLeave: {this.GetType().ToString()}, {e.マウス位置dpx}" );
                            this.OnMouseLeave( e );
                            this.MouseLeave?.Invoke( this, e );
                        }
                        else
                        {
                            // (D) 前回も今回も自要素外 → 何もしない
                        }
                    }
                }

                // (2) すべての子要素についてチェック。
                // 　　親が不可視なら子はチェックしないが、親がヒットチェックなしでも子はチェックする。
                foreach( var 子 in this.子リスト )
                {
                    if( 子 is Element 子要素 )
                    {
                        子要素._MouseEnterまたはMouseLeaveが発生するかチェックする( e );
                    }
                }
            }
        }

        private bool _MouseMoveのヒットチェック( UIMouseEventArgs e )
        {
            bool キャッチした = false;

            // すべての子について深さ優先で降順（描画順の逆）にヒットチェック。
            for( int i = this.子リスト.Count - 1; i >= 0; i-- )
            {
                if( this.子リスト[ i ] is Element 子要素 && 子要素.可視 )
                {
                    if( 子要素._MouseMoveのヒットチェック( e ) )  // 深さ優先（子優先）; ヒットチェックなしでも子孫はチェックする。
                    {
                        キャッチした = true;
                        break;
                    }
                }
            }

            // すべての子がキャッチしなかった場合のみ、自分をヒットチェック。
            if( !( キャッチした ) && this.可視 && this.ヒットチェックあり )
            {
                if( this.ヒットチェック( e.マウス位置dpx ) )
                {
                    //Debug.WriteLine( $"MouseMove: {this.GetType().ToString()}, {e.マウス位置dpx}" );
                    this.MouseMove?.Invoke( this, e );
                    キャッチした = true;
                }
            }

            return キャッチした;
        }

        /// <summary>
        ///     指定された要素が子要素にあれば、それをフォーカスする。
        ///     それ以外の子要素はフォーカスを（あれば）外す。
        ///     この処理は子要素へ再帰する。
        /// </summary>
        /// <param name="対象要素">フォーカス対象の要素。子要素である必要はない。</param>
        private void _フォーカスを通知する( Element 対象要素, UIEventArgs e )
        {
            // すべての子要素について...
            foreach( Element 子要素 in this.子リスト.Where( ( 子 ) => ( 子 is Element ) ) )
            {
                bool 子要素はフォーカス対象である = ( 子要素 == 対象要素 );

                // (1) フォーカスするか外すか？

                if( !( 子要素.フォーカス中 ) && 子要素はフォーカス対象である )
                {
                    // (1-A) 新しくフォーカスされるなら GotFocus イベント発生。
                    子要素.フォーカス中 = true;
                    子要素.OnGotFocus( e );
                }
                else if( 子要素.フォーカス中 && !( 子要素はフォーカス対象である ) )
                {
                    // (1-B) フォーカスが外れるなら LostFocus イベント発生。
                    子要素.フォーカス中 = false;
                    子要素.OnLostFocus( e );
                }
                else
                {
                    // (1-C) 変更なし。
                }

                // (2) フォーカス対象だろうがそうでなかろうがすべての子へ再帰。
                子要素._フォーカスを通知する( 対象要素, e );
            }
        }
    }
}
