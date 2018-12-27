using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;

namespace FDK.UI
{
    public class Image : Element
    {
        /// <summary>
        ///     拡大率を反映した、画像のサイズ[dpx]。
        /// </summary>
        public override Size2F サイズdpx
            => new Size2F( this.原寸サイズdpx.Width * this.拡大率.X, this.原寸サイズdpx.Height * this.拡大率.Y );

        /// <summary>
        ///     画像の原寸サイズ[dpx]。
        /// </summary>
        public Size2F 原寸サイズdpx
            => this._画像.サイズ;

        /// <summary>
        ///		(1f,1f) で原寸。
        /// </summary>
        public Vector2 拡大率
        {
            get;
            set;
        } = new Vector2( 1f, 1f );

        public Image( VariablePath imagePath, Vector2? 位置dpx = null, Vector2? 拡大率 = null )
        {
            this.位置dpx = 位置dpx ?? Vector2.Zero;
            this.拡大率 = 拡大率 ?? Vector2.One;

            this.子Activityを追加する( this._画像 = new 描画可能画像( imagePath ) );
        }
        public Image( Size2F サイズdpx, Vector2? 位置dpx = null, Vector2? 拡大率 = null )
        {
            this.位置dpx = 位置dpx ?? Vector2.Zero;
            this.拡大率 = 拡大率 ?? Vector2.One;

            this.子Activityを追加する( this._画像 = new 描画可能画像( this.サイズdpx ) );
        }

        public void 画像へ描画する( Action<DeviceContext1> action )
        {
            Debug.Assert( this.活性化している );

            this._画像.画像へ描画する( action ); // null 不可
        }


        protected 描画可能画像 _画像 = null;

        protected override void OnPaint( DCEventArgs e )
        {
            base.OnPaint( e );

            this._画像.描画する(  // null 不可
                e.dc,
                this.ウィンドウ矩形dpx.X,
                this.ウィンドウ矩形dpx.Y,
                転送元矩形: null,
                X方向拡大率: this.拡大率.X,
                Y方向拡大率: this.拡大率.Y );
        }
    }
}