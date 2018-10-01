using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using FDK.メディア;
using MikuMikuFlex;

namespace DTXmatixx.MMF
{
    /// <summary>
    ///     <see cref="FDK.メディア.グラフィックデバイス"/> を使った <see cref="MikuMikuFlex.DeviceManager"/> の実装。
    /// </summary>
    public class DeviceManagerBridge : DeviceManager
    {
        public SharpDX.Direct3D11.Device D3DDevice
            => グラフィックデバイス.Instance.D3DDevice;

        public FeatureLevel DeviceFeatureLevel
            => グラフィックデバイス.Instance.D3DDevice.FeatureLevel;

        public DeviceContext D3DDeviceContext
            => グラフィックデバイス.Instance.D3DDevice.ImmediateContext;

        public Adapter Adapter { get; private set; }

        public Factory2 DXGIFactory { get; set; }


        public void Load()
        {
            using( var dxgiDevice = this.D3DDevice.QueryInterface<SharpDX.DXGI.Device1>() )
            {
                this.Adapter = dxgiDevice.Adapter;
                this.DXGIFactory = this.Adapter.GetParent<SharpDX.DXGI.Factory2>();
            }

            エフェクト.初期化する( this );    // 忘れずに！
        }

        public void Dispose()
        {
            this.DXGIFactory?.Dispose();
            this.DXGIFactory = null;

            this.Adapter?.Dispose();
            this.Adapter = null;
        }
    }
}
