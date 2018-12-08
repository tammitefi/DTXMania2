using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;

namespace FDK.UI
{
    public class UIEventArgs : DCEventArgs
    {
        /// <summary>
        ///     マウス位置。スクリーン座標[dpx]。
        /// </summary>
        public Vector2 マウス位置dpx
        {
            get;
            protected set;
        } = Vector2.Zero;

        /// <param name="マウス位置dpx">
        ///     マウス位置。スクリーン座標[dpx]。
        /// </param>
        public UIEventArgs( DeviceContext1 dc, Vector2 マウス位置dpx )
            : base( dc )
        {
            this.マウス位置dpx = マウス位置dpx;
        }
    }
}
