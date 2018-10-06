using Microsoft.VisualStudio.TestTools.UnitTesting;
using SSTFormat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SSTFormat.v1;

namespace SSTFormat.v1.Tests
{
    [TestClass()]
    public class スコアTests
    {
        [TestMethod()]
        public void コマンドのパラメータ文字列部分を返すTest()
        {
            string パラメータ = null;

            // 正常系。

            #region " 基本形。"

            Assert.IsTrue( スコア.コマンドのパラメータ文字列部分を返す(
                    対象文字列: @"#TITLE: 曲名",
                    コマンド名: @"TITLE",
                    パラメータ文字列: out パラメータ ) );
            Assert.AreEqual(
                actual: パラメータ,
                expected: @"曲名",
                ignoreCase: true );

            #endregion
            #region " 大文字小文字は区別しない。"

            Assert.IsTrue( スコア.コマンドのパラメータ文字列部分を返す(
                    対象文字列: @"#titLE: 曲名",   // 大文字小文字が混在
                    コマンド名: @"TITLE",
                    パラメータ文字列: out パラメータ ) );
            Assert.AreEqual(
                actual: パラメータ,
                expected: @"曲名",
                ignoreCase: true );

            Assert.IsTrue( スコア.コマンドのパラメータ文字列部分を返す(
                    対象文字列: @"#titLE: 曲名",
                    コマンド名: @"TITle",    // 大文字小文字が混在
                    パラメータ文字列: out パラメータ ) );
            Assert.AreEqual(
                actual: パラメータ,
                expected: @"曲名",
                ignoreCase: true );

            #endregion
            #region " コロンは省略可能。ただし空白文字で区切ること。"

            Assert.IsTrue( スコア.コマンドのパラメータ文字列部分を返す(
                    対象文字列: @"#TITLE 曲名",    // コロンの代わりに空白文字を使える。
                    コマンド名: @"TITLE",
                    パラメータ文字列: out パラメータ ) );
            Assert.AreEqual(
                actual: パラメータ,
                expected: @"曲名",
                ignoreCase: true );

            #endregion
            #region " 全角空白文字で区切ってもOK。"

            Assert.IsTrue( スコア.コマンドのパラメータ文字列部分を返す(
                対象文字列: @"#TITLE　曲名",    // ← 全角空白文字で区切ってる
                コマンド名: @"TITLE",
                パラメータ文字列: out パラメータ ) );
            Assert.AreEqual(
                actual: パラメータ,
                expected: @"曲名",
                ignoreCase: true );

            #endregion

            // 正常系・コメント関連。

            #region " ; 以降はコメントとして無視される。"

            Assert.IsTrue( スコア.コマンドのパラメータ文字列部分を返す(
                    対象文字列: @"#TITLE: 曲名;コメントです。",
                    コマンド名: @"TITLE",
                    パラメータ文字列: out パラメータ ) );
            Assert.AreEqual(
                actual: パラメータ,
                expected: @"曲名",
                ignoreCase: true );

            #endregion
            #region " ; の前後の半角空白は無視される。"

            Assert.IsTrue( スコア.コマンドのパラメータ文字列部分を返す(
                    対象文字列: @"#TITLE: 曲名  ;  コメントです。",
                    コマンド名: @"TITLE",
                    パラメータ文字列: out パラメータ ) );
            Assert.AreEqual(
                actual: パラメータ,
                expected: @"曲名",
                ignoreCase: true );

            #endregion
            #region " ; の前後のTABは無視される。"

            Assert.IsTrue( スコア.コマンドのパラメータ文字列部分を返す(
                    対象文字列: @"#TITLE: 曲名	;	コメントです。",   // ← TAB で区切ってる
                    コマンド名: @"TITLE",
                    パラメータ文字列: out パラメータ ) );
            Assert.AreEqual(
                actual: パラメータ,
                expected: @"曲名",
                ignoreCase: true );

            #endregion
            #region " ; の前の全角空白は無視されない。"

            Assert.IsTrue( スコア.コマンドのパラメータ文字列部分を返す(
                    対象文字列: @"#TITLE: 曲名　　　;コメントです。",   // "曲名" + 全角空白×3個
                    コマンド名: @"TITLE",
                    パラメータ文字列: out パラメータ ) );
            Assert.AreEqual(
                actual: パラメータ,
                expected: @"曲名　　　", // 末尾の全角もパラメータの一部とみなす。
                ignoreCase: true );

            #endregion

            // 準正常。

            #region " コロンなしでくっつけたら別のコマンド扱いになる。"

            Assert.IsFalse( スコア.コマンドのパラメータ文字列部分を返す( // false が返される。
                    対象文字列: @"#TITLE曲名",
                    コマンド名: @"TITLE",
                    パラメータ文字列: out パラメータ ) );
            Assert.IsNull( パラメータ ); // 戻り値が false のときには、コマンド違いということでパラメータ引数には null が返されてる。

            #endregion
            #region " # とコマンド名は分離不可。"

            Assert.IsFalse( スコア.コマンドのパラメータ文字列部分を返す(   // false が返される。
                    対象文字列: @"# TITLE: 曲名",
                    コマンド名: @"TITLE",
                    パラメータ文字列: out パラメータ ) );
            Assert.IsNull( パラメータ ); // 戻り値が false のときには、コマンド違いということでパラメータ引数には null が返されてる。

            #endregion
        }
    }
}
