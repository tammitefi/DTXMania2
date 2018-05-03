using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using FDK;
using DTXmatixx.Viewer;

namespace DTXmatixx
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            try
            {
                Trace.AutoFlush = true;

                #region " AppData フォルダがなければ作成する。"
                //----------------
                var AppDataフォルダ名 = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create ), @"DTXMatixx\" );

                // なければ作成。
                if( !( Directory.Exists( AppDataフォルダ名 ) ) )
                    Directory.CreateDirectory( AppDataフォルダ名 );
                //----------------
                #endregion

                #region " ログファイルへのログの複製出力開始 "
                //----------------
                Program.ログファイル名 = Log.ログファイル名を生成する( Path.Combine( AppDataフォルダ名, "Logs" ), "Log.", TimeSpan.FromDays( 30 ) );    // 最大30日分保存
                Trace.Listeners.Add( new TraceLogListener( new StreamWriter( Program.ログファイル名, false, Encoding.GetEncoding( "utf-8" ) ) ) );
                //----------------
                #endregion

                Log.現在のスレッドに名前をつける( "描画" );

                Log.WriteLine( "========================" );
                Log.WriteLine( Application.ProductName + " " + App.リリース番号.ToString( "000" ) );
                Log.WriteLine( "========================" );

                Log.システム情報をログ出力する();
                Log.WriteLine( "" );

                using( var app = new App() )
                {
                    string serviceUri = "net.pipe://localhost/DTXMania";
                    string endPointName = "Viewer";
                    string endPointUri = $"{serviceUri}/{endPointName}";

                    // アプリのWCFサービスホストを生成する。
                    var serviceHost = new ServiceHost( app, new Uri( serviceUri ) );

                    // 名前付きパイプにバインドしたエンドポイントをサービスホストへ追加する。
                    serviceHost.AddServiceEndpoint(
                        typeof( IDTXManiaService ),
                        new NetNamedPipeBinding( NetNamedPipeSecurityMode.None ),
                        endPointName );

                    // サービスの受付を開始する。
                    try
                    {
                        serviceHost.Open();
                        Log.Info( $"WCF サービス {endPointUri} の受付を開始しました。" );
                    }
                    catch( AddressAlreadyInUseException )
                    {
                        MessageBox.Show( "DTXMania はすでに起動しています。多重起動はできません。", "DTXMania Runtime Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
                        return;
                    }

                    // アプリを実行する。
                    try
                    {
                        app.Run();
                    }
                    finally
                    {
                        // サービスの受付を終了する。
                        serviceHost.Close( new TimeSpan( 0, 0, 2 ) );   // 最大2sec待つ
                        Log.Info( $"WCF サービス {endPointUri} の受付を終了しました。" );
                    }
                }
                Log.Header( "アプリケーションを終了しました。" );

                Log.WriteLine( "" );
                Log.WriteLine( "遊んでくれてありがとう！" );
            }
            catch( Exception e )
            {
                using( var dlg = new 未処理例外検出ダイアログ() )
                {
                    Trace.WriteLine( "" );
                    Trace.WriteLine( "====> 未処理の例外が検出されました。" );
                    Trace.WriteLine( "" );
                    Trace.WriteLine( e.ToString() );

                    dlg.ShowDialog();
                }
            }
        }

        public static string ログファイル名 = "";
    }
}
