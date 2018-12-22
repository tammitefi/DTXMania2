using Microsoft.VisualStudio.TestTools.UnitTesting;
using SSTFormat.v4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SSTFormat.v4.Tests
{
    [TestClass()]
    public class DTXTests
    {
        // 例外検証用メソッド
        private void 例外が出れば成功( Action action )
        {
            try
            {
                action();
                Assert.Fail();  // ここに来るということは、action() で例外がでなかったということ。
            }
            catch( AssertFailedException )
            {
                throw;  // 失敗。
            }
            catch
            {
                // 成功。
            }
        }


        [TestMethod()]
        public void DTX仕様の実数を取得するTest()
        {
            // DTX仕様の実数の定義（カルチャ非依存）
            // 　小数点文字s = ".,";         // '.' の他に ',' も使える。
            // 　桁区切り文字s = ".,' ";     // 桁区切り文字が小数点文字と被ってるが、一番最後に現れたものだけが小数点として認識される。

            double num = 0f;

            // 整数（小数点なし）→ OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( "123", out num ) );
            Assert.AreEqual( 123, num );


            // 小数点(.)あり → OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( "123.4", out num ) );
            Assert.AreEqual( 123.4, num );

            // 小数点(,)あり → OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( "123,4", out num ) );
            Assert.AreEqual( 123.4, num );


            // 桁区切りに見える小数点(,) → OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( "1,234", out num ) );
            Assert.AreEqual( 1.234, num ); // 1234 ではない。


            // 整数部なしの小数(.) → OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( ".1234", out num ) );
            Assert.AreEqual( 0.1234, num );

            // 整数部なしの小数(,) → OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( ",1234", out num ) );
            Assert.AreEqual( 0.1234, num );


            // 小数部なしの小数(.) → OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( "1234.", out num ) );
            Assert.AreEqual( 1234, num );

            // 小数部なしの小数(,) → OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( "1234,", out num ) );
            Assert.AreEqual( 1234, num );


            // 整数部に桁区切り(,)あり、小数点あり(.) → OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( "12,345.6", out num ) );
            Assert.AreEqual( 12345.6, num );

            // 整数部に桁区切り(,)あり、小数点あり(,) → OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( "12,345,6", out num ) );
            Assert.AreEqual( 12345.6, num );   // 123456 ではない。

            // 整数部に桁区切り(.)あり、小数点あり(,) → OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( "12.345,6", out num ) );
            Assert.AreEqual( 12345.6, num );   // 12.3456 ではない。


            // 小数点(.)の連続 → OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( "12...345", out num ) );
            Assert.AreEqual( 12.345, num );   // エラーではない。

            // 小数点(,)の連続 → OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( "123,,,45", out num ) );
            Assert.AreEqual( 123.45, num );   // エラーではない。

            // 小数点(.)の連続 → OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( "12...34..5", out num ) );
            Assert.AreEqual( 1234.5, num );   // エラーではない。12.345 でもない。

            // 小数点(,)の連続 → OK
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( "12,,,34,,5", out num ) );
            Assert.AreEqual( 1234.5, num );   // エラーではない。12.345 でもない。

            // 小数点(.,)の混在 → OK								   
            Assert.IsTrue( スコア.DTX._DTX仕様の実数を取得する( "12...34,,5", out num ) );
            Assert.AreEqual( 1234.5, num );   // エラーではない。12.345 でもない。
        }

        [TestMethod()]
        public void 小節番号とチャンネル番号を取得するTest()
        {
            int 小節番号, チャンネル番号;

            スコア.DTX.現在の.状態をリセットする();

            // 基本形(DTX)
            スコア.DTX.現在の.データ種別 = スコア.DTX.データ種別.DTX;
            Assert.IsTrue( スコア.DTX._小節番号とチャンネル番号を取得する( @"0FF21", out 小節番号, out チャンネル番号 ) );
            Assert.AreEqual( 0 * 100 + 15 * 10 + 15, 小節番号 );
            Assert.AreEqual( 2 * 16 + 1, チャンネル番号 );

            Assert.IsTrue( スコア.DTX._小節番号とチャンネル番号を取得する( @"ZFFFF", out 小節番号, out チャンネル番号 ) );
            Assert.AreEqual( 35 * 100 + 15 * 10 + 15, 小節番号 );
            Assert.AreEqual( 15 * 16 + 15, チャンネル番号 );

            Assert.IsFalse( スコア.DTX._小節番号とチャンネル番号を取得する( @"001ZZ", out 小節番号, out チャンネル番号 ) );    // チャンネル番号は16進数2桁であること

            // 基本形(GDA)
            スコア.DTX.現在の.データ種別 = スコア.DTX.データ種別.GDA;
            Assert.IsTrue( スコア.DTX._小節番号とチャンネル番号を取得する( @"0FFSD", out 小節番号, out チャンネル番号 ) );
            Assert.AreEqual( 0 * 100 + 15 * 10 + 15, 小節番号 );
            Assert.AreEqual( 0x12, チャンネル番号 );    // 16進数

            Assert.IsTrue( スコア.DTX._小節番号とチャンネル番号を取得する( @"ZFFBD", out 小節番号, out チャンネル番号 ) );
            Assert.AreEqual( 35 * 100 + 15 * 10 + 15, 小節番号 );
            Assert.AreEqual( 0x13, チャンネル番号 );    // 16進数
        }

        [TestMethod()]
        public void 行をコマンドとパラメータとコメントに分解するTest()
        {
            string コマンド, パラメータ, コメント, コマンドzzなし;
            int zz16進数, zz36進数;

            // 基本形; コマンドのみ
            Assert.IsTrue( スコア.DTX._行をコマンドとパラメータとコメントに分解する( @"#command", out コマンド, out コマンドzzなし, out zz16進数, out zz36進数, out パラメータ, out コメント ) );
            Assert.AreEqual( "command", コマンド );
            Assert.AreEqual( -1, zz16進数 );
            Assert.AreEqual( 841, zz36進数 );             // 'nd' は36進数で 23*36+13 = 841 とみなすことができる。
            Assert.AreEqual( "comma", コマンドzzなし );   // 16進数または36進数として zz を取得できたので、'nd' 部分が除去されたコマンドが格納される。
            Assert.AreEqual( "", パラメータ );
            Assert.AreEqual( "", コメント );

            // 基本形; コマンドとパラメータ
            Assert.IsTrue( スコア.DTX._行をコマンドとパラメータとコメントに分解する( @"#command:parameter", out コマンド, out コマンドzzなし, out zz16進数, out zz36進数, out パラメータ, out コメント ) );
            Assert.AreEqual( "command", コマンド );
            Assert.AreEqual( -1, zz16進数 );
            Assert.AreEqual( 841, zz36進数 );             // 'nd' は36進数で 23*36+13 = 841 とみなすことができる。
            Assert.AreEqual( "comma", コマンドzzなし );   // 16進数または36進数として zz を取得できたので、'nd' 部分が除去されたコマンドが格納される。
            Assert.AreEqual( "parameter", パラメータ );
            Assert.AreEqual( "", コメント );

            // 基本形; コマンドとパラメータとコメント
            Assert.IsTrue( スコア.DTX._行をコマンドとパラメータとコメントに分解する( @"#command:parameter;comment", out コマンド, out コマンドzzなし, out zz16進数, out zz36進数, out パラメータ, out コメント ) );
            Assert.AreEqual( "command", コマンド );
            Assert.AreEqual( -1, zz16進数 );
            Assert.AreEqual( 841, zz36進数 );             // 'nd' は36進数で 23*36+13 = 841 とみなすことができる。
            Assert.AreEqual( "comma", コマンドzzなし );   // 16進数または36進数として zz を取得できたので、'nd' 部分が除去されたコマンドが格納される。
            Assert.AreEqual( "parameter", パラメータ );
            Assert.AreEqual( "comment", コメント );

            // 空白チェック
            Assert.IsTrue( スコア.DTX._行をコマンドとパラメータとコメントに分解する( " \t  #   command    :    parameter1  \t  parameter2     ;    comment comment2   ", out コマンド, out コマンドzzなし, out zz16進数, out zz36進数, out パラメータ, out コメント ) );
            Assert.AreEqual( "command", コマンド );
            Assert.AreEqual( -1, zz16進数 );
            Assert.AreEqual( 841, zz36進数 );             // 'nd' は36進数で 23*36+13 = 841 とみなすことができる。
            Assert.AreEqual( "comma", コマンドzzなし );   // 16進数または36進数として zz を取得できたので、'nd' 部分が除去されたコマンドが格納される。
            Assert.AreEqual( "parameter1  \t  parameter2", パラメータ );    // パラメータの両端はトリムされるが、中の空白は（位置、個数ともに）維持される。
            Assert.AreEqual( "comment comment2", コメント );                // コメントについても同上。

            // コマンドとパラメータの間の ':' の省略は OK
            Assert.IsTrue( スコア.DTX._行をコマンドとパラメータとコメントに分解する( @"#command parameter", out コマンド, out コマンドzzなし, out zz16進数, out zz36進数, out パラメータ, out コメント ) );
            Assert.AreEqual( "command", コマンド );
            Assert.AreEqual( -1, zz16進数 );
            Assert.AreEqual( 841, zz36進数 );             // 'nd' は36進数で 23*36+13 = 841 とみなすことができる。
            Assert.AreEqual( "comma", コマンドzzなし );   // 16進数または36進数として zz を取得できたので、'nd' 部分が除去されたコマンドが格納される。
            Assert.AreEqual( "parameter", パラメータ );
            Assert.AreEqual( "", コメント );

            // コマンド末尾のzz番号（16進数2桁）を取得できる。
            Assert.IsTrue( スコア.DTX._行をコマンドとパラメータとコメントに分解する( @"#WAV1F: snare.wav", out コマンド, out コマンドzzなし, out zz16進数, out zz36進数, out パラメータ, out コメント ) );
            Assert.AreEqual( "WAV1F", コマンド );       // zzが含まれる。
            Assert.AreEqual( "WAV", コマンドzzなし );   // zzが含まれない。
            Assert.AreEqual( 16 + 15, zz16進数 ); // 16進数として解析した値が返される。16進数として解析できない場合は -1 。
            Assert.AreEqual( 36 + 15, zz36進数 ); // 36進数として解析した値が返される。36進数として解析できない場合は -1 。
            Assert.AreEqual( "snare.wav", パラメータ );
            Assert.AreEqual( "", コメント );

            // コマンド末尾のzz番号（36進数2桁）を取得できる。
            Assert.IsTrue( スコア.DTX._行をコマンドとパラメータとコメントに分解する( @"#WAV1G: snare.wav", out コマンド, out コマンドzzなし, out zz16進数, out zz36進数, out パラメータ, out コメント ) );
            Assert.AreEqual( "WAV1G", コマンド );       // zzが含まれる。
            Assert.AreEqual( "WAV", コマンドzzなし );   // zzが含まれない。
            Assert.AreEqual( -1, zz16進数 );      // 16進数として解析した値が返される。16進数として解析できない場合は -1 。
            Assert.AreEqual( 36 + 16, zz36進数 ); // 36進数として解析した値が返される。36進数として解析できない場合は -1 。
            Assert.AreEqual( "snare.wav", パラメータ );
            Assert.AreEqual( "", コメント );

            // '#' がない場合、すべてがコメントとみなされる。
            Assert.IsTrue( スコア.DTX._行をコマンドとパラメータとコメントに分解する( @"command : parameter ; comment", out コマンド, out コマンドzzなし, out zz16進数, out zz36進数, out パラメータ, out コメント ) );
            Assert.AreEqual( "", コマンド );
            Assert.AreEqual( -1, zz16進数 );
            Assert.AreEqual( -1, zz36進数 );
            Assert.AreEqual( "", コマンドzzなし );
            Assert.AreEqual( "", パラメータ );
            Assert.AreEqual( "command : parameter ; comment", コメント );

            // どんな文字列でも、このメソッドは false を返さない。（今のところは。）
            //Assert.IsFalse( スコア.DTX._行をコマンドとパラメータとコメントに分解する( @"", out コマンド, out パラメータ, out コメント ) );
        }

        [TestMethod()]
        public void コマンド_TITLE()
        {
            // 基本形
            var score = スコア.DTX.文字列から生成する( @"
#title: てすと１
" );
            Assert.AreEqual( "てすと１", score.曲名 );


            // 複数存在する場合 → 最後のものが有効になる。
            score = スコア.DTX.文字列から生成する( @"
#title: てすと１
#title: てすと２
#title: てすと３
" );
            Assert.AreEqual( "てすと３", score.曲名 );
        }

        [TestMethod()]
        public void コマンド_ARTIST()
        {
            // 基本形
            var score = スコア.DTX.文字列から生成する( @"
#artist: てすと１
" );
            Assert.AreEqual( "てすと１", score.アーティスト名 );


            // 複数存在する場合 → 最後のものが有効になる。
            score = スコア.DTX.文字列から生成する( @"
#artist: てすと１
#artist: てすと２
#artist: てすと３
" );
            Assert.AreEqual( "てすと３", score.アーティスト名 );
        }

        [TestMethod()]
        public void コマンド_COMMENT()
        {
            // 基本形
            var score = スコア.DTX.文字列から生成する( @"
#comment: てすと１
" );
            Assert.AreEqual( "てすと１", score.説明文 );


            // 複数存在する場合 → 最後のものが有効になる。
            score = スコア.DTX.文字列から生成する( @"
#comment: てすと１
#comment: てすと２
#comment: てすと３
" );
            Assert.AreEqual( "てすと３", score.説明文 );
        }

        [TestMethod()]
        public void コマンド_DLEVEL_PLAYLEVEL()
        {
            // 基本形(DLEVEL)
            var score = スコア.DTX.文字列から生成する( @"
#DLEVEL: 10
" );
            Assert.AreEqual( 1.0, score.難易度 );

            // 基本形(PLAYLEVEL)
            score = スコア.DTX.文字列から生成する( @"
#PLAYLEVEL: 99
" );
            Assert.AreEqual( 9.9, score.難易度 );
        }

        [TestMethod()]
        public void コマンド_PREVIEW()
        {
            // 基本形
            var score = スコア.DTX.文字列から生成する( @"
#PREVIEW: pre.wav
" );
            Assert.AreEqual( "pre.wav", score.プレビュー音声ファイル名 );


            // PATH_WAV はまだ反映されない。
            score = スコア.DTX.文字列から生成する( @"
#path_wav: sounds
#PREVIEW: pre.wav
" );
            Assert.AreEqual( "pre.wav", score.プレビュー音声ファイル名 );

            // 複数存在する場合 → 最後のものが有効になる。
            score = スコア.DTX.文字列から生成する( @"
#PREVIEW: pre1.wav
#PREVIEW: pre2.wav
#PREVIEW: pre3.wav
" );
            Assert.AreEqual( "pre3.wav", score.プレビュー音声ファイル名 );
        }

        [TestMethod()]
        public void コマンド_PREIMAGE()
        {
            // 基本形
            var score = スコア.DTX.文字列から生成する( @"
#PREIMAGE: pre.jpg
" );
            Assert.AreEqual( "pre.jpg", score.プレビュー画像ファイル名 );


            // PATH_WAV はまだ反映されない。
            score = スコア.DTX.文字列から生成する( @"
#path_wav: images
#PREIMAGE: pre.jpg
" );
            Assert.AreEqual( "pre.jpg", score.プレビュー画像ファイル名 );

            // 複数存在する場合 → 最後のものが有効になる。
            score = スコア.DTX.文字列から生成する( @"
#PREIMAGE: pre1.jpg
#PREIMAGE: pre2.jpg
#PREIMAGE: pre3.jpg
" );
            Assert.AreEqual( "pre3.jpg", score.プレビュー画像ファイル名 );
        }

        [TestMethod()]
        public void コマンド_PREMOVIE()
        {
            // 基本形
            var score = スコア.DTX.文字列から生成する( @"
#PREMOVIE: pre.mp4
" );
            Assert.AreEqual( "pre.mp4", score.プレビュー動画ファイル名 );


            // PATH_WAV はまだ反映されない。
            score = スコア.DTX.文字列から生成する( @"
#path_wav: images
#PREMOVIE: pre.mp4
" );
            Assert.AreEqual( "pre.mp4", score.プレビュー動画ファイル名 );

            // 複数存在する場合 → 最後のものが有効になる。
            score = スコア.DTX.文字列から生成する( @"
#PREMOVIE: pre1.mp4
#PREMOVIE: pre2.mp4
#PREMOVIE: pre3.mp4
" );
            Assert.AreEqual( "pre3.mp4", score.プレビュー動画ファイル名 );
        }

        [TestMethod()]
        public void コマンド_PATH_WAV()
        {
            // 基本形
            var score = スコア.DTX.文字列から生成する( @"
#PATH_WAV: waves\
" );
            Assert.AreEqual( @"waves\", score.PATH_WAV );

            // 末尾の '\' の有無に関係なく、そのまま格納される。
            score = スコア.DTX.文字列から生成する( @"
#PATH_WAV: waves
" );
            Assert.AreEqual( @"waves", score.PATH_WAV );


            // 複数存在する場合 → 最後のものが有効になる。
            score = スコア.DTX.文字列から生成する( @"
#PATH_WAV: waves1
#PATH_WAV: waves2
#PATH_WAV: waves3
" );
            Assert.AreEqual( @"waves3", score.PATH_WAV );

            // 未定義の場合 → 空文字列。
            score = スコア.DTX.文字列から生成する( @"
#WAV01: sound1.wav
" );
            Assert.AreEqual( @"", score.PATH_WAV );

            // 未定義かつ譜面ファイルパスの指定がある場合 → 譜面ファイルがあるフォルダの絶対パス。
            score = スコア.DTX.文字列から生成する( @"
#WAV01: sound1.wav
" );
            score.譜面ファイルの絶対パス = @"D:\DTXFiles\Demo\score.dtx";
            Assert.AreEqual( @"D:\DTXFiles\Demo", score.PATH_WAV );

            // 相対パスでの定義ありかつ譜面ファイルパスの指定がある場合 → 譜面ファイルがあるフォルダからの相対 PATH_WAV パス。
            score = スコア.DTX.文字列から生成する( @"
#PATH_WAV: waves
#WAV01: sound1.wav
" );
            score.譜面ファイルの絶対パス = @"D:\DTXFiles\Demo\score.dtx";
            Assert.AreEqual( @"D:\DTXFiles\Demo\waves", score.PATH_WAV );

            // 絶対パスでの定義がある場合 → PATH_WAV パス。
            score = スコア.DTX.文字列から生成する( @"
#PATH_WAV: D:\waves
#WAV01: sound1.wav
" );
            score.譜面ファイルの絶対パス = @"D:\DTXFiles\Demo\score.dtx";
            Assert.AreEqual( @"D:\waves", score.PATH_WAV );
        }

        [TestMethod()]
        public void コマンド_WAVzz()
        {
            // 基本形
            var score = スコア.DTX.文字列から生成する( @"
#title: てすと
#wav01: snare.wav
#wav02: bass.wav
#00111: 010203
" );
            Assert.AreEqual( @"snare.wav", score.WAVリスト[ 1 ].ファイルパス );
            Assert.AreEqual( @"bass.wav", score.WAVリスト[ 2 ].ファイルパス );
            this.例外が出れば成功( () => { var p = score.WAVリスト[ 3 ]; } );

            // PATH_WAV はまだどちらにも反映されないこと。
            score = スコア.DTX.文字列から生成する( @"
#title: てすと
#wav01: snare.wav
#path_wav: sounds
#wav02: bass.wav
#00111: 010203
" );
            Assert.AreEqual( @"snare.wav", score.WAVリスト[ 1 ].ファイルパス );
            Assert.AreEqual( @"bass.wav", score.WAVリスト[ 2 ].ファイルパス );
        }

        [TestMethod()]
        public void コマンド_オブジェクト配置()
        {
            #region " 小節長倍率 "
            //----------------
            {
                var score = スコア.DTX.文字列から生成する( @"
#00102: 3.14          ; 小節長倍率
#00111: 0101
" );
                Assert.AreEqual( 1 + 2 + 1, score.小節長倍率リスト.Count );     // 空の先頭小節1個 ＋ 小節2個(0,1) ＋ 小節線のために末尾に追加される空の小節1個

                Assert.AreEqual( 1.0, score.小節長倍率リスト[ 0 ] );    // 小節 -1
                Assert.AreEqual( 1.0, score.小節長倍率リスト[ 1 ] );    // 小節 0 ;   譜面と小節番号は１つずつずれることに注意
                Assert.AreEqual( 3.14, score.小節長倍率リスト[ 2 ] );   // 小節 1
                Assert.AreEqual( 3.14, score.小節長倍率リスト[ 3 ] );   // 小節 2
            }
            //----------------
            #endregion

            #region " 基本形 "
            //----------------
            {
                var score = スコア.DTX.文字列から生成する( @"
#00111: 01FF03          ; HiHat Close
" );
                // 小節  0:  小節の先頭1個 ＋ 小節線1個 ＋ 拍線3個 ＋ BPM1個 ＝ 6個
                // 小節  1:  小節の先頭1個 ＋ 小節線1個 ＋ 拍線3個 ＝ 5個
                // 小節  2:  小節の先頭1個 ＋ 小節線1個 ＋ 拍線3個 ＋ HHC3個 ＝ 8個
                // 小節  3:  小節の先頭1個 ＋ 小節線1個 ＝ 2個
                Assert.AreEqual( 6 + 5 + 8 + 2, score.チップリスト.Count );

                var chips =
                    from chip in score.チップリスト
                    where ( chip.チップ種別 == チップ種別.HiHat_Close )
                    orderby chip.小節内位置
                    select chip;

                // HHCloseが設定した数だけあること。
                Assert.AreEqual( 3, chips.Count() );

                // オブジェクト値がチップサブIDプロパティに格納されていること。
                int i = 0;
                Assert.AreEqual( 1, chips.ElementAt( i++ ).チップサブID );
                Assert.AreEqual( 15 * 36 + 15, chips.ElementAt( i++ ).チップサブID );   // 36進数なので注意
                Assert.AreEqual( 3, chips.ElementAt( i++ ).チップサブID );
            }
            //----------------
            #endregion
        }

        [TestMethod()]
        public void 小節長倍率チップTest()
        {
            var score = スコア.DTX.文字列から生成する( @"
#title: てすと
#00111: 01		; HiHat_Close
#00202: 3.14	; 小節長倍率
#00311: 02		; HiHat_Close
" );
            Assert.AreEqual( 1 + 4 + 1, score.小節長倍率リスト.Count ); // 先頭の空小節1個 ＋ 譜面の小節数(0～3の4個) ＋ 最後の小節線だけのために自動追加される空の小節1個

            // 小節長倍率は、指定した小節以降すべての小節で有効。（DTX仕様; SSTではその小節のみ有効なので注意）
            // --> 上のサンプルでは、小節 02 で x3.14 を指定しているだけだが小節 03 も x3.14 になる。
            var 倍率s = new double[ 6 ] { 1, 1, 1, 3.14, 3.14, 3.14 };  // また、一番最後の小節長倍率は 1f で固定。拍線はない小節なので、見た目も問題ない。（SST仕様）

            for( int i = 0; i < score.小節長倍率リスト.Count; i++ )
                Assert.AreEqual( 倍率s[ i ], score.小節長倍率リスト[ i ] );
        }

        [TestMethod()]
        public void コマンド_PAN_WAVPAN()
        {
            var score = スコア.DTX.文字列から生成する( @"
#pan01: 10
#pan02: 20
#pan03: 100
#wavpan04: -100         ; wavpan は pan と同義
#pan05: 101
#00111: 010203040506    ; HiHat Close
" );
            var HHchips =
                from chip in score.チップリスト
                where ( chip.チップ種別 == チップ種別.HiHat_Close )
                orderby chip.小節内位置
                select chip;

            Assert.AreEqual( 6, HHchips.Count() );    // HH(ch.11) は計 6 個

            #region " チップ[0]について検証 "
            //----------------
            {
                var chip = HHchips.ElementAt( 0 );

                // オブジェクト値 01 がチップサブIDプロパティに格納されていること。
                Assert.AreEqual( 1, chip.チップサブID );

                // 位置が 10 であること。
                Assert.AreEqual( 10, chip.左右位置 );
            }
            //----------------
            #endregion
            #region " チップ[1]について検証 "
            //----------------
            {
                var chip = HHchips.ElementAt( 1 );

                // オブジェクト値 02 がチップサブIDプロパティに格納されていること。
                Assert.AreEqual( 2, chip.チップサブID );

                // 位置が 20 であること。
                Assert.AreEqual( 20, chip.左右位置 );
            }
            //----------------
            #endregion
            #region " チップ[2]について検証 "
            //----------------
            {
                var chip = HHchips.ElementAt( 2 );

                // オブジェクト値 03 がチップサブIDプロパティに格納されていること。
                Assert.AreEqual( 3, chip.チップサブID );

                // 位置が 100 であること。
                Assert.AreEqual( 100, chip.左右位置 );
            }
            //----------------
            #endregion
            #region " チップ[3]について検証 "
            //----------------
            {
                var chip = HHchips.ElementAt( 3 );

                // オブジェクト値 04 がチップサブIDプロパティに格納されていること。
                Assert.AreEqual( 4, chip.チップサブID );

                // 位置が -100 であること。（#WAVPAN に対応していること。）
                Assert.AreEqual( -100, chip.左右位置 );
            }
            //----------------
            #endregion
            #region " チップ[4]について検証 "
            //----------------
            {
                var chip = HHchips.ElementAt( 4 );

                // オブジェクト値 05 がチップサブIDプロパティに格納されていること。
                Assert.AreEqual( 5, chip.チップサブID );

                // 位置が 100 であること。（-100～+100 の範囲外の値は、この範囲内に丸められる。）
                Assert.AreEqual( 100, chip.左右位置 );
            }
            //----------------
            #endregion
            #region " チップ[5]について検証 "
            //----------------
            {
                var chip = HHchips.ElementAt( 5 );

                // オブジェクト値 06 がチップサブIDプロパティに格納されていること。
                Assert.AreEqual( 6, chip.チップサブID );

                // 位置が 0 であること。（#PAN/#WAVPANの指定がない場合の規定値）
                Assert.AreEqual( 0, chip.左右位置 );
            }
            //----------------
            #endregion
        }

        [TestMethod()]
        public void コマンド_VOLUME_WAVVOL()
        {
            var score = スコア.DTX.文字列から生成する( @"
#title: てすと
#volume01: 0
#volume02: 100
#volume03: 1
#wavvol04: 99
#volume05: 101
#00111: 010203040506
" );
            var chips = from chip in score.チップリスト
                        where ( chip.チップ種別 == チップ種別.HiHat_Close )
                        orderby chip.小節内位置
                        select chip;

            Assert.AreEqual( 6, chips.Count() );    // ch.11 × 6

            #region " チップ[0]について検証 "
            //----------------
            {
                var chip = chips.ElementAt( 0 );

                // オブジェクト値がチップサブIDプロパティに格納されていること。
                Assert.AreEqual( 1, chip.チップサブID ); // 01

                // 音量が 1 であること。
                // DTX音量 0 → SSTFには無音はないので最小値の 1 。
                Assert.AreEqual( 1, chip.音量 );
            }
            //----------------
            #endregion
            #region " チップ[1]について検証 "
            //----------------
            {
                var chip = chips.ElementAt( 1 );

                // オブジェクト値がチップサブIDプロパティに格納されていること。
                Assert.AreEqual( 2, chip.チップサブID ); // 02

                // 音量が 最大 であること。
                Assert.AreEqual( チップ.最大音量, chip.音量 );
            }
            //----------------
            #endregion
            #region " チップ[2]について検証 "
            //----------------
            {
                var chip = chips.ElementAt( 2 );

                // オブジェクト値がチップサブIDプロパティに格納されていること。
                Assert.AreEqual( 3, chip.チップサブID ); // 03

                // 音量が 1 であること。
                Assert.AreEqual( 1, chip.音量 );
            }
            //----------------
            #endregion
            #region " チップ[3]について検証 "
            //----------------
            {
                var chip = chips.ElementAt( 3 );

                // オブジェクト値がチップサブIDプロパティに格納されていること。
                Assert.AreEqual( 4, chip.チップサブID ); // 04

                // 音量が 最大 であること。
                Assert.AreEqual( チップ.最大音量, chip.音量 );
            }
            //----------------
            #endregion
            #region " チップ[4]について検証 "
            //----------------
            {
                var chip = chips.ElementAt( 4 );

                // オブジェクト値がチップサブIDプロパティに格納されていること。
                Assert.AreEqual( 5, chip.チップサブID ); // 05

                // 音量が 最大 であること。
                Assert.AreEqual( チップ.最大音量, chip.音量 );
            }
            //----------------
            #endregion
            #region " チップ[5]について検証 "
            //----------------
            {
                var chip = chips.ElementAt( 5 );

                // オブジェクト値がチップサブIDプロパティに格納されていること。
                Assert.AreEqual( 6, chip.チップサブID ); // 06

                // 音量が 最大 であること。
                // DTXで音量の指定がない場合の規定値は100。
                Assert.AreEqual( チップ.最大音量, chip.音量 );
            }
            //----------------
            #endregion
        }

        [TestMethod()]
        public void コマンド_BPM_BASEBPM_CH03_CH08Test()
        {
            var score = スコア.DTX.文字列から生成する( @"
#title: てすと
#basebpm 220
#bpm 140
#bpm01: 132.4
#00003: 10
#00103: 20
#00208: 01
" );
            var bpmChips =
                from chip in score.チップリスト
                where ( chip.チップ種別 == チップ種別.BPM )
                orderby chip.小節番号
                select chip;

            Assert.AreEqual( 4, bpmChips.Count() );    // #BPM:x1, ch03x2, ch08x1

            // #BPM には #BASEBPM が加算されないこと。
            Assert.AreEqual( 140, bpmChips.ElementAt( 0 ).BPM );

            // ch03 には #BASEBPM が加算されていること。
            Assert.AreEqual( ( 220 + 0x10 ), bpmChips.ElementAt( 1 ).BPM );
            Assert.AreEqual( ( 220 + 0x20 ), bpmChips.ElementAt( 2 ).BPM );

            // ch08 には #BASEBPM が加算されていること。
            Assert.AreEqual( ( 220 + 132.4 ), bpmChips.ElementAt( 3 ).BPM );
        }

        [TestMethod()]
        public void コマンド_AVIzz()
        {
            // 基本形
            var score = スコア.DTX.文字列から生成する( @"
#avi01: movie1.avi
#avi02: movie2.mp4
#00111: 010203
" );
            Assert.AreEqual( @"movie1.avi", score.AVIリスト[ 1 ] );
            Assert.AreEqual( @"movie2.mp4", score.AVIリスト[ 2 ] );
            this.例外が出れば成功( () => { var p = score.AVIリスト[ 3 ]; } );

            // PATH_WAV はまだどちらにも反映されないこと。
            score = スコア.DTX.文字列から生成する( @"
#path_wav: sounds
#avi01: movie1.avi
#avi02: movie2.mp4
#00111: 010203
" );
            Assert.AreEqual( @"movie1.avi", score.AVIリスト[ 1 ] );
            Assert.AreEqual( @"movie2.mp4", score.AVIリスト[ 2 ] );
        }

        [TestMethod()]
        public void 曲データTest()
        {
            var score = スコア.DTX.文字列から生成する( @"
; Created by DTXCreator 018

#TITLE: TO MAKE THE END OF BATTLE
#ARTIST: 古代 祐三 / DTX by FROM
#COMMENT: PCゲーム「YsⅡ」（1988年）のオープニングから。動画は、プレビュー：PC88版（一部超加速）、本編：Eternal 版。YsⅠの勇壮なエンディングもつかの間、ダームの塔から天空の国イースへ一気に吹っ飛ばされる主人公。なんか途中で音速超えて、音の壁とか見えてます。こわー。ギターはすべて打ち込みなので、ヘンな弦の重ね方してるとこも多いですが、気になさらぬよう。
#PREVIEW: sounds\preview.ogg
#STAGEFILE: images\nowloading.jpg
#BPM: 232.50
#DLEVEL: 52
#GLEVEL: 60

#PREIMAGE: images\preimage.jpg
#PREMOVIE: images\premovie.avi
#DTXC_LANEBINDEDCHIP: 03 05 00
#DTXC_LANEBINDEDCHIP: 04 03 00
#DTXC_LANEBINDEDCHIP: 05 07 00
#DTXC_LANEBINDEDCHIP: 06 08 00
#DTXC_LANEBINDEDCHIP: 07 09 00
#DTXC_LANEBINDEDCHIP: 03 0D 00
#DTXC_LANEBINDEDCHIP: 04 03 00
#DTXC_LANEBINDEDCHIP: 01 0Z 00
#DTXC_LANEBINDEDCHIP: 04 03 00
#DTXC_LANEBINDEDCHIP: 05 07 00
#DTXC_LANEBINDEDCHIP: 06 09 00
#DTXC_LANEBINDEDCHIP: 07 0B 00
#DTXC_LANEBINDEDCHIP: 01 0Z 00
#DTXC_LANEBINDEDCHIP: 04 03 00


#WAV01: sounds\bgm.ogg
#BGMWAV: 01
#WAV03: sounds\Drums\bd.ogg
#WAV07: sounds\Drums\rtom_m.ogg
#WAV08: sounds\Drums\rtom_m.ogg
#VOLUME08: 50
#WAV09: sounds\Drums\rtom_l.ogg
#WAV0A: sounds\Drums\rtom_l.ogg
#VOLUME0A: 50
#WAV0B: sounds\Drums\rtom_f.ogg
#WAV0C: sounds\Drums\rtom_f.ogg
#VOLUME0C: 50
#WAV0F: sounds\Drums\sd1.ogg
#WAV0G: sounds\Drums\sd1.ogg
#VOLUME0G: 70
#WAV0H: sounds\Drums\sd1.ogg
#VOLUME0H: 40
#WAV0I: sounds\Drums\sd2.ogg
#WAV0J: sounds\Drums\sd2.ogg
#VOLUME0J: 50
#WAV0K: sounds\Drums\sd3.ogg
#WAV0L: sounds\Drums\sd4.ogg
#VOLUME0L: 50
#WAV0N: sounds\Drums\hhc.ogg
#WAV0O: sounds\Drums\hhc.ogg
#VOLUME0O: 60
#WAV0P: sounds\Drums\hhm.ogg
#WAV0Q: sounds\Drums\hhm.ogg
#VOLUME0Q: 70
#WAV0R: sounds\Drums\hho.ogg
#WAV0S: sounds\Drums\hho.ogg
#VOLUME0S: 70
#WAV0U: sounds\Drums\cyc12.ogg	;中
#WAV0V: sounds\Drums\cyc18c.ogg	;中
#WAV0W: sounds\Drums\cycr.ogg	;右
#WAV0X: sounds\Drums\cyc16.ogg	;右
#WAV0Y: sounds\Drums\cyc16m.ogg	;右
#WAV0Z: sounds\Drums\cyc18.ogg	;左
#WAV10: sounds\Drums\cyc15m.ogg	;左
#WAV12: sounds\Drums\ride22.ogg
#WAV14: sounds\Drums\efex.ogg
#WAV15: sounds\Drums\revcym.ogg
#WAV16: sounds\Drums\cyc8sa.ogg


#AVI01: images\Ys2 Eternal OP.avi


#BPM01: 232.5

#00054: 0000000000000000000000000000000000010000000000000000000000000000    ; 小節 000  ch54: 背景動画
#00101: 0001000000000000000000000000000000000000000000000000000000000000    ;      001  ch01: BGM
#00161: 00000000001500000000000000000000                                    ;           ch61: SE1
#00213: 0000000000030000                                                    ;      002  ch13: Bass
#00212: 0000000F                                                            ;           ch12: Snare
#0021A: 00000016                                                            ;           ch1A: LeftCymbal
#00316: 0X                                                                  ;      003  ch16: RightCymbal
#00313: 0300000300030000                                                    ;           ch13: Bass
#00318: 000R0P0R                                                            ;           ch18: HiHat Open
#00312: 000F000F                                                            ;           ch12: Snare
#00321: 0000002300000000                                                    ;           ch21: GuitarAuto
#00322: 0000220000000000                                                    ;           ch22: GuitarAuto
#00324: 2021000000242526                                                    ;           ch24: GuitarAuto
; (略)
#10712: 0F00000F00000F0I                                                    ;      107  ch12: Snare
#10716: 0X00000W00001414                                                    ;           ch16: RightCymbal
#10713: 0000030000030003                                                    ;           ch13: Bass
#10762: 0000000000000016                                                    ;           ch62: SE2
#10761: 0000000000000014                                                    ;           ch61: SE1
#10723: 000000FV00000000                                                    ;           ch23: GuitarAuto
#10725: FU00000000000000                                                    ;           ch25: GuitarAuto
#10727: 000000000000FWFX                                                    ;           ch27: GuitarAuto
#10908: 00000000000000000000000000000001                                    ;      109  ch08: 拡張BPM


#DTXC_LANEBINDEDCHIP: 08 0X 12
#DTXC_CHIPPALETTE: 
" );
            // ヘッダチェック
            Assert.AreEqual( @"TO MAKE THE END OF BATTLE", score.曲名 );
            Assert.AreEqual( @"古代 祐三 / DTX by FROM", score.アーティスト名 );
            Assert.AreEqual( @"PCゲーム「YsⅡ」（1988年）のオープニングから。動画は、プレビュー：PC88版（一部超加速）、本編：Eternal 版。YsⅠの勇壮なエンディングもつかの間、ダームの塔から天空の国イースへ一気に吹っ飛ばされる主人公。なんか途中で音速超えて、音の壁とか見えてます。こわー。ギターはすべて打ち込みなので、ヘンな弦の重ね方してるとこも多いですが、気になさらぬよう。", score.説明文 );
            Assert.AreEqual( @"images\preimage.jpg", score.プレビュー画像ファイル名 );
            Assert.AreEqual( @"images\premovie.avi", score.プレビュー動画ファイル名 );
            Assert.AreEqual( 5.2, score.難易度 );

            // WAVリストチェック
            Assert.AreEqual( 32, score.WAVリスト.Count );
            Assert.AreEqual( @"sounds\bgm.ogg", score.WAVリスト[ 1 ].ファイルパス );                 // WAV01
            Assert.AreEqual( @"sounds\Drums\bd.ogg", score.WAVリスト[ 3 ].ファイルパス );            // WAV03
            Assert.AreEqual( @"sounds\Drums\rtom_m.ogg", score.WAVリスト[ 7 ].ファイルパス );        // WAV07
            Assert.AreEqual( @"sounds\Drums\rtom_m.ogg", score.WAVリスト[ 8 ].ファイルパス );        // WAV08
            Assert.AreEqual( @"sounds\Drums\cyc8sa.ogg", score.WAVリスト[ 36 + 6 ].ファイルパス );   // WAV16

            // チップリストチェック
            var chips = score.チップリスト.Where( ( c ) => ( c.チップ種別 == チップ種別.BGV ) );
            Assert.AreEqual( 1, chips.Count() );
            Assert.AreEqual( _zz36進数( "01" ), chips.ElementAt( 0 ).チップサブID );

            chips = score.チップリスト.Where( ( c ) => ( c.チップ種別 == チップ種別.BGM ) );
            Assert.AreEqual( 1, chips.Count() );
            Assert.AreEqual( _zz36進数( "01" ), chips.ElementAt( 0 ).チップサブID );

            chips = score.チップリスト.Where( ( c ) => ( c.チップ種別 == チップ種別.BPM ) );
            Assert.AreEqual( 2, chips.Count() );
            Assert.AreEqual( 232.5, chips.ElementAt( 0 ).BPM ); // #BPM:
            Assert.AreEqual( 232.5, chips.ElementAt( 1 ).BPM ); // #BPM01, ch.08

            chips = score.チップリスト.Where( ( c ) => ( c.チップ種別 == チップ種別.Snare ) );
            Assert.AreEqual( 7, chips.Count() );

            Assert.AreEqual( _zz36進数( "0F" ), chips.ElementAt( 0 ).チップサブID );
            Assert.AreEqual( 2 + 1, chips.ElementAt( 0 ).小節番号 );    // 1つずれるので注意
            Assert.AreEqual( 384, chips.ElementAt( 0 ).小節解像度 );
            Assert.AreEqual( 384 * 3 / 4, chips.ElementAt( 0 ).小節内位置 );

            Assert.AreEqual( _zz36進数( "0F" ), chips.ElementAt( 1 ).チップサブID );
            Assert.AreEqual( 3 + 1, chips.ElementAt( 1 ).小節番号 );    // 1つずれるので注意
            Assert.AreEqual( 384, chips.ElementAt( 1 ).小節解像度 );
            Assert.AreEqual( 384 * 1 / 4, chips.ElementAt( 1 ).小節内位置 );

            Assert.AreEqual( _zz36進数( "0F" ), chips.ElementAt( 2 ).チップサブID );
            Assert.AreEqual( 3 + 1, chips.ElementAt( 2 ).小節番号 );    // 1つずれるので注意
            Assert.AreEqual( 384, chips.ElementAt( 2 ).小節解像度 );
            Assert.AreEqual( 384 * 3 / 4, chips.ElementAt( 2 ).小節内位置 );

            Assert.AreEqual( _zz36進数( "0F" ), chips.ElementAt( 3 ).チップサブID );
            Assert.AreEqual( _zz36進数( "0F" ), chips.ElementAt( 4 ).チップサブID );
            Assert.AreEqual( _zz36進数( "0F" ), chips.ElementAt( 5 ).チップサブID );
            Assert.AreEqual( _zz36進数( "0I" ), chips.ElementAt( 6 ).チップサブID );

            chips = score.チップリスト.Where( ( c ) => ( c.チップ種別 == チップ種別.Bass ) );
            Assert.AreEqual( 7, chips.Count() );

            chips = score.チップリスト.Where( ( c ) => ( c.チップ種別 == チップ種別.LeftCrash ) );
            Assert.AreEqual( 1, chips.Count() );

            chips = score.チップリスト.Where( ( c ) => ( c.チップ種別 == チップ種別.RightCrash ) );
            Assert.AreEqual( 5, chips.Count() );

            chips = score.チップリスト.Where( ( c ) => ( c.チップ種別 == チップ種別.HiHat_Open ) );
            Assert.AreEqual( 3, chips.Count() );

            chips = score.チップリスト.Where( ( c ) => ( c.チップ種別 == チップ種別.SE1 ) );
            Assert.AreEqual( 2, chips.Count() );

            chips = score.チップリスト.Where( ( c ) => ( c.チップ種別 == チップ種別.SE2 ) );
            Assert.AreEqual( 1, chips.Count() );

            chips = score.チップリスト.Where( ( c ) => ( c.チップ種別 == チップ種別.GuitarAuto ) );
            Assert.AreEqual( 11, chips.Count() );
        }


        private int _zz16進数( string zz )
        {
            if( !スコア.DTX._16進数2桁の文字列を数値に変換して返す( zz, out int zz16 ) )
                return -1;
            return zz16;
        }
        private int _zz36進数( string zz )
        {
            if( !スコア.DTX._36進数2桁の文字列を数値に変換して返す( zz, out int zz36 ) )
                return -1;
            return zz36;
        }
    }
}