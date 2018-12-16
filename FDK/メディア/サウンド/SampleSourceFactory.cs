using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSCore;

namespace FDK
{
    public static class SampleSourceFactory
    {
        /// <summary>
        ///		指定されたファイルから <see cref="ISampleSource"/> を生成して返す。
        ///		失敗すれば null 。
        /// </summary>
        public static ISampleSource Create( SoundDevice device, VariablePath ファイルパス )
        {
            var ext = Path.GetExtension( ファイルパス.変数なしパス ).ToLower();

            #region " NVorbis を試みる "
            //----------------
            if( ".ogg" == ext )
            {
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
            }
            //----------------
            #endregion

            #region " XA を試みる "
            //----------------
            if( ".xa" == ext )
            {
                try
                {
                    return new XaResampledOnMemoryWaveSource( ファイルパス, device.WaveFormat )
                        .ToSampleSource();
                }
                catch
                {
                    // ダメだったので次へ。
                }
            }
            //----------------
            #endregion

            #region " MediaFoundation を試みる "
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

            throw new Exception( $"未対応のオーディオファイルです。" );
        }
    }
}

