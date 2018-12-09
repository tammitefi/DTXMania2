using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DTXMania.設定
{
    /// <summary>
    ///     同じ消音グループ種別に属する場合、再生前消音の対象となる。
    /// </summary>
    enum 消音グループ種別
    {
        Unknown,
        LeftCymbal,
        RightCymbal,
        HiHat,
    }
}
