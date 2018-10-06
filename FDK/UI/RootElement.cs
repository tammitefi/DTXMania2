using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace FDK.UI
{
    public class RootElement : Element
    {
        public void InvokeMouseDown( UIMouseEventArgs e )
            => this.OnMouseDown( e );
        public void InvokeClick( UIEventArgs e )
            => this.OnClick( e );
        public void InvokeMouseUp( UIMouseEventArgs e )
            => this.OnMouseUp( e );
        public void InvokeMouseMove( UIMouseEventArgs e )
            => this.OnMouseMove( e );
    }
}
