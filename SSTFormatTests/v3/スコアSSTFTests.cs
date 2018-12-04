using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SSTFormat.v3.Tests
{
    [TestClass]
    public class スコアSSTFTests
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

        #region 追加のテスト属性
        //
        // テストを作成する際には、次の追加属性を使用できます:
        //
        // クラス内で最初のテストを実行する前に、ClassInitialize を使用してコードを実行してください
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // クラス内のテストをすべて実行したら、ClassCleanup を使用してコードを実行してください
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 各テストを実行する前に、TestInitialize を使用してコードを実行してください
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 各テストを実行した後に、TestCleanup を使用してコードを実行してください
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void SSTFVersion解析Test()
        {
            Version version;

            // 基本形
            version = スコア.SSTF._行にSSTFVersionがあるなら解析して返す( "# SSTFVersion 4.3.2.1" );
            Assert.AreEqual( 4, version.Major );
            Assert.AreEqual( 3, version.Minor );
            Assert.AreEqual( 2, version.Build );
            Assert.AreEqual( 1, version.Revision );

            // build, revision は省略可。
            version = スコア.SSTF._行にSSTFVersionがあるなら解析して返す( "# SSTFVersion 4.3.2" );
            Assert.AreEqual( 4, version.Major );
            Assert.AreEqual( 3, version.Minor );
            Assert.AreEqual( 2, version.Build );
            Assert.AreEqual( -1, version.Revision );    // 未定義の場合は -1。
            version = スコア.SSTF._行にSSTFVersionがあるなら解析して返す( "# SSTFVersion 4.3" );
            Assert.AreEqual( 4, version.Major );
            Assert.AreEqual( 3, version.Minor );
            Assert.AreEqual( -1, version.Build );       // 未定義の場合は -1。
            Assert.AreEqual( -1, version.Revision );    // 未定義の場合は -1。

            // 先頭の '#' は必須。
            version = スコア.SSTF._行にSSTFVersionがあるなら解析して返す( " SSTFVersion 4.3.2.1" );
            Assert.IsNull( version );   // SSTFVersion 宣言文が存在しないとみなされ、null が返される。

            // '#' の前後の空白文字は OK。
            version = スコア.SSTF._行にSSTFVersionがあるなら解析して返す( " \t# 　SSTFVersion 4.3.2.1" );
            Assert.AreEqual( 4, version.Major );
            Assert.AreEqual( 3, version.Minor );
            Assert.AreEqual( 2, version.Build );
            Assert.AreEqual( 1, version.Revision );

            // 'SSTFVersion' は大文字小文字を問わない。
            version = スコア.SSTF._行にSSTFVersionがあるなら解析して返す( "#sstfVERSION 4.3.2.1" );
            Assert.AreEqual( 4, version.Major );
            Assert.AreEqual( 3, version.Minor );
            Assert.AreEqual( 2, version.Build );
            Assert.AreEqual( 1, version.Revision );

            // 'SSTFVersion' と数値をくっつけてもOK。
            version = スコア.SSTF._行にSSTFVersionがあるなら解析して返す( "#SSTFVERSION4.3.2.1" );
            Assert.AreEqual( 4, version.Major );
            Assert.AreEqual( 3, version.Minor );
            Assert.AreEqual( 2, version.Build );
            Assert.AreEqual( 1, version.Revision );
        }

        [TestMethod]
        public void 全行解析するTest()
        {
            #region " 基本形 "
            //----------------
            {
                var text = @"
Title=タイトルです     # 曲名など
Artist=私です          # 曲のアーティスト名。作曲者、演奏者、曲データの作成者など。
Description=説明です   # 説明文。
SoundDevice.Delay=12   # 曲データ作成環境のサウンドデバイスの遅延量[ミリ秒]
Level=1.23             # 難易度。0.00～9.99。
Video=nicovideo:sm33543356      # 背景動画として使うビデオの情報。
PartMemo=1,始まるよー    # 小節ごとのメモ(SSTFEditorで表示される)
PartMemo=99,終わるよー   #    〃
Part = 0;              # 小節 0 の記述を開始
Lane=BPM; Resolution = 1; Chips = 0b166;    # 位置 0/1 で BPM を 166 にする
Lane=Song; Resolution = 128; Chips = 77;    # 位置 77/128 に Song チップを配置する
";
                var score = スコア.SSTF._全行解析する( ref text );

                Assert.AreEqual( "タイトルです", score.曲名 );
                Assert.AreEqual( "私です", score.アーティスト名 );
                Assert.AreEqual( "説明です", score.説明文 );
                Assert.AreEqual( 12, score.サウンドデバイス遅延ms );
                Assert.AreEqual( 1.23, score.難易度 );
                Assert.AreEqual( "nicovideo:sm33543356", score.背景動画ID );
                Assert.AreEqual( 2, score.メモリスト.Count );
                Assert.AreEqual( "始まるよー", score.メモリスト[ 1 ] );
                例外が出れば成功( () => { var m = score.メモリスト[ 2 ]; } );
                Assert.AreEqual( "終わるよー", score.メモリスト[ 99 ] );

                Assert.AreEqual( 4, score.チップリスト.Count );   // 小節メモ1 + 小節メモ99 + BPM + Song

                int i = 0;
                Assert.AreEqual( チップ種別.小節メモ, score.チップリスト[ i ].チップ種別 );
                Assert.AreEqual( 1, score.チップリスト[ i ].小節番号 );
                Assert.AreEqual( 0, score.チップリスト[ i ].小節内位置 );
                Assert.AreEqual( 1, score.チップリスト[ i ].小節解像度 );
                i++;
                Assert.AreEqual( チップ種別.小節メモ, score.チップリスト[ i ].チップ種別 );
                Assert.AreEqual( 99, score.チップリスト[ i ].小節番号 );
                Assert.AreEqual( 0, score.チップリスト[ i ].小節内位置 );
                Assert.AreEqual( 1, score.チップリスト[ i ].小節解像度 );
                i++;
                Assert.AreEqual( チップ種別.BPM, score.チップリスト[ i ].チップ種別 );
                Assert.AreEqual( 0, score.チップリスト[ i ].小節番号 );
                Assert.AreEqual( 0, score.チップリスト[ i ].小節内位置 );
                Assert.AreEqual( 1, score.チップリスト[ i ].小節解像度 );
                Assert.AreEqual( 166, score.チップリスト[ i ].BPM );
                i++;
                Assert.AreEqual( チップ種別.背景動画, score.チップリスト[ i ].チップ種別 );
                Assert.AreEqual( 0, score.チップリスト[ i ].小節番号 );
                Assert.AreEqual( 77, score.チップリスト[ i ].小節内位置 );
                Assert.AreEqual( 128, score.チップリスト[ i ].小節解像度 );
            }
            //----------------
            #endregion

            #region " コメント "
            //----------------
            {
                var text = @"
#Title=ニセのタイトル
Title=ほんもののタイトル
Artist=私    #　ではありません
";
                var score = スコア.SSTF._全行解析する( ref text );
                Assert.AreEqual( "ほんもののタイトル", score.曲名 );
                Assert.AreEqual( "私", score.アーティスト名 );
            }
            //----------------
            #endregion

            #region " 小節長倍率チップ "
            //----------------
            {
                var text = @"
# SSTFVersion 3.3.0.0
Title=てすと
Part=1; Lane=HiHat; Resolution=1; Chips=0;  # HHClose
Part=2s3.14;    # 小節超倍率
Part=3; Lane=HiHat; Resolution=1; Chips=0;  # HHClose
";
                // 解析

                var score = スコア.SSTF._全行解析する( ref text );

                Assert.AreEqual( 3, score.小節長倍率リスト.Count ); // 譜面の小節数(1～3の3個)


                // 後処理

                スコア._後処理を行う( score );

                Assert.AreEqual( 5, score.小節長倍率リスト.Count ); // 譜面の小節数(1～3の3個)＋小節0＋最後の小節線だけのために自動追加される空の小節1個

                var 倍率s = new double[ 5 ] { // 小節長倍率は、指定した小節でのみ有効。
                    1,          // 小節 0
                    1,          // 小節 1
                    3.14,       // 小節 2
                    1,          // 小節 3; DTXならここは 3.14 になる。
                    1           // 小節 4; 一番最後の小節長倍率は 1.0 で固定。拍線はない小節なので、見た目も問題ない。（SST仕様）
                };
                for( int i = 0; i < score.小節長倍率リスト.Count; i++ )
                    Assert.AreEqual( 倍率s[ i ], score.小節長倍率リスト[ i ] );
            }
            //----------------
            #endregion
        }
    }
}
