using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace FDK.メディア.ビデオ.Sources
{
    public interface IVideoSource : IDisposable
    {
        Size2F フレームサイズ { get; }

        /// <summary>
        ///     次に読みだされるフレームがあれば、その表示予定時刻[100ns単位]を返す。
        ///     フレームがなければ、ブロックせずにすぐ 負数 を返す。
        /// </summary>
        long Peek();

        /// <summary>
        ///     フレームを１つ読みだす。
        ///     再生中の場合、フレームが取得できるまでブロックする。
        ///     再生が終了している場合は null を返す。
        ///     取得したフレームは、使用が終わったら、呼び出し元で Dispose すること。
        /// </summary>
        VideoFrame Read();
    }
}
