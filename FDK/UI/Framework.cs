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

namespace FDK.UI
{
    public class Framework : Activity, IDisposable
    {
        public RootElement Root
        {
            get;
            protected set;
        } = null;

        public Framework()
        {
            this.子を追加する( this.Root = new RootElement() {
                位置dpx = new Vector2( 0f, 0f ),
                サイズdpx = グラフィックデバイス.Instance.設計画面サイズ,
                背景色 = Color.Transparent,
            } );
        }
        public void Dispose()
        {
            Debug.Assert( this.活性化していない );
            this.Root = null;
        }
        public void 初期化する()
        {
            if( this.活性化している )
                this.Root.非活性化する();

            this.Root.子リストをクリアする();

            if( this.活性化している )
                this.Root.活性化する();
        }
        public void 描画する( DeviceContext1 dc )
        {
            if( this.Root.可視 )
                this.Root.描画する( new DCEventArgs( dc ) );
        }

        public void MouseDown( object sender, UIMouseEventArgs e )
        {
            if( this.Root.可視 )
                this.Root.InvokeMouseDown( e );
        }
        public void Click( object sender, UIEventArgs e )
        {
            if( this.Root.可視 )
                this.Root.InvokeClick( e );
        }
        public void MouseUp( object sender, UIMouseEventArgs e )
        {
            if( this.Root.可視 )
                this.Root.InvokeMouseUp( e );
        }
        public void MouseMove( object sender, UIMouseEventArgs e )
        {
            if( this.Root.可視 )
                this.Root.InvokeMouseMove( e );
        }
        public void KeyDown( object sender, KeyEventArgs e )
        {
            if( this.Root.可視 )
                this.Root.InvokeKeyDown( e );
        }
    }
}