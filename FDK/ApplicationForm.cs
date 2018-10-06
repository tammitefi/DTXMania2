using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SharpDX;
using FDK.メディア;

namespace FDK
{
    public class ApplicationForm : SharpDX.Windows.RenderForm, IDisposable
    {
        /// <summary>
        ///		ウィンドウの表示モード（全画面 or ウィンドウ）を示す。
        ///		true なら全画面モード、false ならウィンドウモードである。
        ///		値を set することで、モードを変更することもできる。
        /// </summary>
        /// <remarks>
        ///		正確には、「全画面(fullscreen)」ではなく「最大化(maximize)」。
        /// </remarks>
        public bool 全画面モード
        {
            get
                => this.IsFullscreen;

            set
            {
                Trace.Assert( this._初期化完了 );

                if( value )
                {
                    if( !( this.IsFullscreen ) )
                    {
                        this._ウィンドウモードの情報のバックアップ.clientSize = this.ClientSize;
                        this._ウィンドウモードの情報のバックアップ.formBorderStyle = this.FormBorderStyle;

                        // (参考) http://www.atmarkit.co.jp/ait/articles/0408/27/news105.html
                        this.WindowState = FormWindowState.Normal;
                        this.FormBorderStyle = FormBorderStyle.None;
                        this.WindowState = FormWindowState.Maximized;

                        if( !( グラフィックデバイス.Instance.UIFramework.Root.可視 ) )
                            Cursor.Hide();

                        this.IsFullscreen = true;
                    }
                    else
                    {
                        // すでに全画面モードなので何もしない。
                    }
                }
                else
                {
                    if( this.IsFullscreen )
                    {
                        this.WindowState = FormWindowState.Normal;
                        this.ClientSize = this._ウィンドウモードの情報のバックアップ.clientSize;
                        this.FormBorderStyle = this._ウィンドウモードの情報のバックアップ.formBorderStyle;

                        Cursor.Show();
                        this.IsFullscreen = false;
                    }
                    else
                    {
                        // すでにウィンドウモードなので何もしない。
                    }
                }
            }
        }

        /// <summary>
        ///		初期化処理。
        /// </summary>
        public ApplicationForm( SizeF 設計画面サイズ, SizeF 物理画面サイズ, bool 深度ステンシルを使う = true )
        {
            this.SetStyle( ControlStyles.ResizeRedraw, true );
            this.ClientSize = 物理画面サイズ.ToSize();
            this.MinimumSize = new Size( 640, 360 );
            this.Text = "FDK.ApplicationForm";

            グラフィックデバイス.インスタンスを生成する( 
                this.Handle, 
                new Size2F( 設計画面サイズ.Width, 設計画面サイズ.Height ),
                new Size2F( 物理画面サイズ.Width, 物理画面サイズ.Height ),
                深度ステンシルを使う );

			PowerManagement.システムの自動スリープと画面の自動非表示を抑制する();

            this.UserResized += this._UserResize;
            this.Click += this._Click;
            this.MouseDown += this._MouseDown;
            this.MouseUp += this._MouseUp;
            this.MouseMove += this._MouseMove;
            this.KeyDown += this._KeyDown;

            this._初期化完了 = true;
        }

        /// <summary>
        ///		終了処理。
        /// </summary>
        public new void Dispose()
        {
            Debug.Assert( this._初期化完了 );

			PowerManagement.システムの自動スリープと画面の自動非表示の抑制を解除する();

			グラフィックデバイス.インスタンスを解放する();

            this._初期化完了 = false;

            base.Dispose();
        }

        /// <summary>
        ///		メインループ。
        ///		派生クラスでオーバーライドすること。
        /// </summary>
        public virtual void Run()
        {
            SharpDX.Windows.RenderLoop.Run( this, () => {

                var gd = グラフィックデバイス.Instance;

                if( this.FormWindowState == FormWindowState.Minimized )
                    return;

                // アニメーションを進行する。
                gd.Animation.進行する();

                // 現在のUIツリーを描画する。
                gd.UIFramework.描画する( gd.D2DDeviceContext );

                // 全面を黒で塗りつぶすだけのサンプル。
                gd.D2DDeviceContext.BeginDraw();
                gd.D2DDeviceContext.Clear( Color4.Black );
                gd.D2DDeviceContext.EndDraw();

                gd.SwapChain.Present( 1, SharpDX.DXGI.PresentFlags.None );

            } );
        }

