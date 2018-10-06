using Microsoft.VisualStudio.TestTools.UnitTesting;
using SSTFormat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SSTFormat.Tests
{
    [TestClass()]
    public class VersionTests
    {
        private void ファイルに新規出力する( string tempName, string 出力文字列 )
        {
            using( var sw = new StreamWriter( tempName, false, Encoding.UTF8 ) )
            {
                sw.WriteLine( 出力文字列 );
            }
        }

        [TestMethod()]
        public void CreateVersionFromFileTest()
        {
            string tempName = Path.GetTempFileName();
            try
            {
                // 正常系。

                #region " 正常な記述。"
                this.ファイルに新規出力する( tempName, @"# SSTFVersion 1.2.3.4" );
                Assert.IsTrue( new Version( 1, 2, 3, 4 ) == Version.CreateVersionFromFile( tempName ) );
                #endregion
                #region " 省略時は 1.0.0.0 になる。"
                this.ファイルに新規出力する( tempName, @"# コメントやよー" );
                Assert.IsTrue( new Version( 1, 0, 0, 0 ) == Version.CreateVersionFromFile( tempName ) );
                #endregion
                #region " トークンとバージョン番号の間に空白がなくても大丈夫。"
                this.ファイルに新規出力する( tempName, @"# SSTFVersion5.6.7.8" );
                Assert.IsTrue( new Version( 5, 6, 7, 8 ) == Version.CreateVersionFromFile( tempName ) );
                #endregion
                #region " Revision は省略可能。"
                this.ファイルに新規出力する( tempName, @"# SSTFVersion 9.10.11" );
                Assert.IsTrue( new Version( 9, 10, 11 ) == Version.CreateVersionFromFile( tempName ) );
                #endregion
                #region " Build と Revision は省略可能。"
                this.ファイルに新規出力する( tempName, @"# SSTFVersion 12.13" );
                Assert.IsTrue( new Version( 12, 13 ) == Version.CreateVersionFromFile( tempName ) );
                #endregion

                // 準正常系。

                #region " Major と Minor は省略不可。"
                this.ファイルに新規出力する( tempName, @"# SSTFVersion 12" );
                try
                {
                    var ver = Version.CreateVersionFromFile( tempName );
                }
                catch( System.ArgumentException )
                {
                    // 成功。
                }
                this.ファイルに新規出力する( tempName, @"# SSTFVersion " );
                try
                {
                    var ver = Version.CreateVersionFromFile( tempName );
                }
                catch( System.ArgumentException )
                {
                    // 成功。
                }
                #endregion
                #region " ２行目以降に指定しても無視されて、1.0.0.0 になる。"
                this.ファイルに新規出力する( tempName, @"# １行目やよー\n# SSTFVersion 1.2.3.4\n# ３行目やよー" );
                Assert.IsTrue( new Version( 1, 0, 0, 0 ) == Version.CreateVersionFromFile( tempName ) );
                #endregion
            }
            finally
            {
                File.Delete( tempName );
            }
        }
    }
}
