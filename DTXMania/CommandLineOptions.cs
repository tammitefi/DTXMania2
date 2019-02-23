using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DTXMania
{
    class CommandLineOptions
    {
        public readonly string Usage =
            "DTXMania " + App.リリース番号.ToString( "000" ) + "\n" +
            "Usage: dtxmania.exe [options] [filename]\n" +
            "-Vvvv,ppp,\"soundfilename\"    Play sound file vvv=volume, ppp=pan \n" +
            "-S                           DTXV Regeneration stops \n" +
            "-D(Sound mode)(Y or N last 3 characters) Viewer settings \n" +
            "                             (Sound mode) WE=WASAPI Exclusive, WS=WASAPI Shared, A1=ASIO(Numbers are device numbers), D=DSound \n" +
            "                             YYY, YNY etc  1. Letter=GRmode, 2. Letter=TmeStretch, 3. Letter=VSyncWait \n" +
            "-N[xxx]                      Start of regeneration xxx=Playback start measure number \n" +
            "-Etype,freq,bitrate,volBGM,volSE,volDrums,volGuitar,volBassmvolMaster,\"outfilename\",\"dtxfilename\" \n"+
            "                             Used as DTX2WAV type=\"WAV\"or\"MP3\"or\"OGG\", freq=48000 etc., bitrate=192 (kHz etc)" +
            "-B                           BGM only（Do not play drum sound）"+
            "filename                     File to play。";

        // -V 系
        public bool サウンドファイルの再生 { get; set; } = false;
        public int Volume { get; set; }
        public int Pan { get; set; }
        public string サウンドファイル名 { get; set; }

        // -S 系
        public bool 再生停止 { get; set; } = false;

        // -D 系
        public bool ビュアーの設定 { set; get; } = false;
        public string サウンドモード { get; set; }
        public bool GRmode { get; set; }
        public bool TimeStretch { get; set; }
        public bool VSyncWait { get; set; }

        // -N 系
        public bool 再生開始 { get; set; } = false;
        public int 再生開始小節番号 { get; set; }

        // -B 系
        public bool ドラム音を発声する { get; set; } = true;

        // -E 系
        public bool DTX2WAV { get; set; } = false;
        public string Type { get; set; }
        public int Freq { get; set; }
        public int BitRate { get; set; }
        public int volBGM { get; set; }
        public int volSE { get; set; }
        public int volDrums { get; set; }
        public int volGuitar { get; set; }
        public int volBass { get; set; }
        public int volMaster { get; set; }
        public string OutFilename { get; set; }
        public string DTXFilename { get; set; }

        // ファイル名
        public string Filename { get; set; } = null;


        /// <summary>
        ///     指定されたコマンドライン引数を解析する。
        /// </summary>
        /// <param name="args">main()に渡されるものと同じ。</param>
        /// <returns>解析に成功すれば true、失敗すれば false。</returns>
        public bool 解析する( string[] args )
        {
            for( int i = 0; i < args.Length; i++ )
            {
                if( 1 <= args[i].Length && args[ i ][ 0 ] == '-' )
                {
                    if( args[ i ].StartsWith( "-V", StringComparison.OrdinalIgnoreCase ) )
                    {
                        // todo: サウンドファイルの再生
                    }
                    else if( args[ i ].StartsWith( "-S", StringComparison.OrdinalIgnoreCase ) )
                    {
                        #region " 再生停止 "
                        //----------------
                        this.再生停止 = true;
                        //----------------
                        #endregion
                    }
                    else if( args[ i ].StartsWith( "-D", StringComparison.OrdinalIgnoreCase ) )
                    {
                        // todo: ビュアーの設定
                    }
                    else if( args[ i ].StartsWith( "-N", StringComparison.OrdinalIgnoreCase ) )
                    {
                        #region " 再生開始 "
                        //----------------
                        int part = -1;  // 省略値は -1

                        if( 2 < args[ i ].Length )
                        {
                            if( !int.TryParse( args[ i ].Substring( 2 ), out part ) )   // 負数も OK
                                return false;
                        }

                        this.再生開始 = true;
                        this.再生開始小節番号 = part;
                        //----------------
                        #endregion
                    }
                    else if( args[ i ].StartsWith( "-E", StringComparison.OrdinalIgnoreCase ) )
                    {
                        // todo: DTX2WAV 実行
                    }
                    else if( args[ i ].StartsWith( "-C", StringComparison.OrdinalIgnoreCase ) )
                    {
                        // todo: DTX2WAV キャンセル？
                    }
                    else if( args[ i ].StartsWith( "-B", StringComparison.OrdinalIgnoreCase ) )
                    {
                        #region " BGMのみ（ドラムサウンドを発声しない）"
                        //----------------
                        this.ドラム音を発声する = false;
                        //----------------
                        #endregion
                    }
                    else
                    {
                        // 未知のオプション
                        return false;
                    }
                }
                else
                {
                    if( 1 < args[ i ].Length )
                    {
                        // オプション以外はすべてファイル名とみなす
                        this.Filename = args[ i ];
                    }
                }
            }


            // ファイル名の指定があるのに -N も -S もない場合には、-N-1 が指定されたものとみなす。
            if( null != this.Filename && ( !this.再生開始 && !this.再生停止 ) )
            {
                this.再生開始 = true;
                this.再生開始小節番号 = -1;
            }

            return true;
        }
    }
}