        /// <summary>
        ///		コンストラクタでの初期化が終わっていれば true。
        /// </summary>
        protected bool _初期化完了 = false;

        /// <summary>
        ///		フォーム生成時のパラメータを編集して返す。
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                // DWM によってトップウィンドウごとに割り当てられるリダイレクトサーフェスを持たない。（リダイレクトへの画像転送がなくなる分、少し速くなるらしい）
                const int WS_EX_NOREDIRECTIONBITMAP = 0x00200000;

                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOREDIRECTIONBITMAP;
                return cp;
            }
        }

        protected FormWindowState FormWindowState = FormWindowState.Normal;

        private void _UserResize( object sender, EventArgs e )
        {
            this.FormWindowState = this.WindowState;

            if( this.FormWindowState == FormWindowState.Minimized )
            {
                // 最小化されたので何もしない。
            }
            else if( this.ClientSize.IsEmpty )
            {
                // たまに起きるらしい。スキップする。
            }
            else
            {
                // メインループ（RenderLoop）が始まる前にも数回呼び出されることがあるので、それをはじく。
                if( !( this._初期化完了 ) )
                    return;

                // スワップチェーンとその依存リソースを解放し、改めて作成しなおす。

                this.スワップチェーンに依存するグラフィックリソースを解放する();

                グラフィックデバイス.Instance.サイズを変更する( this.ClientSize );

                this.スワップチェーンに依存するグラフィックリソースを作成する();
            }
        }
        private void _MouseDown( object sender, MouseEventArgs e )
        {
            var gd = グラフィックデバイス.Instance;

            var マウス位置px = this.PointToClient( Cursor.Position );
            var マウス位置dpx = new Vector2( マウス位置px.X * gd.拡大率PXtoDPX横, マウス位置px.Y * gd.拡大率PXtoDPX縦 );

            gd.UIFramework.MouseDown( sender, new FDK.UI.UIMouseEventArgs( gd.D2DDeviceContext, マウス位置dpx, e ) );
        }
        private void _Click( object sender, EventArgs e )
        {
            var gd = グラフィックデバイス.Instance;

            var マウス位置px = this.PointToClient( Cursor.Position );
            var マウス位置dpx = new Vector2( マウス位置px.X * gd.拡大率PXtoDPX横, マウス位置px.Y * gd.拡大率PXtoDPX縦 );

            gd.UIFramework.Click( sender, new FDK.UI.UIEventArgs( gd.D2DDeviceContext, マウス位置dpx ) );
        }
        private void _MouseUp( object sender, MouseEventArgs e )
        {
            var gd = グラフィックデバイス.Instance;

            var マウス位置px = this.PointToClient( Cursor.Position );
            var マウス位置dpx = new Vector2( マウス位置px.X * gd.拡大率PXtoDPX横, マウス位置px.Y * gd.拡大率PXtoDPX縦 );

            gd.UIFramework.MouseUp( sender, new FDK.UI.UIMouseEventArgs( gd.D2DDeviceContext, マウス位置dpx, e ) );
        }
        private void _MouseMove( object sender, MouseEventArgs e )
        {
            var gd = グラフィックデバイス.Instance;

            var マウス位置px = this.PointToClient( Cursor.Position );
            var マウス位置dpx = new Vector2( マウス位置px.X * gd.拡大率PXtoDPX横, マウス位置px.Y * gd.拡大率PXtoDPX縦 );

            gd.UIFramework.MouseMove( sender, new FDK.UI.UIMouseEventArgs( gd.D2DDeviceContext, マウス位置dpx, e ) );
        }
        private void _KeyDown( object sender, KeyEventArgs e )
        {
            グラフィックデバイス.Instance.UIFramework.KeyDown( sender, e );
        }

        protected virtual void スワップチェーンに依存するグラフィックリソースを作成する()
        {
            // 派生クラスで実装すること。
        }
        protected virtual void スワップチェーンに依存するグラフィックリソースを解放する()
        {
            // 派生クラスで実装すること。
        }

        /// <summary>
        ///		ウィンドウを全画面モードにする直前に取得し、
        ///		再びウィンドウモードに戻して状態を復元する時に参照する。
        ///		（<see cref="全画面モード"/> を参照。）
        /// </summary>
        private (Size clientSize, FormBorderStyle formBorderStyle) _ウィンドウモードの情報のバックアップ;
    }
}
