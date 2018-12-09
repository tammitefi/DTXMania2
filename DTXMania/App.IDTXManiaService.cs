using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DTXMania
{
    /// <summary>
    ///		IDTXManiaService の実装
    /// </summary>
    /// <remarks>
    ///		このアセンブリ（exe）は、WCF で IDTXManiaService を公開する。
    ///		このサービスインターフェースは、シングルスレッド（GUIスレッド）で同期実行される。（Appクラスの ServiceBehavior属性を参照。）
    ///		このサービスホストはシングルトンであり、すべてのクライアントセッションは同一（単一）のサービスインスタンスへ接続される。（Program.Main() を参照。）
    /// </remarks>
    partial class App : DTXMania.Viewer.IDTXManiaService
    {
        /// <summary>
        ///		曲を読み込み、演奏を開始する。
        ///		ビュアーモードのときのみ有効。
        /// </summary>
        /// <param name="path">曲ファイルパス</param>
        /// <param name="startPart">演奏開始小節番号(0～)</param>
        /// <param name="drumsSound">ドラムチップ音を発声させるなら true。</param>
        public void ViewerPlay( string path, int startPart = 0, bool drumsSound = true )
        {
            // TODO: ViewerPlay メソッドを実装する。
            throw new NotImplementedException();
        }

        /// <summary>
        ///		現在の演奏を停止する。
        ///		ビュアーモードのときのみ有効。
        /// </summary>
        public void ViewerStop()
        {
            // TODO: ViewerStop メソッドを実装する。
            throw new NotImplementedException();
        }

        /// <summary>
        ///		サウンドデバイスの発声遅延[ms]を返す。
        /// </summary>
        /// <returns>遅延量[ms]</returns>
        public float GetSoundDelay()
            => (float) ( App.サウンドデバイス?.再生遅延sec ?? 0.0 ) * 1000.0f;
    }
}
