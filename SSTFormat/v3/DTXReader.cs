using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SSTFormat.v3
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

            // ShiftJIS で一気読み。
            using( var sr = new StreamReader( DTXファイルパス, Encoding.GetEncoding( 932/*Shift-JIS*/ ) ) )
            {
                全入力文字列 = sr.ReadToEnd();
            }

            // 解析。
            var score = ReadFromString( 全入力文字列 );
            score.譜面ファイルパス = DTXファイルパス; // ファイルから読み込んだ場合のみ、このメンバが有効。

            return score;
        }

        /// <summary>
        ///		行からDTXデータを読み込み、スコアを生成する。
        /// </summary>
        /// <returns>生成されたスコア。</returns>
        public static スコア ReadFromString( string 全入力文字列 )
        {
            var スコア = new スコア();

            _行解析( ref スコア, ref 全入力文字列 );

            スコア.曲データファイルを読み込む_後処理だけ();

            return スコア;
        }

        // ※ テストプロジェクトに対しては InternalsVisibleTo 属性により internal メソッドを可視としているため、
        // 　 以降のテスト対象のメソッドは、本来 private でも internal として宣言している。
        internal class C行解析時の状態変数 : IDisposable
        {
            public スコア スコア = null;
            public string コマンド = "";
            public string パラメータ = "";
            public string コメント = "";
            public int 行番号 = 0;
            public int 小節番号 = 0;
            public int チャンネル番号 = 0;
            public int 小節解像度 = 384; // DTX の小節解像度。
            public チップ種別 チップ種別 = チップ種別.Unknown;
            public int オブジェクト総数 = 0;
            public int オブジェクト番号 = 0;

            // 小節長倍率の処理方法：
            // ・DTXでは指定した小節以降の小節長がすべて変わってしまうが、SSTでは指定した小節の小節長のみ変わる。
            // ・そのため、解析時は 小節長倍率マップ に指定があった小節の情報のみ残しておき、
            // 　後処理でまとめて スコア.小節長倍率リスト に登録するものとする。
            // ・なお、スコア.小節長倍率リスト には存在するすべての小節が登録されていなければならない。（SSTの仕様）

            /// <summary>
            ///		小節番号(int), 倍率(float)
            /// </summary>
            public SortedDictionary<int, float> 小節長倍率マップ = new SortedDictionary<int, float>();

            // #BPM, #BPMzz の処理方法：
            // ・#BPMzz は、BPM定義マップ[1～3599] に登録する。
            // ・#BPM は、#BPM00 とみなして BPM定義マップ[0] に登録する。
            // ・ch.03 (整数BPM) はそのまま SSTチップ(BPM) としてチップリストに登録する。
            // ・ch.08（拡張BPM）は、BPM値が 0 の SSTチップ(BPM) をチップリストに登録する。同時に、BPM参照マップ[チップ] にも登録する。
            // ・DTXファイル読み込み後の後処理として、BPM参照マップ に掲載されているすべての SSTチップ(BPM) の BPM値を、BPM定義マップ から引き当てて設定する。

            public float BASEBPM = 0.0f;

            /// <summary>
            ///		番号(int), BPM(float; DTX仕様）
            /// </summary>
            public Dictionary<int, float> BPM定義マップ = new Dictionary<int, float>();

            /// <summary>
            /// 	BPMチップ(チップ), BPM定義のzz番号(int);
            /// </summary>
            public Dictionary<チップ, int> BPM参照マップ = new Dictionary<チップ, int>();

            /// <summary>
            ///		zz番号(int), PAN値(int; 左:-100～中央:0～+100:右）
            /// </summary>
            public Dictionary<int, int> PAN定義マップ = new Dictionary<int, int>();

            /// <summary>
            ///		zz番号(int), VOLUME値(int; 0:無音 ～ 100:原音）
            /// </summary>
            public Dictionary<int, int> VOLUME定義マップ = new Dictionary<int, int>();

            public C行解析時の状態変数( スコア score )
            {
                this.スコア = score;
            }
            public void Dispose()
            {
                this.スコア = null;
            }
        }

        internal static void _行解析( ref スコア スコア, ref string 全入力文字列 )
        {
            Debug.WriteLineIf( Verbose, "行解析を開始します。" );

            // 現在の状態の初期化。
            var 現在の = new C行解析時の状態変数( スコア );

            // TAB は空白に置換しておく。
            全入力文字列 = 全入力文字列.Replace( '\t', ' ' );

            // すべての行について解析。
            using( var sr = new StringReader( 全入力文字列 ) )
            {
                string 行;

                // １行ずつ処理。
                for( 現在の.行番号 = 1; ( 行 = sr.ReadLine() ) != null; 現在の.行番号++ )
                {
                    // 行を、コマンド・パラメータ・コメントに分解。
                    if( !( _行分解( 行, out string コマンド, out string パラメータ, out string コメント ) ) )
                    {
                        Debug.WriteLineIf( Verbose, $"{現在の.行番号}: 行分解に失敗しました。" );
                        continue;
                    }
                    if( string.IsNullOrEmpty( コマンド ) )
                        continue;

                    現在の.コマンド = コマンド;
                    現在の.パラメータ = パラメータ;
                    現在の.コメント = コメント;

                    // 行処理。
                    var done =
                        _行解析_TITLE( 現在の ) ||
                        _行解析_ARTIST( 現在の ) ||
                        _行解析_COMMENT( 現在の ) ||
                        _行解析_PREIMAGE( 現在の ) ||
                        _行解析_BASEBPM( 現在の ) ||
                        _行解析_BPM( 現在の ) ||
                        _行解析_BPMzz( 現在の ) ||
                        _行解析_WAVPANzz_PANzz( 現在の ) ||
                        _行解析_WAVVOLzz_VOLUMEzz( 現在の ) ||
                        _行解析_PATH_WAV( 現在の ) ||
                        _行解析_WAVzz( 現在の ) ||
                        _行解析_AVIzz( 現在の ) ||
                        _行解析_DLEVEL( 現在の ) ||
                        _行解析_オブジェクト記述( 現在の );

                    // 行処理に失敗
                    //if( !( done ) )
                    //	Debug.WriteLineIf( Verbose, $"{現在の.行番号}: 未知のコマンドが使用されました。スキップします。[{現在の.コマンド}]" );
                    //	--> レーン情報とかでいっぱい出るのでコメントアウト
                }
            }

            #region " 後処理(1) BPMチップの値を引き当てる。"
            //----------------
            foreach( var kvp in 現在の.BPM参照マップ )
            {
                // BPMチャンネルから作成された BPMチップには、BASEBPM を加算する。
                // #BASEBPM が複数宣言されていた場合は、最後の値が使用される。
                kvp.Key.BPM =
                    現在の.BPM定義マップ[ kvp.Value ] +
                    現在の.BASEBPM;
            }

            // 引き当てが終わったら、マップが持つチップへの参照を解放する。
            現在の.BPM参照マップ.Clear();
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
            #region " 後処理(3) チップのPAN値, VOLUME値を引き当てる。"
            //----------------
            foreach( var chip in スコア.チップリスト )
            {
                // このDTXreaderクラスの実装で対応しているチャンネルで、
                var query = _チャンネルプロパティ.Where( ( r ) => ( r.chipType == chip.チップ種別 ) ); // タプル型なので SingleOrDefault は使えない。

                if( 1 == query.Count() )
                {
                    var record = query.Single();

                    // WAV を使うチャンネルで、
                    if( record.Wavを使う )
                    {
                        // PAN の指定があるなら、
                        if( 現在の.PAN定義マップ.ContainsKey( chip.チップサブID ) )
                        {
                            // それをチップに設定する。
                            chip.位置 = 現在の.PAN定義マップ[ chip.チップサブID ];
                        }

                        // VOLUME の指定があるなら、
                        if( 現在の.VOLUME定義マップ.ContainsKey( chip.チップサブID ) )
                        {
                            // それをチップに設定する。
                            var dtxvol = Math.Min( Math.Max( 現在の.VOLUME定義マップ[ chip.チップサブID ], 0 ), 100 );
                            chip.音量 = ( 0 == dtxvol ) ? 1 : // SSTFには「無音」はない。
                                (int) Math.Ceiling( ( 8.0 * dtxvol ) / 100.0 ); // 無音:0～100:原音 → 小:1～8:原音 に変換する。
                        }
                    }
                }
            }
            //----------------
            #endregion

            // 解析終了。
            Debug.WriteLineIf( Verbose, "行解析を終了しました。" );
        }

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

        internal static bool _行解析_TITLE( C行解析時の状態変数 現在の )
        {
            if( "title" != 現在の.コマンド.ToLower() )
                return false;

            現在の.スコア.曲名 = 現在の.パラメータ;

            return true;
        }
        internal static bool _行解析_ARTIST( C行解析時の状態変数 現在の )
        {
            if( "artist" != 現在の.コマンド.ToLower() )
                return false;

            現在の.スコア.アーティスト名 = 現在の.パラメータ;

            return true;
        }
        internal static bool _行解析_COMMENT( C行解析時の状態変数 現在の )
        {
            if( "comment" != 現在の.コマンド.ToLower() )
                return false;

            現在の.スコア.説明文 = 現在の.パラメータ;

            return true;
        }
        internal static bool _行解析_BASEBPM( C行解析時の状態変数 現在の )
        {
            if( "basebpm" != 現在の.コマンド.ToLower() )
                return false;

            if( float.TryParse( 現在の.パラメータ, out float bpm値 ) )
            {
                現在の.BASEBPM = bpm値; // 上書き可
                return true;
            }
            else
            {
                return false;
            }
        }
        internal static bool _行解析_BPM( C行解析時の状態変数 現在の )
        {
            if( "bpm" != 現在の.コマンド.ToLower() )
                return false;

            if( float.TryParse( 現在の.パラメータ, out float bpm値 ) )
            {
                // ※ 無限管理には非対応。

                //bpm値 += 現在の.BASEBPM;	--> BPMチャンネルのBPMチップとは異なり、#BPM: の値には #BASEBPM を加算しない。（DTX仕様）

                // "#BPM:" に対応するBPMチップは、常に、小節番号==0かつ小節内位置==0に置かれる。
                var bpmChip = 現在の.スコア.チップリスト.FirstOrDefault( ( c ) => ( c.チップ種別 == チップ種別.BPM && c.小節番号 == 0 && c.小節内位置 == 0 ) );
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

                    現在の.スコア.チップリスト.Add( bpmChip );
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        internal static bool _行解析_BPMzz( C行解析時の状態変数 現在の )
        {
            if( !( 現在の.コマンド.ToLower().StartsWith( "bpm" ) ) || ( 5 != 現在の.コマンド.Length ) )
                return false;

            int zz = _36進数2桁の文字列を数値に変換して取得する( 現在の.コマンド.Substring( 3, 2 ) );
            if( ( 1 > zz ) || ( 36 * 36 - 1 < zz ) )
                return false;

            if( float.TryParse( 現在の.パラメータ, out float bpm値 ) && ( 0f < bpm値 ) && ( 1000f > bpm値 ) )  // 値域制限(<1000)はDTX仕様
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
        internal static bool _行解析_WAVPANzz_PANzz( C行解析時の状態変数 現在の )
        {
            int zz位置 = 0;

            if( 現在の.コマンド.ToLower().StartsWith( "wavpan" ) && ( 8 == 現在の.コマンド.Length ) )
            {
                zz位置 = 6;
            }
            else if( 現在の.コマンド.ToLower().StartsWith( "pan" ) && ( 5 == 現在の.コマンド.Length ) )
            {
                zz位置 = 3;
            }
            else
            {
                return false;
            }

            int zz = _36進数2桁の文字列を数値に変換して取得する( 現在の.コマンド.Substring( zz位置, 2 ) );
            if( ( 1 > zz ) || ( 36 * 36 - 1 < zz ) )
                return false;

            if( int.TryParse( 現在の.パラメータ, out int pan値 ) )
            {
                // ※ 無限管理には非対応。
                現在の.PAN定義マップ[ zz ] = Math.Min( Math.Max( pan値, -100 ), +100 );
                return true;
            }
            else
            {
                return false;
            }
        }
        internal static bool _行解析_WAVVOLzz_VOLUMEzz( C行解析時の状態変数 現在の )
        {
            int zz位置 = 0;

            if( 現在の.コマンド.ToLower().StartsWith( "wavvol" ) && ( 8 == 現在の.コマンド.Length ) )
            {
                zz位置 = 6;
            }
            else if( 現在の.コマンド.ToLower().StartsWith( "volume" ) && ( 8 == 現在の.コマンド.Length ) )
            {
                zz位置 = 6;
            }
            else
            {
                return false;
            }

            int zz = _36進数2桁の文字列を数値に変換して取得する( 現在の.コマンド.Substring( zz位置, 2 ) );
            if( ( 1 > zz ) || ( 36 * 36 - 1 < zz ) )
                return false;

            if( int.TryParse( 現在の.パラメータ, out int pan値 ) )
            {
                // ※ 無限管理には非対応。
                現在の.VOLUME定義マップ[ zz ] = Math.Min( Math.Max( pan値, 0 ), 100 );
                return true;
            }
            else
            {
                return false;
            }
        }
        internal static bool _行解析_PATH_WAV( C行解析時の状態変数 現在の )
        {
            if( "path_wav" != 現在の.コマンド.ToLower() )
                return false;

            現在の.スコア.PATH_WAV = 現在の.パラメータ;

            return true;
        }
        internal static bool _行解析_WAVzz( C行解析時の状態変数 現在の )
        {
            if( !( 現在の.コマンド.ToLower().StartsWith( "wav" ) ) || ( 5 != 現在の.コマンド.Length ) )
                return false;

            int zz = _36進数2桁の文字列を数値に変換して取得する( 現在の.コマンド.Substring( 3, 2 ) );
            if( ( 1 > zz ) || ( 36 * 36 - 1 < zz ) )
                return false;

            // すでに存在する zz 番号の場合は、新しいほうのみを有効とする。
            if( 現在の.スコア.dicWAV.ContainsKey( zz ) )
                現在の.スコア.dicWAV.Remove( zz );    // 古いほうは削除。

            // ここでは、PATH_WAV はまだ反映しない。
            現在の.スコア.dicWAV.Add( zz, (現在の.パラメータ, true) );
            return true;
        }
        internal static bool _行解析_AVIzz( C行解析時の状態変数 現在の )
        {
            if( !( 現在の.コマンド.ToLower().StartsWith( "avi" ) ) || ( 5 != 現在の.コマンド.Length ) )
                return false;

            int zz = _36進数2桁の文字列を数値に変換して取得する( 現在の.コマンド.Substring( 3, 2 ) );
            if( ( 1 > zz ) || ( 36 * 36 - 1 < zz ) )
                return false;

            // すでに存在する zz 番号の場合は、新しいほうのみを有効とする。
            if( 現在の.スコア.dicAVI.ContainsKey( zz ) )
                現在の.スコア.dicAVI.Remove( zz );    // 古いほうは削除。

            // ここでは、PATH_WAV はまだ反映しない。
            現在の.スコア.dicAVI.Add( zz, 現在の.パラメータ );
            return true;
        }
        internal static bool _行解析_DLEVEL( C行解析時の状態変数 現在の )
        {
            if( "dlevel" != 現在の.コマンド.ToLower() )
                return false;

            if( int.TryParse( 現在の.パラメータ, out int level ) )
            {
                現在の.スコア.難易度 = Math.Min( Math.Max( level, 0 ), 99 ) / 10.0f;     // 0～99 → 0.0 ～ 9.9
                return true;
            }
            else
            {
                return false;
            }
        }
        internal static bool _行解析_PREIMAGE( C行解析時の状態変数 現在の )
        {
            if( "preimage" != 現在の.コマンド.ToLower() )
                return false;

            現在の.スコア.プレビュー画像 = 現在の.パラメータ;

            return true;
        }
        internal static bool _行解析_オブジェクト記述( C行解析時の状態変数 現在の )
        {
            if( !( _小節番号とチャンネル番号を取得する( 現在の.コマンド, out 現在の.小節番号, out 現在の.チャンネル番号 ) ) )
                return false;

            現在の.パラメータ = 現在の.パラメータ.Replace( "_", "" );  // 見やすさのために '_' を区切り文字として使用できる（DTX仕様）
            現在の.パラメータ = 現在の.パラメータ.ToLower();            // すべて小文字化（三十六進数変換表には大文字がないので）

            if( 0x02 == 現在の.チャンネル番号 )
            {
                #region " ch02 小節長倍率 "
                //----------------
                if( !( _DTX仕様の実数を取得する( 現在の.パラメータ, out float 小節長倍率 ) ) )
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

            現在の.オブジェクト総数 = 現在の.パラメータ.Length / 2;    // 1オブジェクトは2文字；余った末尾は切り捨てる。（DTX仕様）

            // すべてのオブジェクトについて...
            for( 現在の.オブジェクト番号 = 0; 現在の.オブジェクト番号 < 現在の.オブジェクト総数; 現在の.オブジェクト番号++ )
            {
                int オブジェクト値 = 0;

                #region " オブジェクト値を取得。"
                //----------------
                if( 0x03 == 現在の.チャンネル番号 )
                {
                    // (A) ch03 (BPM) のみ 16進数2桁表記
                    オブジェクト値 = Convert.ToInt32( 現在の.パラメータ.Substring( 現在の.オブジェクト番号 * 2, 2 ), 16 );
                }
                else
                {
                    // (B) その他は 36進数2桁表記
                    オブジェクト値 = _36進数2桁の文字列を数値に変換して取得する( 現在の.パラメータ.Substring( 現在の.オブジェクト番号 * 2, 2 ) );
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
                    チップサブID = オブジェクト値,
                    小節番号 = 現在の.小節番号,
                    小節解像度 = 現在の.小節解像度,
                    小節内位置 = (int) ( 現在の.小節解像度 * 現在の.オブジェクト番号 / 現在の.オブジェクト総数 ),
                    音量 = チップ.最大音量,
                };

                // チャンネルに対応するチャンネルプロパティを取得。
                var queryChannelProperties = _チャンネルプロパティ.Where( ( r ) => ( 現在の.チャンネル番号 == r.channel ) );
                if( 1 != queryChannelProperties.Count() )
                    continue;   // なかったら未対応のチャンネルとして無視。
                var channelProperty = queryChannelProperties.Single();

                // チャンネルに対応するチップ種別を設定。
                chip.チップ種別 = channelProperty.chipType;

                // その他の処理があればを行う。
                switch( 現在の.チャンネル番号 )
                {
                    // BPM
                    case 0x03:
                        chip.BPM = (double) ( オブジェクト値 + 現在の.BASEBPM );  // 引き当てないので、ここでBASEBPMを加算する。
                        break;

                    // 拡張BPM
                    case 0x08:
                        chip.BPM = 0.0; // あとで引き当てる。BASEBPMは引き当て時に加算する。
                        現在の.BPM参照マップ.Add( chip, オブジェクト値 );  // 引き当てを予約。
                        break;

                    // SE1～5
                    case int ch when( 0x61 <= ch && ch <= 0x65 ):
                        _WAVの多重再生を無効にする( オブジェクト値 );
                        break;

                    // チップ配置（ギター）
                    case int ch when( 0x20 <= ch && ch <= 0x27 ):
                        _WAVの多重再生を無効にする( オブジェクト値 );
                        break;

                    // チップ配置（ベース）
                    case int ch when( 0xA0 <= ch && ch <= 0xA7 ):
                        _WAVの多重再生を無効にする( オブジェクト値 );
                        break;

                    // 空打ち（ドラム）
                    case 0xB1: 現在の.スコア.空打ちチップ[ レーン種別.HiHat ] = オブジェクト値; break;      // HiHat Open と共有
                    case 0xB2: 現在の.スコア.空打ちチップ[ レーン種別.Snare ] = オブジェクト値; break;
                    case 0xB3: 現在の.スコア.空打ちチップ[ レーン種別.Bass ] = オブジェクト値; break;
                    case 0xB4: 現在の.スコア.空打ちチップ[ レーン種別.Tom1 ] = オブジェクト値; break;
                    case 0xB5: 現在の.スコア.空打ちチップ[ レーン種別.Tom2 ] = オブジェクト値; break;
                    case 0xB6:
                        現在の.スコア.空打ちチップ[ レーン種別.RightCrash ] = オブジェクト値;               // ReftCrash と China は共有
                        現在の.スコア.空打ちチップ[ レーン種別.China ] = オブジェクト値;
                        break;
                    case 0xB7: 現在の.スコア.空打ちチップ[ レーン種別.Tom3 ] = オブジェクト値; break;
                    case 0xB8: 現在の.スコア.空打ちチップ[ レーン種別.HiHat ] = オブジェクト値; break;      // HiHat Close と共有
                    case 0xB9: 現在の.スコア.空打ちチップ[ レーン種別.Ride ] = オブジェクト値; break;
                    case 0xBC:
                        現在の.スコア.空打ちチップ[ レーン種別.LeftCrash ] = オブジェクト値;                    // LeftCrash と Splash は共有
                        現在の.スコア.空打ちチップ[ レーン種別.Splash ] = オブジェクト値;
                        break;
                }

                // チップリストに登録。
                現在の.スコア.チップリスト.Add( chip );
            }
            return true;

            // ローカル関数
            void _WAVの多重再生を無効にする( int WAV番号 )
            {
                var dicWAV = 現在の.スコア.dicWAV;

                if( dicWAV.ContainsKey( WAV番号 ) )
                    dicWAV[ WAV番号 ] = (dicWAV[ WAV番号 ].ファイルパス, 多重再生する: false);
            }
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
        internal static int _36進数2桁の文字列を数値に変換して取得する( string 文字列 )
        {
            Debug.Assert( 2 == 文字列.Length );

            var 変換文字列 = 文字列.ToLower();

            return
                _三十六進数変換表.IndexOf( 変換文字列[ 0 ] ) * 36 +
                _三十六進数変換表.IndexOf( 変換文字列[ 1 ] );
        }

        private static readonly string _三十六進数変換表 = "0123456789abcdefghijklmnopqrstuvwxyz";  // めんどいので大文字は考慮しない。参照元で小文字に変換のこと。

        /// <summary>
        ///		チャンネル情報データベース。
        /// </summary>
        private static readonly List<(int channel, チップ種別 chipType, bool Wavを使う)> _チャンネルプロパティ = new List<(int channel, チップ種別 chipType, bool Wavを使う)>() {
            ( 0x01, チップ種別.BGM, true ),			// バックコーラス(BGM)
			//( 0x02, チップ種別.Unknown, false ),		// 小節長倍率	チップじゃない。
			( 0x03, チップ種別.BPM, false ),			// BPM
			( 0x08, チップ種別.BPM, false ),			// 拡張BPM
			( 0x11, チップ種別.HiHat_Close, true ),	// チップ配置（ドラム）・ハイハットクローズ
			( 0x12, チップ種別.Snare, true ),		// チップ配置（ドラム）・スネア
			( 0x13, チップ種別.Bass, true ),			// チップ配置（ドラム）・バス
			( 0x14, チップ種別.Tom1, true ),			// チップ配置（ドラム）・ハイタム
			( 0x15, チップ種別.Tom2, true ),			// チップ配置（ドラム）・ロータム
			( 0x16, チップ種別.RightCrash, true ),	// チップ配置（ドラム）・右シンバル
			( 0x17, チップ種別.Tom3, true ),			// チップ配置（ドラム）・フロアタム
			( 0x18, チップ種別.HiHat_Open, true ),	// チップ配置（ドラム）・ハイハットオープン
			( 0x19, チップ種別.Ride, true ),			// チップ配置（ドラム）・ライドシンバル
			( 0x1A, チップ種別.LeftCrash, true ),	// チップ配置（ドラム）・左シンバル
			( 0x1B, チップ種別.HiHat_Foot, true ),	// チップ配置（ドラム）・左ペダル
			( 0x1C, チップ種別.LeftBass, true ),		// チップ配置（ドラム）・左バス
			( 0x20, チップ種別.GuitarAuto, true ),	// チップ配置（ギター）・OPEN
			( 0x21, チップ種別.GuitarAuto, true ),	// チップ配置（ギター）・xxB
			( 0x22, チップ種別.GuitarAuto, true ),	// チップ配置（ギター）・xGx
			( 0x23, チップ種別.GuitarAuto, true ),	// チップ配置（ギター）・xGB
			( 0x24, チップ種別.GuitarAuto, true ),	// チップ配置（ギター）・Rxx
			( 0x25, チップ種別.GuitarAuto, true ),	// チップ配置（ギター）・RxB
			( 0x26, チップ種別.GuitarAuto, true ),	// チップ配置（ギター）・RGx
			( 0x27, チップ種別.GuitarAuto, true ),	// チップ配置（ギター）・RGB
			( 0x50, チップ種別.小節線, false ),		// 小節線
			( 0x51, チップ種別.拍線, false ),			// 拍線
			( 0x54, チップ種別.背景動画, false ),		// 動画
			( 0x5A, チップ種別.背景動画, false ),		// 動画（全画面）
			( 0x61, チップ種別.SE1, true ),			// SE1
			( 0x62, チップ種別.SE2, true ),			// SE2
			( 0x63, チップ種別.SE3, true ),			// SE3
			( 0x64, チップ種別.SE4, true ),			// SE4
			( 0x65, チップ種別.SE5, true ),			// SE5
			( 0xA0, チップ種別.BassAuto, true ),		// チップ配置（ベース）・OPEN
			( 0xA1, チップ種別.BassAuto, true ),		// チップ配置（ベース）・xxB
			( 0xA2, チップ種別.BassAuto, true ),		// チップ配置（ベース）・xGx
			( 0xA3, チップ種別.BassAuto, true ),		// チップ配置（ベース）・xGB
			( 0xA4, チップ種別.BassAuto, true ),		// チップ配置（ベース）・Rxx
			( 0xA5, チップ種別.BassAuto, true ),		// チップ配置（ベース）・RxB
			( 0xA6, チップ種別.BassAuto, true ),		// チップ配置（ベース）・RGx
			( 0xA7, チップ種別.BassAuto, true ),		// チップ配置（ベース）・RGB
		};
    }
}
