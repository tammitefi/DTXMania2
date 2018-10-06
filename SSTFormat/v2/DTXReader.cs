using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SSTFormat.v2
{
    public static class DTXReader
    {
        /// <summary>
        ///		スコア生成中のデバッグメッセージを出力するなら true。
        /// </summary>
        public static bool Verbose { get; set; } = true;

        static DTXReader()
        {
        }

        /// <summary>
        ///		ファイルからDTXデータを読み込み、スコアを生成する。
        /// </summary>
        /// <returns>生成されたスコア。</returns>
        public static スコア ReadFromFile( string DTXファイルパス )
        {
            string 全入力文字列 = null;

            using( var sr = new StreamReader( DTXファイルパス, Encoding.GetEncoding( 932/*Shift-JIS*/ ) ) )
            {
                全入力文字列 = sr.ReadToEnd();
            }

            return ReadFromString( 全入力文字列 );
        }

        /// <summary>
        ///		行からDTXデータを読み込み、スコアを生成する。
        /// </summary>
        /// <returns>生成されたスコア。</returns>
        public static スコア ReadFromString( string 全入力文字列 )
        {
            var スコア = new スコア();

            // 解析
            _行解析( ref スコア, ref 全入力文字列 );

            // 後処理
            スコア.曲データファイルを読み込む_後処理だけ();

            return スコア;
        }

        // ※ テストプロジェクトに対しては InternalsVisibleTo 属性により internal メソッドを可視としているため、
        // 　 以降のテスト対象のメソッドは、本来 private でも internal として宣言している。

        /// <summary>
        ///		行を、コマンドとパラメータとコメントの３ブロックに分割して返す。
        ///		存在しないブロックは null または 空文字列 に設定される。
        /// </summary>
        /// <returns>行分解に成功すればtrue、失敗すればfalse。</returns>
        internal static bool _行分解( string 行, out string コマンド, out string パラメータ, out string コメント )
        {
            コマンド = null;
            パラメータ = null;
            コメント = null;

            行 = 行.Trim( ' ', '　', '\t' );
            if( 0 == 行.Length )
                return true; // 空行。

            // この書式の具体的な仕様については、単体テストコードを参照。
            string DTX行書式 = @"^\s*(?:#\s*([^:;\s]*)[:\s]*([^;]*)?)?[;\s]*(.*)$";

            var m = Regex.Match( 行, DTX行書式 );

            if( m.Success && ( 4 == m.Groups.Count ) )
            {
                コマンド = m.Groups[ 1 ].Value?.Trim();
                パラメータ = m.Groups[ 2 ].Value?.Trim();
                コメント = m.Groups[ 3 ].Value?.Trim();
                return true;
            }
            else
            {
                return false;
            }
        }

        internal class C行解析時の状態変数
        {
            public int 行番号 = 0;
            public int 小節番号 = 0;
            public int チャンネル番号 = 0;
            public int 小節解像度 = 384;
            public チップ種別 チップ種別 = チップ種別.Unknown;
            public int オブジェクト総数 = 0;
            public int オブジェクト番号 = 0;

            // 小節長倍率の処理方法：
            // ・DTXでは指定した小節以降の小節長がすべて変わってしまうが、SSTでは指定した小節の小節長のみ変わる。
            // ・そのため、解析時は 小節長倍率マップ に指定があった小節の情報のみ残しておき、
            // 　後処理でまとめて スコア.小節長倍率リスト に登録するものとする。
            // ・なお、スコア.小節長倍率リスト には存在するすべての小節が登録されていなければならない。（SSTの仕様）

            /// <summary>
            ///		&lt;小節番号, 倍率&gt;
            /// </summary>
            public SortedDictionary<int, float> 小節長倍率マップ = null;

            // #BPM, #BPMzz の処理方法：
            // ・#BPMzz は、BPM定義マップ[1～3599] に登録する。
            // ・#BPM は、#BPM00 とみなして BPM定義マップ[0] に登録する。
            // ・ch.03 (整数BPM) はそのまま SSTチップ(BPM) としてチップリストに登録する。
            // ・ch.08（拡張BPM）は、BPM値が 0 の SSTチップ(BPM) をチップリストに登録する。同時に、BPM参照マップ[チップ] にも登録する。
            // ・DTXファイル読み込み後の後処理として、BPM参照マップ に掲載されているすべての SSTチップ(BPM) の BPM値を、BPM定義マップ から引き当てて設定する。

            public float BASEBPM = 0.0f;

            /// <summary>
            ///		&lt;zz番号, BPM値&gt;
            ///		bpm値は float 型（DTX仕様）
            /// </summary>
            public Dictionary<int, float> BPM定義マップ = null;

            /// <summary>
            /// 	&lt;SSTチップ(BPM), BPM定義のzz番号&gt;
            /// </summary>
            public Dictionary<チップ, int> BPM参照マップ = null;
        }

        internal static void _行解析( ref スコア スコア, ref string 全入力文字列 )
        {
            // 現在の状態の初期化。
            var 現在の = new C行解析時の状態変数() {
                小節番号 = 0,
                小節解像度 = 384,    // DTX の小節解像度。
                チップ種別 = チップ種別.Unknown,
                小節長倍率マップ = new SortedDictionary<int, float>(),
                BPM定義マップ = new Dictionary<int, float>(),
                BPM参照マップ = new Dictionary<チップ, int>(),
            };

            Debug.WriteLineIf( Verbose, "行解析を開始します。" );

            #region " 前処理(1) TAB は SPACE に置換しておく。"
            //----------------
            全入力文字列 = 全入力文字列.Replace( '\t', ' ' );
            //----------------
            #endregion

            #region " すべての行について解析。"
            //----------------
            using( var sr = new StringReader( 全入力文字列 ) )
            {
                string 行;

                // １行ずつ処理。
                for( 現在の.行番号 = 1; ( 行 = sr.ReadLine() ) != null; 現在の.行番号++ )
                {
                    // 行分解。
                    if( !( _行分解( 行, out string コマンド, out string パラメータ, out string コメント ) ) )
                    {
                        Debug.WriteLineIf( Verbose, $"{現在の.行番号}: 行分解に失敗しました。" );
                        continue;
                    }
                    if( string.IsNullOrEmpty( コマンド ) )
                        continue;

                    // 行処理。
                    var done =
                        _行解析_TITLE( ref スコア, ref 現在の, コマンド, パラメータ, コメント ) ||
                        _行解析_COMMENT( ref スコア, ref 現在の, コマンド, パラメータ, コメント ) ||
                        _行解析_BASEBPM( ref スコア, ref 現在の, コマンド, パラメータ, コメント ) ||
                        _行解析_BPM( ref スコア, ref 現在の, コマンド, パラメータ, コメント ) ||
                        _行解析_BPMzz( ref スコア, ref 現在の, コマンド, パラメータ, コメント ) ||
                        _行解析_オブジェクト記述( ref スコア, ref 現在の, コマンド, パラメータ, コメント );

                    // 行処理に失敗
                    //if( !( done ) )
                    //	Debug.WriteLineIf( Verbose, $"{現在の.行番号}: 未知のコマンドが使用されました。スキップします。[{コマンド}]" );
                }
            }
            //----------------
            #endregion

            #region " 後処理(1) BPMチップの値を引き当てる。"
            //----------------
            {
                foreach( var kvp in 現在の.BPM参照マップ )
                {
                    kvp.Key.BPM =
                        現在の.BPM定義マップ[ kvp.Value ] +
                        現在の.BASEBPM;        // 複数あるなら最後の値が入っている。
                }

                現在の.BPM参照マップ.Clear();   // 引き当てが終わったら、マップが持つチップへの参照を解放する。
            }
            //----------------
            #endregion
            #region " 後処理(2) 小節長倍率マップをもとに、スコアの小節長倍率リストを構築する。"
            //----------------
            {
                double 現在の倍率 = 1.0f;

                for( int i = 0; i <= スコア.最大小節番号; i++ )      // すべての小節に対して設定。（SST仕様）
                {
                    if( 現在の.小節長倍率マップ.ContainsKey( i ) )
                        現在の倍率 = (double) 現在の.小節長倍率マップ[ i ]; // 指定された倍率は、それが指定された小節以降の小節にも適用する。（DTX仕様）

                    スコア.小節長倍率を設定する( i, 現在の倍率 );
                }
            }
            //----------------
            #endregion

            // 解析終了。
            Debug.WriteLineIf( Verbose, "行解析を終了しました。" );
        }

        internal static bool _行解析_TITLE( ref スコア スコア, ref C行解析時の状態変数 現在の, string コマンド, string パラメータ, string コメント )
        {
            if( "title" != コマンド.ToLower() )
                return false;

            スコア.Header.曲名 = パラメータ;

            return true;
        }
        internal static bool _行解析_COMMENT( ref スコア スコア, ref C行解析時の状態変数 現在の, string コマンド, string パラメータ, string コメント )
        {
            if( "comment" != コマンド.ToLower() )
                return false;

            スコア.Header.説明文 = パラメータ;

            return true;
        }
        internal static bool _行解析_BASEBPM( ref スコア スコア, ref C行解析時の状態変数 現在の, string コマンド, string パラメータ, string コメント )
        {
            if( "basebpm" != コマンド.ToLower() )
                return false;

            if( float.TryParse( パラメータ, out float bpm値 ) )
            {
                現在の.BASEBPM = bpm値; // 上書き可
                return true;
            }
            else
            {
                return false;
            }
        }
        internal static bool _行解析_BPM( ref スコア スコア, ref C行解析時の状態変数 現在の, string コマンド, string パラメータ, string コメント )
        {
            if( "bpm" != コマンド.ToLower() )
                return false;

            if( float.TryParse( パラメータ, out float bpm値 ) )
            {
                // ※ 無限管理には非対応。

                //bpm値 += 現在の.BASEBPM;	--> #BPM: の値には #BASEBPM を加算しない。（DTX仕様）

                // "#BPM:" に対応するBPMチップは、常に、小節番号==0かつ小節内位置==0に置かれる。
                var bpmChip = スコア.チップリスト.FirstOrDefault( ( c ) => ( c.チップ種別 == チップ種別.BPM && c.小節番号 == 0 && c.小節内位置 == 0 ) );
                if( null != bpmChip )
                {
                    // (A) すでに存在するなら上書き
                    bpmChip.BPM = bpm値;
                }
                else
                {
                    // (B) まだ存在していないなら新規追加
                    bpmChip = new チップ() {
                        チップ種別 = チップ種別.BPM,
                        小節番号 = 0,
                        小節解像度 = 現在の.小節解像度,
                        小節内位置 = 0,
                        音量 = チップ.最大音量,
                        BPM = bpm値,
                    };

                    スコア.チップリスト.Add( bpmChip );
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        internal static bool _行解析_BPMzz( ref スコア スコア, ref C行解析時の状態変数 現在の, string コマンド, string パラメータ, string コメント )
        {
            if( !( コマンド.ToLower().StartsWith( "bpm", ignoreCase: true, culture: null ) ) || ( 5 != コマンド.Length ) )
                return false;

            int zz =
                _三十六進数変換表.IndexOf( コマンド[ 3 ] ) * 36 +
                _三十六進数変換表.IndexOf( コマンド[ 4 ] );          // 36進数2桁表記

            if( ( 1 > zz ) || ( 36 * 36 - 1 < zz ) )
                return false;   // 有効範囲は 1～3599

            if( float.TryParse( パラメータ, out float bpm値 ) && ( 0f < bpm値 ) && ( 1000f > bpm値 ) )  // 値域制限はDTX仕様
            {
                // ※ 無限管理には非対応。
                現在の.BPM定義マップ.Remove( zz );   // 上書き可
                現在の.BPM定義マップ[ zz ] = bpm値;
                return true;
            }
            else
            {
                return false;
            }
        }
        internal static bool _行解析_オブジェクト記述( ref スコア スコア, ref C行解析時の状態変数 現在の, string コマンド, string パラメータ, string コメント )
        {
            if( !( _小節番号とチャンネル番号を取得する( コマンド, out 現在の.小節番号, out 現在の.チャンネル番号 ) ) )
                return false;

            パラメータ = パラメータ.Replace( "_", "" );  // 見やすさのために '_' を区切り文字として使用できる（DTX仕様）
            パラメータ = パラメータ.ToLower();            // すべて小文字化（三十六進数変換表には大文字がないので）

            if( 0x02 == 現在の.チャンネル番号 )
            {
                #region " ch02 小節長倍率 "
                //----------------
                if( !( _DTX仕様の実数を取得する( パラメータ, out float 小節長倍率 ) ) )
                {
                    Debug.WriteLineIf( Verbose, $"{現在の.行番号}: ch02 のパラメータ（小節長倍率）に指定された実数の解析に失敗しました。" );
                    return false;
                }
                else if( 0.0 >= 小節長倍率 )
                {
                    Debug.WriteLineIf( Verbose, $"{現在の.行番号}: ch02 のパラメータ（小数長倍率）に 0 または負数を指定することはできません。" );
                    return false;
                }

                現在の.小節長倍率マップ.Remove( 現在の.小節番号 );            // 上書き可
                現在の.小節長倍率マップ[ 現在の.小節番号 ] = 小節長倍率;
                return true;
                //----------------
                #endregion
            }

            現在の.オブジェクト総数 = パラメータ.Length / 2;    // 1オブジェクトは2文字；余った末尾は切り捨てる。（DTX仕様）

            for( 現在の.オブジェクト番号 = 0; 現在の.オブジェクト番号 < 現在の.オブジェクト総数; 現在の.オブジェクト番号++ )
            {
                int オブジェクト値 = 0;

                #region " オブジェクト値を取得。"
                //----------------
                if( 0x03 == 現在の.チャンネル番号 )
                {
                    // (A) ch03 (BPM) のみ 16進数2桁表記
                    オブジェクト値 = Convert.ToInt32( パラメータ.Substring( 現在の.オブジェクト番号 * 2, 2 ), 16 );
                }
                else
                {
                    // (B) その他は 36進数2桁表記
                    オブジェクト値 =
                        _三十六進数変換表.IndexOf( パラメータ[ 現在の.オブジェクト番号 * 2 ] ) * 36 +
                        _三十六進数変換表.IndexOf( パラメータ[ 現在の.オブジェクト番号 * 2 + 1 ] );
                }

                if( 0 > オブジェクト値 )
                {
                    Debug.WriteLineIf( Verbose, $"{現在の.行番号}: オブジェクトの記述に不正があります。" );
                    return false;
                }
                //----------------
                #endregion

                if( 0x00 == オブジェクト値 )
                    continue;   // 00 はスキップ。

                // チップを作成し、共通情報を初期化。
                var chip = new チップ() {
                    チップ種別 = チップ種別.Unknown,
                    小節番号 = 現在の.小節番号,
                    小節解像度 = 現在の.小節解像度,
                    小節内位置 = (int) ( 現在の.小節解像度 * 現在の.オブジェクト番号 / 現在の.オブジェクト総数 ),
                    音量 = チップ.最大音量,
                };

                // チャンネル別に分岐。
                switch( 現在の.チャンネル番号 )
                {
                    // バックコーラス(BGM)
                    case 0x01:
                        chip.チップ種別 = チップ種別.背景動画;
                        break;

                    // 小節長倍率 → 先に処理済み。
                    case 0x02:
                        break;

                    // BPM
                    case 0x03:
                        chip.チップ種別 = チップ種別.BPM;
                        chip.BPM = (double) ( オブジェクト値 + 現在の.BASEBPM );  // 引き当てないので、ここでBASEBPMを加算する。
                        break;

                    // 拡張BPM
                    case 0x08:
                        chip.チップ種別 = チップ種別.BPM;
                        chip.BPM = 0.0; // あとで引き当てる。BASEBPMは引き当て時に加算する。
                        現在の.BPM参照マップ.Add( chip, オブジェクト値 );  // 引き当てを予約。
                        break;

                    // チップ配置（ドラム）
                    case 0x11: chip.チップ種別 = チップ種別.HiHat_Close; break;
                    case 0x12: chip.チップ種別 = チップ種別.Snare; break;
                    case 0x13: chip.チップ種別 = チップ種別.Bass; break;
                    case 0x14: chip.チップ種別 = チップ種別.Tom1; break;
                    case 0x15: chip.チップ種別 = チップ種別.Tom2; break;
                    case 0x16: chip.チップ種別 = チップ種別.RightCrash; break;
                    case 0x17: chip.チップ種別 = チップ種別.Tom3; break;
                    case 0x18: chip.チップ種別 = チップ種別.HiHat_Open; break;
                    case 0x19: chip.チップ種別 = チップ種別.Ride; break;
                    case 0x1A: chip.チップ種別 = チップ種別.LeftCrash; break;
                    case 0x1B: chip.チップ種別 = チップ種別.HiHat_Foot; break;   // Ver.K拡張
                    case 0x1C: chip.チップ種別 = チップ種別.Bass; break;          // Ver.K拡張; 左右バスは統合

                    // 小節線、拍線
                    case 0x50: chip.チップ種別 = チップ種別.小節線; break;
                    case 0x51: chip.チップ種別 = チップ種別.拍線; break;
                }

                // チップリストに登録。
                if( チップ種別.Unknown != chip.チップ種別 )
                {
                    スコア.チップリスト.Add( chip );
                }
            }
            return true;
        }

        internal static bool _小節番号とチャンネル番号を取得する( string 行, out int 小節番号, out int チャンネル番号 )
        {
            小節番号 = 0;
            チャンネル番号 = 0;

            var m = Regex.Match( 行, @"^([0-9|a-z|A-Z])([0-9]{2})([0-9|a-f|A-F]{2})$" );

            if( m.Success && ( 4 == m.Groups.Count ) )
            {
                int 小節番号の百の位 = _三十六進数変換表.IndexOf( m.Groups[ 1 ].Value.ToLower() );  // 0～Z（36進数1桁）
                int 小節番号の十と一の位 = Convert.ToInt32( m.Groups[ 2 ].Value );          // 00～99（10進数2桁）
                小節番号 = 小節番号の百の位 * 100 + 小節番号の十と一の位;

                チャンネル番号 = Convert.ToInt32( m.Groups[ 3 ].Value, 16 );       // 00～FF（16進数2桁）
            }
            else
            {
                return false;
            }

            return true;
        }
        internal static bool _DTX仕様の実数を取得する( string 文字列, out float 数値 )
        {
            数値 = 0f;

            // DTX仕様の実数の定義（カルチャ非依存）
            const string 小数点文字s = ".,";                 // '.' の他に ',' も使える。
            const string 任意の桁区切り文字 = @"[\.,' ]";    // 小数点文字と被ってる文字もあるので注意。

            int 小数点の位置 = 文字列.LastIndexOfAny( 小数点文字s.ToCharArray() );    // 小数点文字s のうち、一番最後に現れた箇所。（DTX仕様）

            string 整数部;
            string 小数部;

            // 整数部と小数部に分けて取得し、それぞれから桁区切り文字を除去する。
            if( -1 == 小数点の位置 )
            {
                // (A) 小数点がない場合
                整数部 = Regex.Replace( 文字列, 任意の桁区切り文字, "" );
                小数部 = "";
            }
            else
            {
                // (B) 小数点がある場合
                整数部 = Regex.Replace( 文字列.Substring( 0, 小数点の位置 ), 任意の桁区切り文字, "" );
                小数部 = Regex.Replace( 文字列.Substring( 小数点の位置 + 1 ), 任意の桁区切り文字, "" );
            }

            // 整数部＋小数点＋小数部 で CurrentCulture な実数文字列を作成し、float へ変換する。
            return float.TryParse( $"{整数部}{CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator}{小数部}", out 数値 );
        }

        private static readonly string _三十六進数変換表 = "0123456789abcdefghijklmnopqrstuvwxyz";  // めんどいので大文字は考慮しない
    }
}
