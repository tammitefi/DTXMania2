using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using FDK.メディア;

namespace FDK.UI
{
    public class UIMouseEventArgs : UIEventArgs
    {
        public MouseEventArgs mouseEventArgs
        {
            get;
            protected set;
        }

        /// <param name="マウス位置dpx">
        ///     マウス位置。スクリーン座標[dpx]。
        /// </param>
        public UIMouseEventArgs( DeviceContext1 dc, Vector2 マウス位置dpx, MouseEventArgs mea )
            : base( dc, マウス位置dpx )
        {
            this.mouseEventArgs = mea;
        }
    }
}
