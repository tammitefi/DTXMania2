using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using MikuMikuFlex;
using FDK.メディア;

namespace DTXmatixx.MMF
{
    /// <summary>
    ///     <see cref="FDK.メディア.グラフィックデバイス"/> を使った <see cref="MikuMikuFlex.TargetContext"/> の実装。
    /// </summary>
    class TargetContext : MikuMikuFlex.TargetContext
    {
        public RenderTargetView D3Dレンダーターゲットビュー
            => グラフィックデバイス.Instance.D3DRenderTargetView;

        public DepthStencilView 深度ステンシルビュー
            => グラフィックデバイス.Instance.D3DDepthStencilView;

        public SwapChain SwapChain
            => グラフィックデバイス.Instance.SwapChain;

        public 行列管理 行列管理 { get; set; }

        public カメラモーション カメラモーション { get; set; }

        public ワールド空間 ワールド空間 { get; set; }

        public bool IsSelfShadowMode1 { get; protected set; }

        public bool IsEnabledTransparent { get; protected set; }

        public Control BindedControl { get; private set; }


        public TargetContext( Control owner )
            : base()
        {
            this.BindedControl = owner;

            var カメラ = new カメラ(
                カメラの初期位置: new Vector3( 0, 20, -40 ),
                カメラの初期注視点: new Vector3( 0, 3, 0 ),
                カメラの初期上方向ベクトル: new Vector3( 0, 1, 0 ) );
            var 射影行列 = new 射影();
            射影行列.射影行列を初期化する( (float) Math.PI / 4f, 1.618f, 1, 200 );
            this.行列管理 = new 行列管理( new ワールド行列(), カメラ, 射影行列 );

            this.ワールド空間 = new ワールド空間();

            this.ビューポートを設定する();
        }

        public void Dispose()
        {
            this.ワールド空間?.Dispose();
            this.ワールド空間 = null;
        }

        public void ビューポートを設定する()
        {
            RenderContext.Instance.DeviceManager.D3DDeviceContext.Rasterizer.SetViewports(
                new SharpDX.Mathematics.Interop.RawViewportF[] {
                    new Viewport {
                        Width = BindedControl.Width,
                        Height = BindedControl.Height,
                        MaxDepth = 1,
                    }
                } );
        }
    }
}
