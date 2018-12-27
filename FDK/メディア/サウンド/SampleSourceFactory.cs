using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSCore;

namespace FDK
{
    /// <summary>
    ///     <see cref="ISampleSource"/> の生成を行う。
    /// </summary>
    public static class SampleSourceFactory
    {
        /// <summary>
        ///		指定されたファイルの音声をデコードし、<see cref="ISampleSource"/> を返す。
        ///		失敗すれば null 。
        /// </summary>
        public static ISampleSource Create( SoundDevice device, VariablePath ファイルパス )
        {
            var 拡張子 = Path.GetExtension( ファイルパス.変数なしパス ).ToLower();

            if( ".ogg" == 拡張子 )
            {
                #region " OggVorvis "
                //----------------
                try
                {
                    using( var audioStream = new FileStream( ファイルパス.変数なしパス, FileMode.Open, FileAccess.Read ) )
                    {
                        return new NVorbisResampledOnMemoryWaveSource( audioStream, device.WaveFormat )
                            .ToSampleSource();
                    }
                }
                catch
                {
                    // ダメだったので次へ。
                }
                //----------------
                #endregion
            }
            if( ".wav" == 拡張子 )
            {
                #region " WAV "
                //----------------
                try
                {
                    return new WavResampledOnMemoryWaveSource( ファイルパス, device.WaveFormat )
                        .ToSampleSource();
                }
                catch
                {
                    // ダメだったので次へ。
                }
                //----------------
                #endregion
            }
            if( ".xa" == 拡張子 )
            {
                #region " XA "
                //----------------
                try
                {
                    return new XaResampledOnMemoryWaveSource( ファイルパス, device.WaveFormat )
                        .ToSampleSource();
                }
                catch
                {
                    // ダメだったので次へ。
                }
                //----------------
                #endregion
            }
            else
            {
                #region " MediaFoundation "
                //----------------
                try
                {
                    var fi = new FileInfo( ファイルパス.変数なしパス );

                    /*
                    if( 1 * 1024 * 1024 < fi.Length )   // 1MB 以上ならストリーミング再生
                    {
                        return new MediaFoundationOnStreamingWaveSource( ファイルパス, device.WaveFormat )
                            .ToSampleSource();
                    }
                    else
                    {
                        return new MediaFoundationOnMemoryWaveSource( ファイルパス, device.WaveFormat )
                            .ToSampleSource();
                    }
                    */

                    // すべてオンメモリ
                    return new MediaFoundationOnMemoryWaveSource( ファイルパス, device.WaveFormat )
                        .ToSampleSource();

                }
                catch
                {
                    // ダメだったので次へ。
                }
                //----------------
                #endregion
            }

            Log.ERROR( $"未対応のオーディオファイルです。{ファイルパス.変数付きパス}" );
            return null;
        }
    }
}

