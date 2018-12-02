using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SSTFormat.v3
{
    public class スコア : IDisposable
    {
        /// <summary>
        ///		  このソースが実装するSSTFバージョン。
        /// </summary>
        public Version SSTFVERSION { get; } = new Version( 3, 3, 0, 0 );

        public const double 初期BPM = 120.0;
        public const double 初期小節解像度 = 480.0;

        /// <summary>
        ///		1ms あたりのピクセル数。
        /// </summary>
        /// <remarks>
        ///		BPM 150 のとき、1小節が 234 ピクセルになるように調整。
        ///		→ 60秒で150拍のとき、1小節(4拍)が 234 dpx。
        ///		→ 60秒の間に、150[拍]÷4[拍]＝37.5[小節]。
        ///		→ 60秒の間に、37.5[小節]×234[dpx/小節]＝ 8775[dpx]。
        ///		→ 1ms の間に、8775[dpx]÷60000[ms]＝0.14625[dpx/ms]。割り切れて良かった。
        /// </remarks>
        public const double 基準譜面速度dpxms = 0.14625 * 2.25;  // "* 2.25" は「x1.0はもう少し速くてもいいんではないか？」という感覚的な調整分。
                                                           /// <summary>
                                                           ///		1秒あたりの設計ピクセル数 [dpx] 。
                                                           /// </summary>
        public const double 基準譜面速度dpxsec = 基準譜面速度dpxms * 1000.0;

        public static readonly List<string> 背景動画のデフォルト拡張子リスト = new List<string>() {
            ".mp4", ".avi", ".wmv", ".mpg", ".mpeg"
        };

        // 外部向けヘルパ

        /// <summary>
        ///		指定されたコマンド名が対象文字列内で使用されている場合に、パラメータ部分の文字列を返す。
        /// </summary>
        /// <remarks>
        ///		.dtx や box.def 等で使用されている "#＜コマンド名＞[:]＜パラメータ＞[;コメント]" 形式の文字列（対象文字列）について、
        ///		指定されたコマンドを使用する行であるかどうかを判別し、使用する行であるなら、そのパラメータ部分の文字列を引数に格納し、true を返す。
        ///		対象文字列のコマンド名が指定したコマンド名と異なる場合には、パラメータ文字列に null を格納して false を返す。
        ///		コマンド名は正しくてもパラメータが存在しない場合には、空文字列("") を格納して true を返す。
        /// </remarks>
        /// <param name="対象文字列">調べる対象の文字列。（例: "#TITLE: 曲名 ;コメント"）</param>
        /// <param name="コマンド名">調べるコマンドの名前（例:"TITLE"）。#は不要、大文字小文字は区別されない。</param>
        /// <returns>パラメータ文字列の取得に成功したら true、異なるコマンドだったなら false。</returns>
        public static bool コマンドのパラメータ文字列部分を返す( string 対象文字列, string コマンド名, out string パラメータ文字列 )
        {
            // コメント部分を除去し、両端をトリムする。なお、全角空白はトリムしない。
            対象文字列 = 対象文字列.Split( ';' )[ 0 ].Trim( ' ', '\t' );

            string 正規表現パターン = $@"^\s*#{コマンド名}(:|\s)+(.*)\s*$";  // \s は空白文字。
            var m = Regex.Match( 対象文字列, 正規表現パターン, RegexOptions.IgnoreCase );

            if( m.Success && ( 3 <= m.Groups.Count ) )
            {
                パラメータ文字列 = m.Groups[ 2 ].Value;
                return true;
            }
            else
            {
                パラメータ文字列 = null;
                return false;
            }
        }


        // ヘッダ

        /// <summary>
        ///		ファイルのSSTFバージョン。
        ///		ファイルにバージョンの指定がない場合は v1.0.0.0 とみなす。
        /// </summary>
        public Version SSTFバージョン { get; protected set; } = new Version( 1, 0, 0, 0 );

        public string 曲名 { get; set; } = "(no title)";

        public string アーティスト名 { get; set; } = "";

        public string 説明文 { get; set; } = "";

        public float サウンドデバイス遅延ms { get; set; } = 0f;

        /// <summary>
        ///		0.00～9.99。
        /// </summary>
        public float 難易度 { get; set; } = 5.0f;

        /// <summary>
        ///		動画ファイルパス。
        ///		SSTF用。
        /// </summary>
        public string 背景動画ファイル名 { get; set; } = null;

        /// <summary>
        ///		プレビュー画像。DTX用。
        /// </summary>
        public string プレビュー画像 { get; set; } = null;

        /// <summary>
        ///     動画のID。SSTF用。
        ///     指定がある場合、<see cref="背景動画ファイル名"/>より優先される。
        /// </summary>
        /// <remarks>
        ///     書式: "プロトコル: 動画ID", 大文字小文字は区別されない。
        ///     　例: ニコニコ動画の場合 …… "NicoVideo: sm12345678"
        /// </remarks>
        public string 動画ID { get; set; } = null;


        // プロパティ

        public List<チップ> チップリスト { get; set; } = new List<チップ>();

        /// <summary>
        ///		インデックス番号が小節番号を表すので、
        ///		小節 0 から最大小節まで、すべての小節の倍率がこのリストに含まれる。
        /// </summary>
        public List<double> 小節長倍率リスト { get; protected set; } = new List<double>();

        public int 最大小節番号
        {
            get
            {
                int 最大小節番号 = 0;

                foreach( チップ chip in this.チップリスト )
                {
                    if( chip.小節番号 > 最大小節番号 )
                        最大小節番号 = chip.小節番号;
                }

                return 最大小節番号;
            }
        }

        public Dictionary<int, string> dicメモ { get; protected set; } = new Dictionary<int, string>();

        /// <summary>
        ///		#WAVzz で指定された、サウンドファイルへの相対パス他。
        ///		パスの基点は、#PATH_WAV があればそこ、なければ曲譜面ファイルと同じ場所。
        /// </summary>
        public Dictionary<int, (string ファイルパス, bool 多重再生する)> dicWAV { get; protected set; } = new Dictionary<int, (string ファイルパス, bool 多重再生する)>();

        /// <summary>
        ///		#AVIzz で指定された、動画ファイルへの相対パス。
        ///		パスの基点は、#PATH_WAV があればそこ、なければ曲譜面ファイルと同じ場所。
        ///		DTX用。
        /// </summary>
        public Dictionary<int, string> dicAVI
        {
            get;
            protected set;
        } = new Dictionary<int, string>();

        /// <summary>
        ///		WAV/AVIファイルの基点フォルダの絶対パス。
        ///		末尾は '\' 。（例: "D:\DTXData\DemoSong\Sounds\"）
        /// </summary>
        public string PATH_WAV
        {
            get
            {
                if( null != this.譜面ファイルパス )
                    return Path.Combine( Path.GetDirectoryName( this.譜面ファイルパス ), this._PATH_WAV );
                else
                    return this._PATH_WAV;
            }

            set
            {
                this._PATH_WAV = value;

                if( this._PATH_WAV.Last() != '\\' )
                    this._PATH_WAV += '\\';

                //if( !( Directory.Exists( this.PATH_WAV ) ) )
                //	throw new DirectoryNotFoundException( "PATH_WAV に存在しないフォルダが指定されました。" );
            }
        }

        /// <summary>
        ///		譜面ファイルの絶対パス。
        /// </summary>
        public string 譜面ファイルパス
        {
            get;
            set;
        } = null;

        /// <summary>
        ///		レーン種別, zz番号。
        ///		空打ちチップが指定されている場合はそのWAVzzのzz番号を、指定されていないなら 0 を保持する。
        /// </summary>
        public Dictionary<レーン種別, int> 空打ちチップ
        {
            get;
            protected set;
        } = new Dictionary<レーン種別, int>();


        public スコア()
        {
            this._初期化する();
        }

        public スコア( string 曲データファイル名 )
            : this()
        {
            this.曲データファイルを読み込む( 曲データファイル名 );
        }

        public void Dispose()
        {
        }

        /// <remarks>
        ///		失敗すれば何らかの例外を発出する。
        /// </remarks>
        public void 曲データファイルを読み込む( string 曲データファイル名 )
        {
            this.譜面ファイルパス = Path.GetFullPath( 曲データファイル名 );
            if( !( File.Exists( this.譜面ファイルパス ) ) )
                throw new FileNotFoundException( $"指定されたファイルが存在しません。" );

            var ファイルのSSTFバージョン = Version.CreateVersionFromFile( 曲データファイル名 );

            // ファイルのSSTFバージョンによって処理分岐。
            if( SSTFVERSION.Major == ファイルのSSTFバージョン.Major )
            {
                this._v3曲データファイルを読み込む( 曲データファイル名 );
            }
            else
            {
                var v2score = new SSTFormat.v2.スコア( 曲データファイル名 );
                this._v2スコアからマイグレーションする( v2score );
            }

            this.曲データファイルを読み込む_後処理だけ();
        }

        public void 曲データファイルを読み込む_ヘッダだけ( string 曲データファイル名 )
        {
            this.譜面ファイルパス = Path.GetFullPath( 曲データファイル名 );
            if( !( File.Exists( this.譜面ファイルパス ) ) )
                throw new FileNotFoundException( $"指定されたファイルが存在しません。" );

            var ファイルのSSTFバージョン = Version.CreateVersionFromFile( 曲データファイル名 );

            // ファイルのSSTFバージョンによって処理分岐。
            if( SSTFVERSION.Major == ファイルのSSTFバージョン.Major )
            {
                this._v3曲データファイルを読み込む_ヘッダだけ( 曲データファイル名 );
            }
            else
            {
                var v2score = new SSTFormat.v2.スコア();
                v2score.曲データファイルを読み込む_ヘッダだけ( 曲データファイル名 );
                this._v2スコアからマイグレーションする_ヘッダだけ( v2score );
            }
        }
        /// <summary>
        ///		すでにスコアの構築が完了しているものとして、チップリストへの後処理（小節線・拍線の追加、発声時刻の計算など）のみ行う。
        /// </summary>
        public void 曲データファイルを読み込む_後処理だけ()
        {
            #region " 拍線の追加。小節線を先に追加すると小節が１つ増えるので、先に拍線から追加する。"
            //-----------------
            int 最大小節番号 = this.最大小節番号;       // this.最大小節番号 プロパティはチップ数に依存して変化するので、for 文には組み込まないこと。

            for( int i = 0; i <= 最大小節番号; i++ )
            {
                double 小節長倍率 = this.小節長倍率を取得する( i );
                for( int n = 1; n * 0.25 < 小節長倍率; n++ )
                {
                    this.チップリスト.Add(
                        new チップ() {
                            小節番号 = i,
                            チップ種別 = チップ種別.拍線,
                            小節内位置 = (int) ( ( n * 0.25 ) * 100 ),
                            小節解像度 = (int) ( 小節長倍率 * 100 ),
                        } );
                }
            }
            //-----------------
            #endregion
            #region " 小節線の追加。"
            //-----------------
            最大小節番号 = this.最大小節番号;

            for( int i = 0; i <= 最大小節番号 + 1; i++ )
            {
                this.チップリスト.Add(
                    new チップ() {
                        小節番号 = i,
                        チップ種別 = チップ種別.小節線,
                        小節内位置 = 0,
                        小節解像度 = 1,
                    } );
            }
            //-----------------
            #endregion
            #region " 小節の先頭 の追加。"
            //----------------
            最大小節番号 = this.最大小節番号;

            // 「小節の先頭」チップは、小節線と同じく、全小節の先頭位置に置かれる。
            // 小節線には今後譜面作者によって位置をアレンジできる可能性を残したいが、
            // ビュアーが小節の先頭位置を検索するためには、小節の先頭に置かれるチップが必要になる。
            // よって、譜面作者の影響を受けない（ビュアー用の）チップを機械的に配置する。

            for( int i = 0; i <= 最大小節番号; i++ )
            {
                this.チップリスト.Add(
                    new チップ() {
                        小節番号 = i,
                        チップ種別 = チップ種別.小節の先頭,
                        小節内位置 = 0,
                        小節解像度 = 1,
                    } );
            }
            //----------------
            #endregion

            this.チップリスト.Sort();

            #region " 全チップの発声/描画時刻と譜面内位置を計算する。"
            //-----------------

            // 1. BPMチップを無視し(初期BPMで固定)、dic小節長倍率, Cチップ.小節解像度, Cチップ.小節内位置 から両者を計算する。
            //    以下、チップリストが小節番号順にソートされているという前提で。

            double チップが存在する小節の先頭時刻ms = 0.0;
            int 現在の小節の番号 = 0;

            foreach( チップ chip in this.チップリスト )
            {
                #region " チップの小節番号が現在の小節番号よりも大きい場合、チップが存在する小節に至るまで、「dbチップが存在する小節の先頭時刻ms」を更新する。"
                //-----------------
                while( 現在の小節の番号 < chip.小節番号 )
                {
                    double 現在の小節の小節長倍率 = this.小節長倍率を取得する( 現在の小節の番号 );
                    チップが存在する小節の先頭時刻ms += _BPM初期値固定での1小節4拍の時間ms * 現在の小節の小節長倍率;

                    現在の小節の番号++;    // 現在の小節番号 が chip.小節番号 に追いつくまでループする。
                }
                //-----------------
                #endregion
                #region " チップの発声/描画時刻を求める。"
                //-----------------
                double チップが存在する小節の小節長倍率 = this.小節長倍率を取得する( 現在の小節の番号 );

                chip.発声時刻sec =
                    chip.描画時刻sec =
                        ( チップが存在する小節の先頭時刻ms + ( _BPM初期値固定での1小節4拍の時間ms * チップが存在する小節の小節長倍率 * chip.小節内位置 ) / chip.小節解像度 ) / 1000.0;
                //-----------------
                #endregion
            }

            // 2. BPMチップを考慮しながら調整する。（譜面内位置grid はBPMの影響を受けないので無視）

            double 現在のBPM = スコア.初期BPM;
            int チップ数 = this.チップリスト.Count;
            for( int i = 0; i < チップ数; i++ )
            {
                // BPM チップ以外は無視。
                var BPMチップ = this.チップリスト[ i ];
                if( BPMチップ.チップ種別 != チップ種別.BPM )
                    continue;

                // BPMチップより後続の全チップの n発声/描画時刻ms を、新旧BPMの比率（加速率）で修正する。
                double 加速率 = BPMチップ.BPM / 現在のBPM; // BPMチップ.dbBPM > 0.0 であることは読み込み時に保証済み。
                for( int j = i + 1; j < チップ数; j++ )
                {
                    double 時刻sec = BPMチップ.発声時刻sec + ( this.チップリスト[ j ].発声時刻sec - BPMチップ.発声時刻sec ) / 加速率;
                    this.チップリスト[ j ].発声時刻sec = 時刻sec;
                    this.チップリスト[ j ].描画時刻sec = 時刻sec;
                }

                現在のBPM = BPMチップ.BPM;
            }
            //-----------------
            #endregion
        }

        /// <summary>
        ///		現在の スコア の内容をデータファイル（*.sstf）に書き出す。
        /// </summary>
        /// <remarks>
        ///		小節線、拍線、Unknown チップは出力しない。
        ///		失敗すれば何らかの例外を発出する。
        /// </remarks>
        public void 曲データファイルを書き出す( string 曲データファイル名, string ヘッダ行 )
        {
            using( var sw = new StreamWriter( 曲データファイル名, false, Encoding.UTF8 ) )
            {
                // SSTFバージョンの出力
                sw.WriteLine( $"# SSTFVersion {this.SSTFVERSION.ToString()}" );

                // ヘッダ行の出力
                sw.WriteLine( $"{ヘッダ行}" );    // strヘッダ行に"{...}"が入ってても大丈夫なようにstring.Format()で囲む。
                sw.WriteLine( "" );

                // ヘッダコマンド行の出力
                sw.WriteLine( "Title=" + ( ( string.IsNullOrEmpty( this.曲名 ) ) ? "(no title)" : this.曲名 ) );
                if( !string.IsNullOrEmpty( this.説明文 ) )
                {
                    // 改行コードは、２文字のリテラル "\n" に置換。
                    sw.WriteLine( $"Description=" + this.説明文.Replace( Environment.NewLine, @"\n" ) );
                }
                sw.WriteLine( $"SoundDevice.Delay={this.サウンドデバイス遅延ms}" );
                sw.WriteLine( $"Level={this.難易度.ToString( "0.00" )}" );
                sw.WriteLine( "" );

                // 全チップの出力

                #region " 全チップの最終小節番号を取得する。"
                //-----------------
                int 最終小節番号 = 0;
                foreach( var cc in this.チップリスト )
                {
                    if( cc.小節番号 > 最終小節番号 )
                        最終小節番号 = cc.小節番号;
                }
                //-----------------
                #endregion

                for( int 小節番号 = 0; 小節番号 <= 最終小節番号; 小節番号++ )
                {
                    #region " dicレーン別チップリストの初期化。"
                    //-----------------
                    var dicレーン別チップリスト = new Dictionary<レーン種別, List<チップ>>();

                    foreach( レーン種別 laneType in Enum.GetValues( typeof( レーン種別 ) ) )
                        dicレーン別チップリスト[ laneType ] = new List<チップ>();
                    //-----------------
                    #endregion
                    #region " dicレーン別チップリストの構築； 小節番号 の小節に存在するチップのみをレーン別に振り分けて格納する。"
                    //-----------------
                    foreach( var cc in this.チップリスト )
                    {
                        // 出力しないチップ種別は無視。
                        if( cc.チップ種別 == チップ種別.小節線 ||
                            cc.チップ種別 == チップ種別.拍線 ||
                            cc.チップ種別 == チップ種別.小節メモ ||
                            cc.チップ種別 == チップ種別.小節の先頭 ||
                            cc.チップ種別 == チップ種別.BGM ||
                            cc.チップ種別 == チップ種別.SE1 ||
                            cc.チップ種別 == チップ種別.SE2 ||
                            cc.チップ種別 == チップ種別.SE3 ||
                            cc.チップ種別 == チップ種別.SE4 ||
                            cc.チップ種別 == チップ種別.SE5 ||
                            cc.チップ種別 == チップ種別.GuitarAuto ||
                            cc.チップ種別 == チップ種別.BassAuto ||
                            cc.チップ種別 == チップ種別.Unknown )
                        {
                            continue;
                        }

                        if( cc.小節番号 > 小節番号 )
                        {
                            // チップリストは昇順に並んでいるので、これ以上検索しても無駄。
                            break;
                        }
                        else if( cc.小節番号 == 小節番号 )
                        {
                            var lane = レーン種別.Bass;   // 対応するレーンがなかったら Bass でも返しておく。

                            foreach( var kvp in _dicSSTFレーンチップ対応表 )
                            {
                                if( kvp.Value.Contains( cc.チップ種別 ) )
                                {
                                    lane = kvp.Key;
                                    break;
                                }
                            }

                            dicレーン別チップリスト[ lane ].Add( cc );
                        }
                    }
                    //-----------------
                    #endregion

                    #region " Part行 出力。"
                    //-----------------
                    sw.Write( $"Part = {小節番号.ToString()}" );

                    if( this.小節長倍率リスト[ 小節番号 ] != 1.0 )
                        sw.Write( $"s{this.小節長倍率リスト[ 小節番号 ].ToString()}" );

                    sw.WriteLine( ";" );
                    //-----------------
                    #endregion
                    #region " Lane, Resolution, Chip 行 出力。"
                    //-----------------
                    foreach( レーン種別 laneType in Enum.GetValues( typeof( レーン種別 ) ) )
                    {
                        if( 0 < dicレーン別チップリスト[ laneType ].Count )
                        {
                            sw.Write( $"Lane={laneType.ToString()}; " );

                            #region " 新しい解像度を求める。"
                            //-----------------
                            int 新しい解像度 = 1;
                            foreach( var cc in dicレーン別チップリスト[ laneType ] )
                                新しい解像度 = this._最小公倍数を返す( 新しい解像度, cc.小節解像度 );
                            //-----------------
                            #endregion
                            #region " dicレーン別チップリスト[ lane ] 要素の 小節解像度 と 小節内位置 を 新しい解像度 に合わせて修正する。 "
                            //-----------------
                            foreach( var cc in dicレーン別チップリスト[ laneType ] )
                            {
                                int 倍率 = 新しい解像度 / cc.小節解像度;      // 新しい解像度 は 小節解像度 の最小公倍数なので常に割り切れる。

                                cc.小節解像度 *= 倍率;
                                cc.小節内位置 *= 倍率;
                            }
                            //-----------------
                            #endregion

                            sw.Write( $"Resolution = {新しい解像度}; " );
                            sw.Write( "Chips = " );

                            for( int i = 0; i < dicレーン別チップリスト[ laneType ].Count; i++ )
                            {
                                チップ cc = dicレーン別チップリスト[ laneType ][ i ];

                                // 位置を出力。
                                sw.Write( cc.小節内位置.ToString() );

                                // 属性を出力（あれば）。

                                #region " (1) 共通属性 "
                                //-----------------
                                if( cc.音量 < チップ.最大音量 )
                                    sw.Write( $"v{cc.音量.ToString()}" );
                                //-----------------
                                #endregion
                                #region " (2) 専用属性 "
                                //-----------------
                                switch( cc.チップ種別 )
                                {
                                    case チップ種別.Ride_Cup:
                                        sw.Write( 'c' );
                                        break;

                                    case チップ種別.HiHat_Open:
                                        sw.Write( 'o' );
                                        break;

                                    case チップ種別.HiHat_HalfOpen:
                                        sw.Write( 'h' );
                                        break;

                                    case チップ種別.HiHat_Foot:
                                        sw.Write( 'f' );
                                        break;

                                    case チップ種別.Snare_OpenRim:
                                        sw.Write( 'o' );
                                        break;

                                    case チップ種別.Snare_ClosedRim:
                                        sw.Write( 'c' );
                                        break;

                                    case チップ種別.Snare_Ghost:
                                        sw.Write( 'g' );
                                        break;

                                    case チップ種別.Tom1_Rim:
                                        sw.Write( 'r' );
                                        break;

                                    case チップ種別.Tom2_Rim:
                                        sw.Write( 'r' );
                                        break;

                                    case チップ種別.Tom3_Rim:
                                        sw.Write( 'r' );
                                        break;

                                    case チップ種別.LeftCymbal_Mute:
                                        sw.Write( 'm' );
                                        break;

                                    case チップ種別.RightCymbal_Mute:
                                        sw.Write( 'm' );
                                        break;

                                    case チップ種別.BPM:
                                        sw.Write( $"b{cc.BPM.ToString()}" );
                                        break;
                                }
                                //-----------------
                                #endregion

                                // 区切り文字 または 終端文字 を出力
                                sw.Write( ( i == dicレーン別チップリスト[ laneType ].Count - 1 ) ? ";" : "," );
                            }

                            sw.WriteLine( "" ); // 改行
                        }
                    }
                    //-----------------
                    #endregion

                    sw.WriteLine( "" ); // 次の Part 前に１行あける。
                }

                // メモ（小節単位）の出力

                #region " dicメモ を小節番号で昇順に出力する。"
                //-----------------
                var dic昇順メモ = new Dictionary<int, string>();
                int 最大小節番号 = this.最大小節番号;

                for( int i = 0; i <= 最大小節番号; i++ )
                {
                    if( this.dicメモ.ContainsKey( i ) )
                        dic昇順メモ.Add( i, this.dicメモ[ i ] );
                }

                foreach( var kvp in dic昇順メモ )
                {
                    int 小節番号 = kvp.Key;

                    // 改行コードは、２文字のリテラル "\n" に置換。
                    string メモ = kvp.Value.Replace( Environment.NewLine, @"\n" );

                    sw.WriteLine( $"PartMemo={小節番号},{メモ}" );
                }

                sw.WriteLine( "" );
                //-----------------
                #endregion

                sw.Close();
            }
        }

        /// <summary>
        ///		指定された Config.Speed を考慮し、指定された時間[ms]の間に流れるピクセル数[dpx]を算出して返す。</para>
        /// </summary>
        [Obsolete( "指定時間がミリ秒単位ではなく秒単位であるメソッドを使用してください。" )]
        public int 指定された時間msに対応する符号付きピクセル数を返す( double speed, long 指定時間ms )
        {
            return (int) ( 指定時間ms * スコア.基準譜面速度dpxms * speed );
        }

        /// <summary>
        ///		指定された Config.Speed を考慮し、指定された時間[秒]の間に流れるピクセル数[dpx]を算出して返す。
        /// </summary>
        public double 指定された時間secに対応する符号付きピクセル数を返す( double speed, double 指定時間sec )
        {
            return ( 指定時間sec * スコア.基準譜面速度dpxsec * speed );
        }

        public double 小節長倍率を取得する( int 小節番号 )
        {
            // 小節長倍率リスト が短ければ増設する。
            if( 小節番号 >= this.小節長倍率リスト.Count )
            {
                int 不足数 = 小節番号 - this.小節長倍率リスト.Count + 1;
                for( int i = 0; i < 不足数; i++ )
                    this.小節長倍率リスト.Add( 1.0 );
            }

            // 小節番号に対応する倍率を返す。
            return this.小節長倍率リスト[ 小節番号 ];
        }

        public void 小節長倍率を設定する( int 小節番号, double 倍率 )
        {
            // 小節長倍率リスト が短ければ増設する。
            if( 小節番号 >= this.小節長倍率リスト.Count )
            {
                int 不足数 = 小節番号 - this.小節長倍率リスト.Count + 1;
                for( int i = 0; i < 不足数; i++ )
                    this.小節長倍率リスト.Add( 1.0 );
            }

            // 小節番号に対応付けて倍率を登録する。
            this.小節長倍率リスト[ 小節番号 ] = 倍率;
        }


        /// <summary>
        ///		空文字、または絶対パス。
        ///		null は不可。
        /// </summary>
        private string _PATH_WAV = "";

        private static readonly Dictionary<レーン種別, List<チップ種別>> _dicSSTFレーンチップ対応表 = new Dictionary<レーン種別, List<チップ種別>>() {
            { レーン種別.Unknown, new List<チップ種別>() { チップ種別.Unknown } },
            { レーン種別.LeftCrash, new List<チップ種別>() { チップ種別.LeftCrash, チップ種別.LeftCymbal_Mute } },
            { レーン種別.Ride, new List<チップ種別>() { チップ種別.Ride, チップ種別.Ride_Cup } },
            { レーン種別.China, new List<チップ種別>() { チップ種別.China } },
            { レーン種別.Splash, new List<チップ種別>() { チップ種別.Splash } },
            { レーン種別.HiHat, new List<チップ種別>() { チップ種別.HiHat_Close, チップ種別.HiHat_HalfOpen, チップ種別.HiHat_Open } },
            { レーン種別.Foot, new List<チップ種別>() { チップ種別.HiHat_Foot } },
            { レーン種別.Snare, new List<チップ種別>() { チップ種別.Snare, チップ種別.Snare_ClosedRim, チップ種別.Snare_Ghost, チップ種別.Snare_OpenRim } },
            { レーン種別.Bass, new List<チップ種別>() { チップ種別.Bass } },
            { レーン種別.Tom1, new List<チップ種別>() { チップ種別.Tom1, チップ種別.Tom1_Rim } },
            { レーン種別.Tom2, new List<チップ種別>() { チップ種別.Tom2, チップ種別.Tom2_Rim } },
            { レーン種別.Tom3, new List<チップ種別>() { チップ種別.Tom3, チップ種別.Tom3_Rim } },
            { レーン種別.RightCrash, new List<チップ種別>() { チップ種別.RightCrash, チップ種別.RightCymbal_Mute } },
            { レーン種別.BPM, new List<チップ種別>() { チップ種別.BPM } },
            { レーン種別.Song, new List<チップ種別>() { チップ種別.背景動画, チップ種別.BGM, チップ種別.SE1, チップ種別.SE2, チップ種別.SE3, チップ種別.SE4, チップ種別.SE5, チップ種別.GuitarAuto, チップ種別.BassAuto } },
        };

        private const double _BPM初期値固定での1小節4拍の時間ms = ( 60.0 * 1000 ) / ( スコア.初期BPM / 4.0 );
        private const double _BPM初期値固定での1小節4拍の時間sec = 60.0 / ( スコア.初期BPM / 4.0 );

        private void _初期化する()
        {
            this.SSTFバージョン = SSTFVERSION;
            this.曲名 = "(no title)";
            this.説明文 = "";
            this.サウンドデバイス遅延ms = 0f;
            this.難易度 = 5.0f;
            this.背景動画ファイル名 = "";
            this.プレビュー画像 = null;
            this.小節長倍率リスト = new List<double>();
            this.dicメモ = new Dictionary<int, string>();
            this.dicWAV = new Dictionary<int, (string ファイルパス, bool 多重再生する)>();
            this._PATH_WAV = "";
            this.譜面ファイルパス = null;
            this.空打ちチップ = new Dictionary<レーン種別, int>();
            foreach( レーン種別 lane in Enum.GetValues( typeof( レーン種別 ) ) )
                this.空打ちチップ.Add( lane, 0 );
        }

        /// <summary>
        ///		指定された曲データファイルを、現行バージョンのSSTFormatとして読み込む。
        /// </summary>
        private void _v3曲データファイルを読み込む( string 曲データファイル名 )
        {
            this._初期化する();

            #region " 背景動画ファイル名を更新する。"
            //----------------
            this.背景動画ファイル名 =
                ( from file in Directory.GetFiles( Path.GetDirectoryName( 曲データファイル名 ) )
                  where スコア.背景動画のデフォルト拡張子リスト.Any( 拡張子名 => ( Path.GetExtension( file ).ToLower() == 拡張子名 ) )
                  select file ).FirstOrDefault();
            //----------------
            #endregion

            using( var sr = new StreamReader( 曲データファイル名, Encoding.UTF8 ) )
            {
                var 現在の = new 解析用作業変数();

                // 1行ずつ読み込む。
                while( false == sr.EndOfStream )
                {
                    string 行 = this._行を読み込む( sr );
                    現在の.行番号++;

                    if( string.IsNullOrEmpty( 行 ) )
                        continue;

                    if( this._行をヘッダ行と想定して解析する( 行, 現在の ) )
                        continue;

                    if( this._行を小節メモ行として解析する( 行, 現在の ) )
                        continue;

                    if( this._行をチップ記述行として解析する( 行, 現在の ) )
                        continue;
                }
            }
        }

        /// <summary>
        ///		指定されたスコアから、現行バージョンに変換する。
        /// </summary>
        /// <param name="v2score">読み込み済みのスコア</param>
        private void _v2スコアからマイグレーションする( SSTFormat.v2.スコア v2score )
        {
            this._v2スコアからマイグレーションする_ヘッダだけ( v2score );

            this.チップリスト = new List<チップ>();
            foreach( var v2chip in v2score.チップリスト )
                this.チップリスト.Add( new チップ( v2chip ) );

            this.小節長倍率リスト = new List<double>();
            foreach( var v2scale in v2score.小節長倍率リスト )
                this.小節長倍率リスト.Add( v2scale );

            this.dicメモ = new Dictionary<int, string>();
            foreach( var kvp in v2score.dicメモ )
                this.dicメモ.Add( kvp.Key, kvp.Value );
        }

        private void _v3曲データファイルを読み込む_ヘッダだけ( string 曲データファイル名 )
        {
            this._初期化する();

            #region " 背景動画ファイル名を更新する。"
            //----------------
            this.背景動画ファイル名 =
                ( from file in Directory.GetFiles( Path.GetDirectoryName( 曲データファイル名 ) )
                  where スコア.背景動画のデフォルト拡張子リスト.Any( 拡張子名 => ( Path.GetExtension( file ).ToLower() == 拡張子名 ) )
                  select file ).FirstOrDefault();
            //----------------
            #endregion

            using( var sr = new StreamReader( 曲データファイル名, Encoding.UTF8 ) )
            {
                var 現在の = new 解析用作業変数();

                // 1行ずつ読み込む。
                while( false == sr.EndOfStream )
                {
                    string 行 = this._行を読み込む( sr );
                    現在の.行番号++;

                    if( string.IsNullOrEmpty( 行 ) )
                        continue;

                    if( this._行をヘッダ行と想定して解析する( 行, 現在の ) )
                        continue;
                }
            }
        }

        private void _v2スコアからマイグレーションする_ヘッダだけ( SSTFormat.v2.スコア v2score )
        {
            this._初期化する();

            this.SSTFバージョン = SSTFVERSION;
            this.曲名 = v2score.Header.曲名;
            this.説明文 = v2score.Header.説明文;
            this.サウンドデバイス遅延ms = v2score.Header.サウンドデバイス遅延ms;
            this.難易度 = 5.0f;    // 新規
            this.背景動画ファイル名 = v2score.背景動画ファイル名;
        }

        private class 解析用作業変数
        {
            public int 行番号 = 0;
            public int 小節番号 = 0;
            public int 小節解像度 = 384;
            public チップ種別 チップ種別 = チップ種別.Unknown;
        }

        /// <returns>
        ///		行をヘッダとして処理したなら true 、該当しないまたはエラーが発生したときは false を返す。
        ///	</returns>
        private bool _行をヘッダ行と想定して解析する( string 行, 解析用作業変数 現在の )
        {
            if( 行.ToLower().StartsWith( "title" ) )
            {
                #region " Title コマンド "
                //-----------------
                string[] items = 行.Split( '=' );

                if( 2 != items.Length )
                {
                    Trace.TraceError( $"Title の書式が不正です。スキップします。[{現在の.行番号}行目]" );
                    return false;
                }

                this.曲名 = items[ 1 ].Trim();

                return true;
                //-----------------
                #endregion
            }
            if( 行.ToLower().StartsWith( "artist" ) )
            {
                #region " Artist コマンド "
                //-----------------
                string[] items = 行.Split( '=' );

                if( 2 != items.Length )
                {
                    Trace.TraceError( $"Artist の書式が不正です。スキップします。[{現在の.行番号}行目]" );
                    return false;
                }

                this.アーティスト名 = items[ 1 ].Trim();

                return true;
                //-----------------
                #endregion
            }
            if( 行.ToLower().StartsWith( "description" ) )
            {
                #region " Description コマンド "
                //-----------------
                string[] items = 行.Split( '=' );

                if( items.Length != 2 )
                {
                    Trace.TraceError( $"Description の書式が不正です。スキップします。[{現在の.行番号}行目]" );
                    return false;
                }

                // ２文字のリテラル "\n" は改行に復号。
                this.説明文 = items[ 1 ].Trim().Replace( @"\n", Environment.NewLine );

                return true;
                //-----------------
                #endregion
            }
            if( 行.ToLower().StartsWith( "sounddevice.delay" ) )
            {
                #region " SoundDevice.Delay コマンド "
                //-----------------
                string[] items = 行.Split( '=' );

                if( 2 != items.Length )
                {
                    Trace.TraceError( $"SoundDevice.Delay の書式が不正です。スキップします。[{現在の.行番号}行目]" );
                    return false;
                }

                // ２文字のリテラル "\n" は改行に復号。
                if( float.TryParse( items[ 1 ].Trim().Replace( @"\n", Environment.NewLine ), out float value ) )
                    this.サウンドデバイス遅延ms = value;

                return true;
                //-----------------
                #endregion
            }
            if( 行.ToLower().StartsWith( "level" ) )
            {
                #region " Level コマンド "
                //----------------
                string[] items = 行.Split( '=' );

                if( 2 != items.Length )
                {
                    Trace.TraceError( $"Level の書式が不正です。スキップします。[{現在の.行番号}行目]" );
                    return false;
                }

                try
                {
                    this.難易度 = Math.Max( Math.Min( float.Parse( items[ 1 ].Trim() ), 9.99f ), 0.00f );
                }
                catch
                {
                    Trace.TraceError( $"Level の右辺が不正です。スキップします。[{現在の.行番号}行目]" );
                    return false;
                }

                return true;
                //----------------
                #endregion
            }
            if( 行.ToLower().StartsWith( "video" ) )
            {
                #region " Video コマンド "
                //----------------
                string[] items = 行.Split( '=' );

                if( 2 != items.Length )
                {
                    Trace.TraceError( $"Video の書式が不正です。スキップします。[{現在の.行番号}行目]" );
                    return false;
                }

                var videoId = items[ 1 ].Trim();

                {   // 書式確認
                    items = videoId.Split( ':' );
                    if( 2 != items.Length )
                    {
                        Trace.TraceError( $"Video の動画IDの書式が不正です。スキップします。[{現在の.行番号}行目]" );
                        return false;
                    }
                }

                this.動画ID = videoId;

                return true;
                //----------------
                #endregion
            }

            return false;
        }

        /// <returns>
        ///		行を小節メモ行として処理したなら true 、該当しないまたはエラーが発生したときは false を返す。
        ///	</returns>
        private bool _行を小節メモ行として解析する( string 行, 解析用作業変数 現在の )
        {
            if( !( 行.ToLower().StartsWith( "partmemo" ) ) )
                return false;

            #region " '=' 以前を除去する。"
            //-----------------
            int 等号位置 = 行.IndexOf( '=' );

            if( 0 >= 等号位置 )
            {
                Trace.TraceError( $"PartMemo の書式が不正です。スキップします。[{現在の.行番号}]行目]" );
                return false;
            }
            行 = 行.Substring( 等号位置 + 1 ).Trim();
            if( string.IsNullOrEmpty( 行 ) )
            {
                Trace.TraceError( $"PartMemo の書式が不正です。スキップします。[{現在の.行番号}]行目]" );
                return false;
            }
            //-----------------
            #endregion
            #region " カンマ位置を取得する。"
            //-----------------
            int カンマ位置 = 行.IndexOf( ',' );

            if( 0 >= カンマ位置 )
            {
                Trace.TraceError( $"PartMemo の書式が不正です。スキップします。[{現在の.行番号}]行目]" );
                return false;
            }
            //-----------------
            #endregion
            #region " 小節番号を取得する。"
            //-----------------
            string 小説番号文字列 = 行.Substring( 0, カンマ位置 );

            if( !( int.TryParse( 小説番号文字列, out int 小節番号 ) || ( 0 > 小節番号 ) ) )
            {
                Trace.TraceError( $"PartMemo の小節番号が不正です。スキップします。[{現在の.行番号}]行目]" );
                return false;
            }
            //-----------------
            #endregion
            #region " メモを取得する。"
            //-----------------
            string メモ = 行.Substring( カンマ位置 + 1 );

            // ２文字のリテラル文字列 "\n" は改行に復号。
            メモ = メモ.Replace( @"\n", Environment.NewLine );
            //-----------------
            #endregion
            #region " メモが空文字列でないなら dicメモ に登録すると同時に、チップとしても追加する。"
            //-----------------
            if( !( string.IsNullOrEmpty( メモ ) ) )
            {
                this.dicメモ.Add( 小節番号, メモ );

                this.チップリスト.Add(
                    new チップ() {
                        チップ種別 = チップ種別.小節メモ,
                        小節番号 = 小節番号,
                        小節内位置 = 0,
                        小節解像度 = 1,
                    } );
            }
            //-----------------
            #endregion

            return true;
        }

        /// <returns>
        ///		常に true を返す。
        ///	</returns>
        private bool _行をチップ記述行として解析する( string 行, 解析用作業変数 現在の )
        {
            // 行を区切り文字でトークンに分割。
            string[] tokens = 行.Split( new char[] { ';', ':' } );

            // すべてのトークンについて……
            foreach( string token in tokens )
            {
                #region " トークンを区切り文字 '=' で コマンド と パラメータ に分割し、それぞれの先頭末尾の空白を削除する。"
                //-----------------
                string[] items = token.Split( '=' );

                if( 2 != items.Length )
                {
                    if( 0 == token.Trim().Length )  // 空文字列（行末など）は不正じゃない。
                        continue;

                    Trace.TraceError( $"コマンドとパラメータの記述書式が不正です。このコマンドをスキップします。[{現在の.行番号}行目]" );
                    continue;
                }

                string コマンド = items[ 0 ].Trim();
                string パラメータ = items[ 1 ].Trim();
                //-----------------
                #endregion

                switch( コマンド.ToLower() )
                {
                    case "part":
                        #region " Part（小節番号指定）コマンド "
                        //-----------------
                        {
                            #region " 小節番号を取得・設定。"
                            //----------------
                            string 小節番号文字列 = this._指定された文字列の先頭から数字文字列を取り出す( ref パラメータ );
                            if( string.IsNullOrEmpty( 小節番号文字列 ) )
                            {
                                Trace.TraceError( $"Part（小節番号）コマンドに小節番号の記述がありません。このコマンドをスキップします。[{現在の.行番号}行目]" );
                                continue;
                            }
                            if( !( int.TryParse( 小節番号文字列, out int 小節番号 ) ) )
                            {
                                Trace.TraceError( $"Part（小節番号）コマンドの小節番号が不正です。このコマンドをスキップします。[{現在の.行番号}行目]" );
                                continue;
                            }
                            if( 0 > 小節番号 )
                            {
                                Trace.TraceError( $"Part（小節番号）コマンドの小節番号が負数です。このコマンドをスキップします。[{現在の.行番号}行目]" );
                                continue;
                            }
                            現在の.小節番号 = 小節番号;
                            //----------------
                            #endregion

                            // Part の属性があれば取得する。

                            while( 0 < パラメータ.Length )
                            {
                                char 属性ID = char.ToLower( パラメータ[ 0 ] );

                                if( 属性ID == 's' )
                                {
                                    #region " 小節長倍率(>0) → list小節長倍率 "
                                    //-----------------
                                    パラメータ = パラメータ.Substring( 1 ).Trim();

                                    string 小節長倍率文字列 = this._指定された文字列の先頭から数字文字列を取り出す( ref パラメータ );
                                    if( string.IsNullOrEmpty( 小節長倍率文字列 ) )
                                    {
                                        Trace.TraceError( $"Part（小節番号）コマンドに小節長倍率の記述がありません。この属性をスキップします。[{現在の.行番号}行目]" );
                                        continue;
                                    }
                                    パラメータ = パラメータ.Trim();

                                    if( !( double.TryParse( 小節長倍率文字列, out double 小節長倍率 ) ) )
                                    {
                                        Trace.TraceError( $"Part（小節番号）コマンドの小節長倍率が不正です。この属性をスキップします。[{現在の.行番号}行目]" );
                                        continue;
                                    }
                                    if( 0.0 >= 小節長倍率 )
                                    {
                                        Trace.TraceError( $"Part（小節番号）コマンドの小節長倍率が 0.0 または負数です。この属性をスキップします。[{現在の.行番号}行目]" );
                                        continue;
                                    }
                                    // 小節長倍率辞書に追加 or 上書き更新。
                                    this.小節長倍率を設定する( 現在の.小節番号, 小節長倍率 );

                                    continue;
                                    //-----------------
                                    #endregion
                                }
                            }
                        }
                        //-----------------
                        #endregion
                        break;

                    case "lane":
                        #region " Lane（レーン指定）コマンド（チップ種別の仮決め）"
                        //-----------------
                        {
                            switch( パラメータ.ToLower() )
                            {
                                case "leftcrash": 現在の.チップ種別 = チップ種別.LeftCrash; break;
                                case "ride": 現在の.チップ種別 = チップ種別.Ride; break;
                                case "china": 現在の.チップ種別 = チップ種別.China; break;
                                case "splash": 現在の.チップ種別 = チップ種別.Splash; break;
                                case "hihat": 現在の.チップ種別 = チップ種別.HiHat_Close; break;
                                case "snare": 現在の.チップ種別 = チップ種別.Snare; break;
                                case "bass": 現在の.チップ種別 = チップ種別.Bass; break;
                                case "tom1": 現在の.チップ種別 = チップ種別.Tom1; break;
                                case "tom2": 現在の.チップ種別 = チップ種別.Tom2; break;
                                case "tom3": 現在の.チップ種別 = チップ種別.Tom3; break;
                                case "rightcrash": 現在の.チップ種別 = チップ種別.RightCrash; break;
                                case "bpm": 現在の.チップ種別 = チップ種別.BPM; break;
                                case "song": 現在の.チップ種別 = チップ種別.背景動画; break;
                                default:
                                    Trace.TraceError( $"Lane（レーン指定）コマンドのパラメータ記述 '{パラメータ}' が不正です。このコマンドをスキップします。[{現在の.行番号}行目]" );
                                    break;
                            }
                        }
                        //-----------------
                        #endregion
                        break;

                    case "resolution":
                        #region " Resolution（小節解像度指定）コマンド "
                        //-----------------
                        {
                            if( !( int.TryParse( パラメータ, out int 解像度 ) ) )
                            {
                                Trace.TraceError( $"Resolution（小節解像度指定）コマンドの解像度が不正です。このコマンドをスキップします。[{現在の.行番号}行目]" );
                                continue;
                            }
                            if( 1 > 解像度 )
                            {
                                Trace.TraceError( $"Resolution（小節解像度指定）コマンドの解像度は 1 以上でなければなりません。このコマンドをスキップします。[{現在の.行番号}行目]" );
                                continue;
                            }

                            現在の.小節解像度 = 解像度;
                        }
                        //-----------------
                        #endregion
                        break;

                    case "chips":
                        #region " Chips（チップ指定）コマンド "
                        //-----------------
                        // パラメータを区切り文字 ',' でチップトークンに分割。
                        string[] chipTokens = パラメータ.Split( ',' );

                        // すべてのチップトークンについて……
                        for( int i = 0; i < chipTokens.Length; i++ )
                        {
                            chipTokens[ i ].Trim();

                            if( 0 == chipTokens[ i ].Length )
                                continue;

                            #region " チップを生成する。"
                            //-----------------
                            var chip = new チップ() {
                                小節番号 = 現在の.小節番号,
                                チップ種別 = 現在の.チップ種別,
                                小節解像度 = 現在の.小節解像度,
                                音量 = チップ.最大音量,
                            };
                            //-----------------
                            #endregion
                            #region " チップ位置を取得する。"
                            //-----------------
                            {
                                string 位置番号文字列 = this._指定された文字列の先頭から数字文字列を取り出す( ref chipTokens[ i ] );
                                chipTokens[ i ].Trim();

                                // 文法チェック。
                                if( string.IsNullOrEmpty( 位置番号文字列 ) )
                                {
                                    Trace.TraceError( $"チップの位置指定の記述がありません。このチップをスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                    continue;
                                }

                                // 位置を取得。
                                if( false == int.TryParse( 位置番号文字列, out int チップ位置 ) )
                                {
                                    Trace.TraceError( $"チップの位置指定の記述が不正です。このチップをスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                    continue;
                                }

                                // 値域チェック。
                                if( ( 0 > チップ位置 ) || ( チップ位置 >= 現在の.小節解像度 ) )
                                {
                                    Trace.TraceError( $"チップの位置が負数であるか解像度(Resolution)以上の値になっています。このチップをスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                    continue;
                                }

                                chip.小節内位置 = チップ位置;
                            }
                            //-----------------
                            #endregion
                            #region " 共通属性・レーン別属性があれば取得する。"
                            //-----------------
                            while( 0 < chipTokens[ i ].Length )
                            {
                                var 属性ID = char.ToLower( chipTokens[ i ][ 0 ] );

                                // 共通属性

                                if( 'v' == 属性ID )
                                {
                                    #region " 音量 "
                                    //----------------
                                    chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                    string 音量文字列 = this._指定された文字列の先頭から数字文字列を取り出す( ref chipTokens[ i ] );
                                    chipTokens[ i ].Trim();

                                    // 文法チェック。
                                    if( string.IsNullOrEmpty( 音量文字列 ) )
                                    {
                                        Trace.TraceError( $"チップの音量指定の記述がありません。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                        continue;
                                    }

                                    // チップ音量の取得。
                                    if( !( int.TryParse( 音量文字列, out int チップ音量 ) ) )
                                    {
                                        Trace.TraceError( $"チップの音量指定の記述が不正です。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                        continue;
                                    }

                                    // 値域チェック。
                                    if( ( 1 > チップ音量 ) || ( チップ音量 > チップ.最大音量 ) )
                                    {
                                        Trace.TraceError( $"チップの音量が適正範囲（1～{チップ.最大音量}）を超えています。このチップをスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                        continue;
                                    }

                                    chip.音量 = チップ音量;
                                    continue;
                                    //----------------
                                    #endregion
                                }

                                // レーン別属性

                                switch( 現在の.チップ種別 )
                                {
                                    case チップ種別.LeftCrash:
                                        if( 'm' == 属性ID )
                                        {
                                            #region " ミュート "
                                            //----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            chip.チップ種別 = チップ種別.LeftCymbal_Mute;
                                            //----------------
                                            #endregion
                                        }
                                        else
                                        {
                                            #region " 未知の属性 "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            Trace.TraceError( $"未対応の属性「{属性ID}」が指定されています。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                            //-----------------
                                            #endregion
                                        }
                                        continue;

                                    case チップ種別.Ride:
                                    case チップ種別.Ride_Cup:
                                        if( 'c' == 属性ID )
                                        {
                                            #region " Ride.カップ "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            chip.チップ種別 = チップ種別.Ride_Cup;
                                            //-----------------
                                            #endregion
                                        }
                                        else
                                        {
                                            #region " 未知の属性 "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            Trace.TraceError( $"未対応の属性「{属性ID}」が指定されています。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                            //-----------------
                                            #endregion
                                        }
                                        continue;

                                    case チップ種別.China:
                                        {
                                            #region " 未知の属性 "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            Trace.TraceError( $"未対応の属性「{属性ID}」が指定されています。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                            //-----------------
                                            #endregion
                                        }
                                        continue;

                                    case チップ種別.Splash:
                                        {
                                            #region " 未知の属性 "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            Trace.TraceError( $"未対応の属性「{属性ID}」が指定されています。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                            //-----------------
                                            #endregion
                                        }
                                        continue;

                                    case チップ種別.HiHat_Close:
                                    case チップ種別.HiHat_HalfOpen:
                                    case チップ種別.HiHat_Open:
                                    case チップ種別.HiHat_Foot:
                                        if( 'o' == 属性ID )
                                        {
                                            #region " HiHat.オープン "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            chip.チップ種別 = チップ種別.HiHat_Open;
                                            //-----------------
                                            #endregion
                                        }
                                        else if( 'h' == 属性ID )
                                        {
                                            #region " HiHat.ハーフオープン "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            chip.チップ種別 = チップ種別.HiHat_HalfOpen;
                                            //-----------------
                                            #endregion
                                        }
                                        else if( 'c' == 属性ID )
                                        {
                                            #region " HiHat.クローズ "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            chip.チップ種別 = チップ種別.HiHat_Close;
                                            //-----------------
                                            #endregion
                                        }
                                        else if( 'f' == 属性ID )
                                        {
                                            #region " HiHat.フットスプラッシュ "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            chip.チップ種別 = チップ種別.HiHat_Foot;
                                            //-----------------
                                            #endregion
                                        }
                                        else
                                        {
                                            #region " 未知の属性 "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            Trace.TraceError( $"未対応の属性「{属性ID}」が指定されています。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                            //-----------------
                                            #endregion
                                        }
                                        continue;

                                    case チップ種別.Snare:
                                    case チップ種別.Snare_ClosedRim:
                                    case チップ種別.Snare_OpenRim:
                                    case チップ種別.Snare_Ghost:
                                        if( 'o' == 属性ID )
                                        {
                                            #region " Snare.オープンリム "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            chip.チップ種別 = チップ種別.Snare_OpenRim;
                                            //-----------------
                                            #endregion
                                        }
                                        else if( 'c' == 属性ID )
                                        {
                                            #region " Snare.クローズドリム "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            chip.チップ種別 = チップ種別.Snare_ClosedRim;
                                            //-----------------
                                            #endregion
                                        }
                                        else if( 'g' == 属性ID )
                                        {
                                            #region " Snare.ゴースト "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            chip.チップ種別 = チップ種別.Snare_Ghost;
                                            //-----------------
                                            #endregion
                                        }
                                        else
                                        {
                                            #region " 未知の属性 "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            Trace.TraceError( $"未対応の属性「{属性ID}」が指定されています。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                            //-----------------
                                            #endregion
                                        }
                                        continue;

                                    case チップ種別.Bass:
                                        {
                                            #region " 未知の属性 "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            Trace.TraceError( $"未対応の属性「{属性ID}」が指定されています。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                            //-----------------
                                            #endregion
                                        }
                                        continue;

                                    case チップ種別.Tom1:
                                    case チップ種別.Tom1_Rim:
                                        if( 'r' == 属性ID )
                                        {
                                            #region " Tom1.リム "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            chip.チップ種別 = チップ種別.Tom1_Rim;
                                            //-----------------
                                            #endregion
                                        }
                                        else
                                        {
                                            #region " 未知の属性 "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            Trace.TraceError( $"未対応の属性「{属性ID}」が指定されています。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                            //-----------------
                                            #endregion
                                        }
                                        continue;

                                    case チップ種別.Tom2:
                                    case チップ種別.Tom2_Rim:
                                        if( 'r' == 属性ID )
                                        {
                                            #region " Tom2.リム "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            chip.チップ種別 = チップ種別.Tom2_Rim;
                                            //-----------------
                                            #endregion
                                        }
                                        else
                                        {
                                            #region " 未知の属性 "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            Trace.TraceError( $"未対応の属性「{属性ID}」が指定されています。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                            //-----------------
                                            #endregion
                                        }
                                        continue;

                                    case チップ種別.Tom3:
                                    case チップ種別.Tom3_Rim:
                                        if( 'r' == 属性ID )
                                        {
                                            #region " Tom3.リム "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            chip.チップ種別 = チップ種別.Tom3_Rim;
                                            //-----------------
                                            #endregion
                                        }
                                        else
                                        {
                                            #region " 未知の属性 "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            Trace.TraceError( $"未対応の属性「{属性ID}」が指定されています。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                            //-----------------
                                            #endregion
                                        }
                                        continue;

                                    case チップ種別.RightCrash:
                                        if( 'm' == 属性ID )
                                        {
                                            #region " ミュート "
                                            //----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            chip.チップ種別 = チップ種別.RightCymbal_Mute;
                                            //----------------
                                            #endregion
                                        }
                                        else
                                        {
                                            #region " 未知の属性 "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            Trace.TraceError( $"未対応の属性「{属性ID}」が指定されています。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                            //-----------------
                                            #endregion
                                        }
                                        continue;

                                    case チップ種別.BPM:
                                        if( 'b' == 属性ID )
                                        {
                                            #region " BPM値 "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();

                                            string BPM文字列 = this._指定された文字列の先頭から数字文字列を取り出す( ref chipTokens[ i ] );
                                            chipTokens[ i ].Trim();

                                            if( string.IsNullOrEmpty( BPM文字列 ) )
                                            {
                                                Trace.TraceError( $"BPM数値の記述がありません。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                                continue;
                                            }

                                            if( false == double.TryParse( BPM文字列, out double BPM ) || ( 0.0 >= BPM ) )
                                            {
                                                Trace.TraceError( $"BPM数値の記述が不正です。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                                continue;
                                            }

                                            chip.BPM = BPM;
                                            //-----------------
                                            #endregion
                                        }
                                        else
                                        {
                                            #region " 未知の属性 "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            Trace.TraceError( $"未対応の属性「{属性ID}」が指定されています。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                            //-----------------
                                            #endregion
                                        }
                                        continue;

                                    case チップ種別.背景動画:
                                        {
                                            #region " 未知の属性 "
                                            //-----------------
                                            chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                            Trace.TraceError( $"未対応の属性「{属性ID}」が指定されています。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                            //-----------------
                                            #endregion
                                        }
                                        continue;
                                }

                                #region " 未知の属性 "
                                //-----------------
                                chipTokens[ i ] = chipTokens[ i ].Substring( 1 ).Trim();
                                Trace.TraceError( $"未対応の属性「{属性ID}」が指定されています。この属性をスキップします。[{現在の.行番号}行目; {i + 1}個目のチップ]" );
                                //-----------------
                                #endregion
                            }
                            //-----------------
                            #endregion

                            this.チップリスト.Add( chip );
                        }
                        //-----------------
                        #endregion
                        break;
                }
            }

            return true;
        }

        /// <summary>
        ///		取出文字列の先頭にある数字（小数点も有効）の連続した部分を取り出して、戻り値として返す。
        ///		また、取出文字列から取り出した数字文字列部分を除去した文字列を再度格納する。
        /// </summary>
        private string _指定された文字列の先頭から数字文字列を取り出す( ref string 取出文字列 )
        {
            int 桁数 = 0;
            while( ( 桁数 < 取出文字列.Length ) && ( char.IsDigit( 取出文字列[ 桁数 ] ) || 取出文字列[ 桁数 ] == '.' ) )
                桁数++;

            if( 0 == 桁数 )
                return "";

            string 数字文字列 = 取出文字列.Substring( 0, 桁数 );
            取出文字列 = ( 桁数 == 取出文字列.Length ) ? "" : 取出文字列.Substring( 桁数 );

            return 数字文字列;
        }

        /// <summary>
        ///		Stream から１行読み込み、コメントや改行等の前処理を行ってから返す。
        /// </summary>
        /// <param name="reader">
        ///		行の読み込み元。読み込んだ分、ポジションは進められる。
        /// </param>
        /// <returns>
        ///		読み込んで、コメントや改行等の前処理を適用したあとの行を返す。
        ///		reader が EOF だった場合には null を返す。
        /// </returns>
        private string _行を読み込む( StreamReader reader )
        {
            if( reader.EndOfStream )
                return null;

            // 1行読み込む。
            string 行 = reader.ReadLine();

            // (1) 改行とTABを空白文字に変換し、先頭末尾の空白を削除する。
            行 = 行.Replace( Environment.NewLine, " " );
            行 = 行.Replace( '\t', ' ' );
            行 = 行.Trim();

            // (2) 行中の '#' 以降はコメントとして除外する。
            int 区切り位置 = 行.IndexOf( '#' );
            if( 0 <= 区切り位置 )
            {
                行 = 行.Substring( 0, 区切り位置 );
                行 = 行.Trim();
            }

            return 行;
        }

        private int _最大公約数を返す( int m, int n )
        {
            if( ( 0 >= m ) || ( 0 >= n ) )
                throw new Exception( "引数に0以下の数は指定できません。" );

            // ユーグリッドの互除法
            int r;
            while( ( r = m % n ) != 0 )
            {
                m = n;
                n = r;
            }

            return n;
        }

        private int _最小公倍数を返す( int m, int n )
        {
            if( ( 0 >= m ) || ( 0 >= n ) )
                throw new Exception( "引数に0以下の数は指定できません。" );

            return ( m * n / this._最大公約数を返す( m, n ) );
        }
    }
}
