using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DTXmatixx.設定
{
    enum 入力グループプリセット種別
    {
        /// <summary>
        ///     シンバル類のチップは、どのシンバル類を叩いても OK とするプリセット。
        /// </summary>
        シンバルフリー,

        /// <summary>
        ///     類似したチップをグループ化したプリセット。
        /// </summary>
        基本形,
    }
}
