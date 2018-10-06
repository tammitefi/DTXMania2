using Microsoft.VisualStudio.TestTools.UnitTesting;
using SSTFormat.v2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SSTFormat.v2.Tests
{
    [TestClass()]
    public class DTXReaderTests
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
        public void 行分解Test()
        {
            string コマンド, パラメータ, コメント;

            #region " 空行 "
            //----------------
            DTXReader._行分解( "", out コマンド, out パラメータ, out コメント );
            Assert.IsTrue( string.IsNullOrEmpty( コマンド ) );
            Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
            Assert.IsTrue( string.IsNullOrEmpty( コメント ) );

            DTXReader._行分解( "      ", out コマンド, out パラメータ, out コメント );
            Assert.IsTrue( string.IsNullOrEmpty( コマンド ) );
            Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
            Assert.IsTrue( string.IsNullOrEmpty( コメント ) );

            // TAB → 空白文字扱い。
            DTXReader._行分解( "\t", out コマンド, out パラメータ, out コメント );
            Assert.IsTrue( string.IsNullOrEmpty( コマンド ) );
            Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
            Assert.IsTrue( string.IsNullOrEmpty( コメント ) );

            DTXReader._行分解( "   \t  ", out コマンド, out パラメータ, out コメント );
            Assert.IsTrue( string.IsNullOrEmpty( コマンド ) );
            Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
            Assert.IsTrue( string.IsNullOrEmpty( コメント ) );
            //----------------
            #endregion
            #region " コマンド "
            //----------------
            {
                // 区切り文字なし → OK
                DTXReader._行分解( "#TITLE", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
                Assert.IsTrue( string.IsNullOrEmpty( コメント ) );

                // 区切り文字 ":" あり → OK
                DTXReader._行分解( "#TITLE:", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
                Assert.IsTrue( string.IsNullOrEmpty( コメント ) );

                // 区切り文字は空白文字でもいい。
                DTXReader._行分解( "#TITLE ", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
                Assert.IsTrue( string.IsNullOrEmpty( コメント ) );

                DTXReader._行分解( "#TITLE\t", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
                Assert.IsTrue( string.IsNullOrEmpty( コメント ) );

                DTXReader._行分解( "#TITLE\t:", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
                Assert.IsTrue( string.IsNullOrEmpty( コメント ) );

                // "#" の前後の空白文字 → 無視される。
                DTXReader._行分解( "    #    TITLE", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
                Assert.IsTrue( string.IsNullOrEmpty( コメント ) );

                // "#" がない → コマンドではなくすべてコメントだとみなされる。
                DTXReader._行分解( "TITLE", out コマンド, out パラメータ, out コメント );
                Assert.IsTrue( string.IsNullOrEmpty( コマンド ) );
                Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
                Assert.AreEqual( expected: "TITLE", actual: コメント );
            }
            //----------------
            #endregion
            #region " コメント "
            //----------------
            {
                // 行頭から。→ OK
                DTXReader._行分解( @";コメントだよ！", out コマンド, out パラメータ, out コメント );
                Assert.IsTrue( string.IsNullOrEmpty( コマンド ) );
                Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
                Assert.AreEqual( expected: @"コメントだよ！", actual: コメント );

                // コマンドが NG である（"#"がない） → 行中に ";" があっても、すべてコメントだとみなされる。
                DTXReader._行分解( @"NGコマンド文 ; コメントだよ！", out コマンド, out パラメータ, out コメント );
                Assert.IsTrue( string.IsNullOrEmpty( コマンド ) );
                Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
                Assert.AreEqual( expected: @"NGコマンド文 ; コメントだよ！", actual: コメント );

                // コメントの前後の空白 → 無視される。
                DTXReader._行分解( @";     コメントだよ！     ", out コマンド, out パラメータ, out コメント );
                Assert.IsTrue( string.IsNullOrEmpty( コマンド ) );
                Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
                Assert.AreEqual( expected: @"コメントだよ！", actual: コメント );

                // コメントの区切り文字だけが存在する → すべて null または空文字列になる。
                DTXReader._行分解( ";", out コマンド, out パラメータ, out コメント );
                Assert.IsTrue( string.IsNullOrEmpty( コマンド ) );
                Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
                Assert.IsTrue( string.IsNullOrEmpty( コメント ) );

                // コメントの区切り文字が複数ある → 最初に現れた ';' のみ有効。２回目以降の出現はコメント文に文字列として含まれる。
                DTXReader._行分解( @";コメントその１;その２", out コマンド, out パラメータ, out コメント );
                Assert.IsTrue( string.IsNullOrEmpty( コマンド ) );
                Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
                Assert.AreEqual( expected: @"コメントその１;その２", actual: コメント );
            }
            //----------------
            #endregion
            #region " コマンド＋パラメータ "
            //----------------
            {
                // 基本形。
                DTXReader._行分解( "#TITLE: タイトルだよ！", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.AreEqual( expected: "タイトルだよ！", actual: パラメータ );
                Assert.IsTrue( string.IsNullOrEmpty( コメント ) );

                // 区切り文字は空白でもいい。
                DTXReader._行分解( "#TITLE タイトルだよ！", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.AreEqual( expected: "タイトルだよ！", actual: パラメータ );
                Assert.IsTrue( string.IsNullOrEmpty( コメント ) );

                DTXReader._行分解( "#TITLE\tタイトルだよ！", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.AreEqual( expected: "タイトルだよ！", actual: パラメータ );
                Assert.IsTrue( string.IsNullOrEmpty( コメント ) );

                DTXReader._行分解( "#TITLE \t  タイトルだよ！", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.AreEqual( expected: "タイトルだよ！", actual: パラメータ );
                Assert.IsTrue( string.IsNullOrEmpty( コメント ) );

                // パラメータには途中に空白を含めることができる。ただし、パラメータの前後の空白は無視される。
                DTXReader._行分解( "#TITLE:      タイトルだよ！ これも！     ……これも！               ", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.AreEqual( expected: "タイトルだよ！ これも！     ……これも！", actual: パラメータ );
                Assert.IsTrue( string.IsNullOrEmpty( コメント ) );
            }
            //----------------
            #endregion
            #region " コマンド＋パラメータ＋コメント "
            //----------------
            {
                // 基本形。
                DTXReader._行分解( "#TITLE: タイトルだよ！;コメントだよ！", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.AreEqual( expected: "タイトルだよ！", actual: パラメータ );
                Assert.AreEqual( expected: "コメントだよ！", actual: コメント );

                // 間に空白を入れても無視される。
                DTXReader._行分解( "#TITLE:     タイトルだよ！         ;   \t     コメントだよ！           ", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.AreEqual( expected: "タイトルだよ！", actual: パラメータ );
                Assert.AreEqual( expected: "コメントだよ！", actual: コメント );
            }
            //----------------
            #endregion
            #region " コマンド＋コメント "
            //----------------
            {
                // 基本形。
                DTXReader._行分解( "#TITLE;コメントだよ！", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
                Assert.AreEqual( expected: "コメントだよ！", actual: コメント );

                // 間に空白を入れても無視される。
                DTXReader._行分解( "#TITLE     ;        コメントだよ！     ", out コマンド, out パラメータ, out コメント );
                Assert.AreEqual( expected: "TITLE", actual: コマンド );
                Assert.IsTrue( string.IsNullOrEmpty( パラメータ ) );
                Assert.AreEqual( expected: "コメントだよ！", actual: コメント );
            }
            //----------------
            #endregion
        }

        [TestMethod()]
        public void ReadFromStringTest()
        {
            // インライン
            {
                #region " 小節長倍率チップの検証 "
                //----------------
                {
                    var score = DTXReader.ReadFromString( @"
#title: てすと
#00111: 01		; HiHat_Close
#00202: 3.14	; 小節長倍率
#00311: 02		; HiHat_Close
" );
                    Assert.AreEqual( 5, score.小節長倍率リスト.Count ); // 譜面の小節数(0～3の4個)＋最後の小節線だけのために自動追加される空の小節1個

                    // 小節長倍率は、指定した小節以降すべての小節で有効。（DTX仕様; SSTではその小節のみ有効なので注意）
                    // --> 上のサンプルでは、小節 02 で x3.14 を指定しているだけだが小節 03 も x3.14 になる。
                    var 倍率s = new float[ 5 ] { 1f, 1f, 3.14f, 3.14f, 1f };  // また、一番最後の小節長倍率は 1f で固定。拍線はない小節なので、見た目も問題ない。（SST仕様）

                    for( int i = 0; i < score.小節長倍率リスト.Count; i++ )
                        Assert.AreEqual( 倍率s[ i ], score.小節長倍率リスト[ i ] );
                }
                //----------------
                #endregion
                #region " BPM, BASEBPM, ch03, ch08 チップの検証 "
                //----------------
                {
                    var score = DTXReader.ReadFromString( @"
#title: てすと
#basebpm 220
#bpm 140
#bpm01: 132.4
#00003: 10
#00103: 20
#00208: 01
" );
                    var chips = from chip in score.チップリスト
                                where ( chip.チップ種別 == チップ種別.BPM )
                                orderby chip.小節番号
                                select chip;

                    Assert.AreEqual( 4, chips.Count() );    // #BPM:x1, ch03x2, ch08x1

                    // #BPM には #BASEBPM が加算されないこと。
                    Assert.AreEqual( 140f, chips.ElementAt( 0 ).BPM );

                    // ch03 には #BASEBPM が加算されていること。
                    Assert.AreEqual( ( 220f + 0x10 ), chips.ElementAt( 1 ).BPM );
                    Assert.AreEqual( ( 220f + 0x20 ), chips.ElementAt( 2 ).BPM );

                    // ch08 には #BASEBPM が加算されていること。
                    Assert.AreEqual( ( 220f + 132.4f ), chips.ElementAt( 3 ).BPM );
                }
                //----------------
                #endregion
            }

            // ファイルから
            {
                var score = DTXReader.ReadFromFile( @"v2\テストファイル\ようこそじゃパリパークへ.dtx" );

                Assert.AreEqual( "ようこそジャパリパークへ", score.Header.曲名 );
                Assert.AreEqual( "JVCKENWOOD Victor Entertainment Corp. 2017", score.Header.説明文 );

                #region " BPM チップの検証 "
                //----------------
                {
                    var chips = from chip in score.チップリスト
                                where ( chip.チップ種別 == チップ種別.BPM )
                                orderby chip.小節番号
                                select chip;

                    Assert.AreEqual( expected: 1 + 15, actual: chips.Count() );  // #BPM: + ch08 を15箇所で使用

                    var BPM定義 = new float[] {
                        170.00f,	// #BPM: 170.00
						169f,		// #BPM01: 169
						170f,		// #BPM02: 170
						171f,		// #BPM03: 171
						169.8f,		// #BPM04: 169.8
						171.8f,		// #BPM05: 171.8
						168f,		// #BPM06: 168
						170.8f,		// #BPM07: 170.8
						172f,		// #BPM08: 172
						168.8f,		// #BPM09: 168.8
					};

                    var BPM参照 = new(int 小節番号, float bpm値)[] {
                        ( 0, 170f ),			// #BPM: 170.00
						( 3, BPM定義[ 4 ] ),		// #00308: 04
						( 5, BPM定義[ 3 ] ),		// #00508: 03
						( 7, BPM定義[ 2 ] ),		// #00708: 02
						( 11, BPM定義[ 6 ] ),	// #01108: 06
						( 13, BPM定義[ 4 ] ),	// #01308: 04
						( 16, BPM定義[ 8 ] ),	// #01608: 08
						( 17, BPM定義[ 6 ] ),	// #01708: 06
						( 18, BPM定義[ 4 ] ),	// #01808: 04
						( 23, BPM定義[ 2 ] ),	// #02308: 02
						( 36, BPM定義[ 1 ] ),	// #03608: 01
						( 40, BPM定義[ 3 ] ),	// #04008: 03
						( 42, BPM定義[ 2 ] ),	// #04208: 02
						( 44, BPM定義[ 4 ] ),	// #04408: 04
						( 54, BPM定義[ 1 ] ),	// #05408: 01
						( 56, BPM定義[ 2 ] ),	// #05608: 02
					};

                    for( int i = 0; i < BPM参照.Length; i++ )
                    {
                        var c = chips.ElementAt( i );
                        Assert.AreEqual( expected: BPM参照[ i ].小節番号, actual: c.小節番号 );
                        Assert.AreEqual( expected: BPM参照[ i ].bpm値, actual: c.BPM );
                    }
                }
                //----------------
                #endregion
                #region " BGM チップの検証 "
                //----------------
                {
                    var chips = from chip in score.チップリスト
                                where ( chip.チップ種別 == チップ種別.背景動画 )
                                orderby chip.小節番号
                                select chip;

                    Assert.AreEqual( 1, chips.Count() );

                    // #00001: 00000000000000000000_00000000000000000000_00000000000000000000_00000000000000000000_00000000000000000000_
                    //         00000000000000000000_00000000000000000000_00000000000000000000_00000000000000000000_00000000000000000000_
                    //         00000000000000000000_00000000000000000000_00000000000000000000_00000000000000000000_00000000000000000000_
                    //         00000000000100000000_00000000000000000000_00000000000000000000_00000000000000000000_0000
                    //  --> 155/192 番目　↑ に 01
                    //    = 310/384 番目
                    var c = chips.ElementAt( 0 );
                    Assert.AreEqual( 0, c.小節番号 );
                    Assert.AreEqual( 384, c.小節解像度 );
                    Assert.AreEqual( 310, c.小節内位置 );
                }
                //----------------
                #endregion
            }
        }

        [TestMethod()]
        public void 小節番号とチャンネル番号を取得するTest()
        {
            int 小節番号, チャンネル番号;

            #region " 基本形。"
            //----------------
            {
                Assert.IsTrue( DTXReader._小節番号とチャンネル番号を取得する( "01234", out 小節番号, out チャンネル番号 ) );
                Assert.AreEqual( expected: 12, actual: 小節番号 );
                Assert.AreEqual( expected: 3 * 16 + 4, actual: チャンネル番号 );
            }
            //----------------
            #endregion
            #region " 小節番号は 000 ～ Z99 （36進数1桁＆10進数2桁）であること。"
            //----------------
            {
                Assert.IsTrue( DTXReader._小節番号とチャンネル番号を取得する( "00001", out 小節番号, out チャンネル番号 ) );
                Assert.AreEqual( expected: 0, actual: 小節番号 );
                Assert.AreEqual( expected: 1, actual: チャンネル番号 );

                Assert.IsTrue( DTXReader._小節番号とチャンネル番号を取得する( "09901", out 小節番号, out チャンネル番号 ) );
                Assert.AreEqual( expected: 99, actual: 小節番号 );
                Assert.AreEqual( expected: 1, actual: チャンネル番号 );

                Assert.IsTrue( DTXReader._小節番号とチャンネル番号を取得する( "10001", out 小節番号, out チャンネル番号 ) );
                Assert.AreEqual( expected: 100, actual: 小節番号 );
                Assert.AreEqual( expected: 1, actual: チャンネル番号 );

                Assert.IsTrue( DTXReader._小節番号とチャンネル番号を取得する( "19901", out 小節番号, out チャンネル番号 ) );
                Assert.AreEqual( expected: 199, actual: 小節番号 );
                Assert.AreEqual( expected: 1, actual: チャンネル番号 );

                Assert.IsTrue( DTXReader._小節番号とチャンネル番号を取得する( "99901", out 小節番号, out チャンネル番号 ) );
                Assert.AreEqual( expected: 999, actual: 小節番号 );
                Assert.AreEqual( expected: 1, actual: チャンネル番号 );

                Assert.IsTrue( DTXReader._小節番号とチャンネル番号を取得する( "A0001", out 小節番号, out チャンネル番号 ) );    // A00 == 1000
                Assert.AreEqual( expected: 1000, actual: 小節番号 );
                Assert.AreEqual( expected: 1, actual: チャンネル番号 );

                Assert.IsTrue( DTXReader._小節番号とチャンネル番号を取得する( "A9901", out 小節番号, out チャンネル番号 ) );
                Assert.AreEqual( expected: 10 * 100 + 99, actual: 小節番号 );
                Assert.AreEqual( expected: 1, actual: チャンネル番号 );

                Assert.IsTrue( DTXReader._小節番号とチャンネル番号を取得する( "A9901", out 小節番号, out チャンネル番号 ) );
                Assert.AreEqual( expected: 10 * 100 + 99, actual: 小節番号 );
                Assert.AreEqual( expected: 1, actual: チャンネル番号 );

                Assert.IsTrue( DTXReader._小節番号とチャンネル番号を取得する( "Z9901", out 小節番号, out チャンネル番号 ) );    // Z99 == 3599（最大小節番号）
                Assert.AreEqual( expected: 35 * 100 + 99, actual: 小節番号 );
                Assert.AreEqual( expected: 1, actual: チャンネル番号 );

                Assert.IsFalse( DTXReader._小節番号とチャンネル番号を取得する( "Z9A01", out 小節番号, out チャンネル番号 ) );   // NG; Z99 の次はない

                Assert.IsTrue( DTXReader._小節番号とチャンネル番号を取得する( "z9901", out 小節番号, out チャンネル番号 ) );    // 小文字でもOK
                Assert.AreEqual( expected: 35 * 100 + 99, actual: 小節番号 );
                Assert.AreEqual( expected: 1, actual: チャンネル番号 );
            }
            //----------------
            #endregion
            #region " チャンネル番号は 00 ～ FF （16進数2桁）であること。"
            //----------------
            {
                Assert.IsTrue( DTXReader._小節番号とチャンネル番号を取得する( "0009A", out 小節番号, out チャンネル番号 ) );    // OK
                Assert.AreEqual( expected: 0, actual: 小節番号 );
                Assert.AreEqual( expected: 9 * 16 + 10, actual: チャンネル番号 );

                Assert.IsTrue( DTXReader._小節番号とチャンネル番号を取得する( "000FF", out 小節番号, out チャンネル番号 ) );    // OK
                Assert.AreEqual( expected: 0, actual: 小節番号 );
                Assert.AreEqual( expected: 255, actual: チャンネル番号 );

                Assert.IsFalse( DTXReader._小節番号とチャンネル番号を取得する( "000FG", out 小節番号, out チャンネル番号 ) );   // NG
            }
            //----------------
            #endregion
            #region " 取得元文字列は、常に５文字であること。"
            //----------------
            {
                Assert.IsFalse( DTXReader._小節番号とチャンネル番号を取得する( "123456", out 小節番号, out チャンネル番号 ) );  // NG
                Assert.IsFalse( DTXReader._小節番号とチャンネル番号を取得する( "1234", out 小節番号, out チャンネル番号 ) );    // NG
                Assert.IsFalse( DTXReader._小節番号とチャンネル番号を取得する( "", out 小節番号, out チャンネル番号 ) );        // NG; 空文字もダメ。
            }
            //----------------
            #endregion
        }

        [TestMethod()]
        public void DTX仕様の実数を取得するTest()
        {
            // DTX仕様の実数の定義（カルチャ非依存）
            // 　小数点文字s = ".,";         // '.' の他に ',' も使える。
            // 　桁区切り文字s = ".,' ";     // 桁区切り文字が小数点文字と被ってるが、一番最後に現れたものだけが小数点として認識される。

            float num = 0f;

            // 整数（小数点なし）→ OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( "123", out num ) );
            Assert.AreEqual( 123f, num );


            // 小数点(.)あり → OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( "123.4", out num ) );
            Assert.AreEqual( 123.4f, num );

            // 小数点(,)あり → OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( "123,4", out num ) );
            Assert.AreEqual( 123.4f, num );


            // 桁区切りに見える小数点(,) → OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( "1,234", out num ) );
            Assert.AreEqual( 1.234f, num ); // 1234 ではない。


            // 整数部なしの小数(.) → OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( ".1234", out num ) );
            Assert.AreEqual( 0.1234f, num );

            // 整数部なしの小数(,) → OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( ",1234", out num ) );
            Assert.AreEqual( 0.1234f, num );


            // 小数部なしの小数(.) → OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( "1234.", out num ) );
            Assert.AreEqual( 1234f, num );

            // 小数部なしの小数(,) → OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( "1234,", out num ) );
            Assert.AreEqual( 1234f, num );


            // 整数部に桁区切り(,)あり、小数点あり(.) → OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( "12,345.6", out num ) );
            Assert.AreEqual( 12345.6f, num );

            // 整数部に桁区切り(,)あり、小数点あり(,) → OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( "12,345,6", out num ) );
            Assert.AreEqual( 12345.6f, num );   // 123456 ではない。

            // 整数部に桁区切り(.)あり、小数点あり(,) → OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( "12.345,6", out num ) );
            Assert.AreEqual( 12345.6f, num );   // 12.3456 ではない。


            // 小数点(.)の連続 → OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( "12...345", out num ) );
            Assert.AreEqual( 12.345f, num );   // エラーではない。

            // 小数点(,)の連続 → OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( "123,,,45", out num ) );
            Assert.AreEqual( 123.45f, num );   // エラーではない。

            // 小数点(.)の連続 → OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( "12...34..5", out num ) );
            Assert.AreEqual( 1234.5f, num );   // エラーではない。12.345 でもない。

            // 小数点(,)の連続 → OK
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( "12,,,34,,5", out num ) );
            Assert.AreEqual( 1234.5f, num );   // エラーではない。12.345 でもない。

            // 小数点(.,)の混在 → OK								   
            Assert.IsTrue( DTXReader._DTX仕様の実数を取得する( "12...34,,5", out num ) );
            Assert.AreEqual( 1234.5f, num );   // エラーではない。12.345 でもない。
        }
    }
}
