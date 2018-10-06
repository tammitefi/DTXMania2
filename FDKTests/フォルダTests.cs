using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDK.Tests
{
    [TestClass()]
    public class フォルダTests
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
        public void 絶対パスを相対パスに変換するTest()
        {
            string 結果 = null;

            // 正常系。

            #region " 完全内包 "
            結果 = FDK.Folder.絶対パスを相対パスに変換する(
                基点フォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT\",   // これが
                変換したいフォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT\Users\testUser1\" );  // これに内包される
            Assert.AreEqual(
                expected: @"Users\testUser1\",
                actual: 結果,
                ignoreCase: false );
            #endregion
            #region " 完全内包、大文字小文字は無視 "
            結果 = FDK.Folder.絶対パスを相対パスに変換する(
                基点フォルダの絶対パス: @"C:\Program Files (x86)\strokestylet\",   // 小文字
                変換したいフォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT\Users\testUser1\" );  // 大文字
            Assert.AreEqual(
                expected: @"Users\testUser1\",  // 大文字小文字の違いは無視される
                actual: 結果,
                ignoreCase: false );
            #endregion
            #region " 完全内包外 "
            結果 = FDK.Folder.絶対パスを相対パスに変換する(
                基点フォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT\",   // これと
                変換したいフォルダの絶対パス: @"C:\StrokeStyleT\Users\testUser1\" );  // これが お互いを含めない
            Assert.AreEqual(
                expected: @"..\..\StrokeStyleT\Users\testUser1\",   // ちゃんと相対的にたぐって出力される
                actual: 結果,
                ignoreCase: false );
            #endregion
            #region " 完全逆内包 "
            結果 = FDK.Folder.絶対パスを相対パスに変換する(
                基点フォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT\Users\testUser1\",   // これに
                変換したいフォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT\" );  // これが含まれる場合でも
            Assert.AreEqual(
                expected: @"..\..\",    // ちゃんと識別する。
                actual: 結果,
                ignoreCase: false );
            #endregion

            // 準正常系。

            #region " 別ドライブを指定した → 変換対象パスがそのまま（絶対パスのまま）返される。"
            結果 = FDK.Folder.絶対パスを相対パスに変換する(
                基点フォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT\Users\testUser1\",   // Cドライブ
                変換したいフォルダの絶対パス: @"D:\Program Files (x86)\StrokeStyleT\" );  // Dドライブ
            Assert.AreEqual(
                expected: @"D:\Program Files (x86)\StrokeStyleT\",  // 変換パスがそのまま返される。
                actual: 結果,
                ignoreCase: false );
            #endregion
            #region " 基点に相対パスを指定した → 例外発生 "
            例外が出れば成功( () => {
                結果 = FDK.Folder.絶対パスを相対パスに変換する(
                    基点フォルダの絶対パス: @"Program Files (x86)\StrokeStyleT\",  // 相対パス
                    変換したいフォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT\Users\testUser1\" );
            } );
            #endregion
            #region " 変換元に相対パスを指定した → 例外発生 "
            例外が出れば成功( () => {
                結果 = FDK.Folder.絶対パスを相対パスに変換する(
                    基点フォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT\",
                    変換したいフォルダの絶対パス: @"Program Files (x86)\StrokeStyleT\Users\testUser1\" ); // 相対パス
            } );
            #endregion
            #region " 基点と変換元に相対パスを指定した → 例外発生 "
            例外が出れば成功( () => {
                結果 = FDK.Folder.絶対パスを相対パスに変換する(
                    基点フォルダの絶対パス: @"Program Files (x86)\StrokeStyleT\",  // 相対パス
                    変換したいフォルダの絶対パス: @"Program Files (x86)\StrokeStyleT\Users\testUser1\" ); // 相対パス
            } );
            #endregion

            // 基点パスと変換前パスの末尾の \ の有無での挙動を調べる。
            // → 実際には、どの挙動だろうと System.IO.Path.Combine() を使えば問題ない。

            #region " 完全内包、基点:なし、変換:なし → 結果:なし "
            結果 = FDK.Folder.絶対パスを相対パスに変換する(
                基点フォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT",    // 末尾 \ なし
                変換したいフォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT\Users\testUser1" );   // 末尾 \ なし
            Assert.AreEqual(
                expected: @"Users\testUser1",   // 末尾 \ なし
                actual: 結果,
                ignoreCase: false );
            #endregion
            #region " 完全内包、基点:あり、変換:なし → 結果:なし "
            結果 = FDK.Folder.絶対パスを相対パスに変換する(
                基点フォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT",    // 末尾 \ あり
                変換したいフォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT\Users\testUser1" );   // 末尾 \ なし
            Assert.AreEqual(
                expected: @"Users\testUser1",   // 末尾 \ なし
                actual: 結果,
                ignoreCase: false );
            #endregion
            #region " 完全内包、基点:なし、変換:あり → 結果:あり "
            結果 = FDK.Folder.絶対パスを相対パスに変換する(
                基点フォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT",    // 末尾 \ なし
                変換したいフォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT\Users\testUser1\" );   // 末尾 \ あり
            Assert.AreEqual(
                expected: @"Users\testUser1\",   // 末尾 \ あり
                actual: 結果,
                ignoreCase: false );
            #endregion
            #region " 完全内包、基点:あり、変換:あり → 結果:あり "
            結果 = FDK.Folder.絶対パスを相対パスに変換する(
                基点フォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT\",    // 末尾 \ あり
                変換したいフォルダの絶対パス: @"C:\Program Files (x86)\StrokeStyleT\Users\testUser1\" );   // 末尾 \ あり
            Assert.AreEqual(
                expected: @"Users\testUser1\",   // 末尾 \ あり
                actual: 結果,
                ignoreCase: false );
            #endregion
        }

        [TestMethod()]
        public void フォルダ変数の追加と削除()
        {
            string 結果 = null;

            // 正常系。

            #region " 変数1 を追加して、変数付き絶対パスを展開・生成できる。"

            FDK.Folder.フォルダ変数を追加または更新する( "変数1", @"C:\Test\" ); // フォルダ変数の値は、基本的に末尾に \ を付けて登録する。
            結果 = FDK.Folder.絶対パスに含まれるフォルダ変数を展開して返す( @"$(変数1)Memo.txt" );  // そしてパスの変数直後には \ を付けない。
            Assert.AreEqual(
                expected: @"C:\Test\Memo.txt",
                actual: 結果,
                ignoreCase: false );

            結果 = FDK.Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( @"C:\Test\Memo.txt" );
            Assert.AreEqual(
                expected: @"$(変数1)Memo.txt",    // 生成される
                actual: 結果,
                ignoreCase: false );

            結果 = FDK.Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( @"C:\Test111\Memo.txt" );
            Assert.AreEqual(
                expected: @"C:\Test111\Memo.txt",    // 変数が見当たらないので、そのまま返される
                actual: 結果,
                ignoreCase: false );

            #endregion
            #region " 変数2 を追加して、変数1 と間違えずに絶対パスを変換・生成できる。"

            FDK.Folder.フォルダ変数を追加または更新する( "変数2", @"D:\Test2\" );
            結果 = FDK.Folder.絶対パスに含まれるフォルダ変数を展開して返す( @"変数1 は $(変数1) です。" );
            Assert.AreEqual(
                expected: @"変数1 は C:\Test\ です。",
                actual: 結果,
                ignoreCase: false );

            結果 = FDK.Folder.絶対パスに含まれるフォルダ変数を展開して返す( @"変数2 は $(変数2) です。" );
            Assert.AreEqual(
                expected: @"変数2 は D:\Test2\ です。",
                actual: 結果,
                ignoreCase: false );

            結果 = FDK.Folder.絶対パスに含まれるフォルダ変数を展開して返す( @"変数1 は $(変数1) で、変数2 は $(変数2) です。" );
            Assert.AreEqual(
                expected: @"変数1 は C:\Test\ で、変数2 は D:\Test2\ です。",
                actual: 結果,
                ignoreCase: false );

            結果 = FDK.Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( @"C:\Test\Memo.txt" );
            Assert.AreEqual(
                expected: @"$(変数1)Memo.txt",    // 変数1で生成される
                actual: 結果,
                ignoreCase: false );

            結果 = FDK.Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( @"D:\Test2\Memo.txt" );
            Assert.AreEqual(
                expected: @"$(変数2)Memo.txt",    // 変数2で生成される
                actual: 結果,
                ignoreCase: false );

            #endregion
            #region " 変数1 を削除 → 変数1 だけ変換・生成しなくなる。"

            FDK.Folder.フォルダ変数を削除する( "変数1" );
            結果 = FDK.Folder.絶対パスに含まれるフォルダ変数を展開して返す( @"$(変数1)Memo.txt" );
            Assert.AreEqual(
                expected: @"$(変数1)Memo.txt",        // 変数1 はもう変換されない
                actual: 結果,
                ignoreCase: false );

            結果 = FDK.Folder.絶対パスに含まれるフォルダ変数を展開して返す( @"$(変数2)Memo.txt" );
            Assert.AreEqual(
                expected: @"D:\Test2\Memo.txt", // 変数2 はまだ変換できる
                actual: 結果,
                ignoreCase: false );

            結果 = FDK.Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( @"C:\Test\Memo.txt" );
            Assert.AreEqual(
                expected: @"C:\Test\Memo.txt",    // 変数1 はもう生成されない
                actual: 結果,
                ignoreCase: false );

            結果 = FDK.Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( @"D:\Test2\Memo.txt" );
            Assert.AreEqual(
                expected: @"$(変数2)Memo.txt",    // 変数2 はまだ生成できる
                actual: 結果,
                ignoreCase: false );

            #endregion
            #region " 変数2 を追加 → すでに同一名の変数が存在する場合は、値が上書き登録される。"

            FDK.Folder.フォルダ変数を追加または更新する( "変数2", @"E:\Test2-1\" );
            結果 = FDK.Folder.絶対パスに含まれるフォルダ変数を展開して返す( @"$(変数2)Memo2-1.txt" );
            Assert.AreEqual(
                expected: @"E:\Test2-1\Memo2-1.txt",    // 上書きされた値で変換される
                actual: 結果,
                ignoreCase: false );

            結果 = FDK.Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( @"E:\Test2-1\Memo2-1.txt" );
            Assert.AreEqual(
                expected: @"$(変数2)Memo2-1.txt",    // 上書きされた変数2の値で生成される
                actual: 結果,
                ignoreCase: false );

            結果 = FDK.Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( @"D:\Test2\Memo2.txt" );
            Assert.AreEqual(
                expected: @"D:\Test2\Memo2.txt",    // 上書き前の変数値はもう無効
                actual: 結果,
                ignoreCase: false );

            #endregion
            #region " 変数2 を削除 → 変数1 も 2 も変換・生成しなくなる。"

            FDK.Folder.フォルダ変数を削除する( "変数2" );
            結果 = FDK.Folder.絶対パスに含まれるフォルダ変数を展開して返す( @"$(変数1)Memo.txt" );
            Assert.AreEqual(
                expected: @"$(変数1)Memo.txt",        // 変数1 は変換されない
                actual: 結果,
                ignoreCase: false );

            結果 = FDK.Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( @"C:\Test\Memo.txt" );
            Assert.AreEqual(
                expected: @"C:\Test\Memo.txt",    // 変数1 が見当たらないので、そのまま返される
                actual: 結果,
                ignoreCase: false );

            結果 = FDK.Folder.絶対パスに含まれるフォルダ変数を展開して返す( @"$(変数2)Memo.txt" );
            Assert.AreEqual(
                expected: @"$(変数2)Memo.txt", // 変数2 ももう変換されない
                actual: 結果,
                ignoreCase: false );

            結果 = FDK.Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( @"D:\Test2\Memo.txt" );
            Assert.AreEqual(
                expected: @"D:\Test2\Memo.txt",    // 変数2 が見当たらないので、そのまま返される
                actual: 結果,
                ignoreCase: false );

            #endregion

            // 準正常系。

            #region " 存在しない変数を削除 → 例外発生 "
            例外が出れば成功( () => {
                FDK.Folder.フォルダ変数を削除する( "変数1" );
            } );
            #endregion
        }
    }
}
