using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace FDK.UI
{
    public class ListViewItem : Label
    {
        public ListViewItem( string テキスト )
            : base( テキスト, Size2F.Zero, null )
        {
            this.ヒットチェックあり = false;
        }
    }
}
