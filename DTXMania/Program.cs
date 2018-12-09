using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using FDK;
using DTXMania.Viewer;

namespace DTXMania
{
    static class Program
    {
        public const int ログファイルの最大保存日数 = 30;
        public static string ログファイル名 = "";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            try
            {
                Trace.AutoFlush = true;

                #region " %USERPROFILE%/AppData/DTXMania フォルダがなければ作成する。"
                //----------------
                var AppDataフォルダ名 = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create ), @"DTXMania\" );

                // なければ作成。
                if( !( Directory.Exists( AppDataフォルダ名 ) ) )
                    Directory.CreateDirectory( AppDataフォルダ名 );
                //----------------
                #endregion

                #region " ログファイルへのログの複製出力開始。"
                //----------------
                Program.ログファイル名 = Log.ログファイル名を生成する( Path.Combine( AppDataフォルダ名, "Logs" ), "Log.", TimeSpan.FromDays( ログファイルの最大保存日数 ) );

                // ログファイルをTraceリスナとして追加。
                // 以降、Trace（ならびにFDK.Logクラス）による出力は、このリスナ（＝ログファイル）にも出力される。
                Trace.Listeners.Add( new TraceLogListener( new StreamWriter( Program.ログファイル名, false, Encoding.GetEncoding( "utf-8" ) ) ) );
                //----------------
                #endregion

                Log.現在のスレッドに名前をつける( "描画" );
                Log.WriteLine( Application.ProductName + " " + App.リリース番号.ToString( "000" ) );  // アプリ名とバージョン
                Log.システム情報をログ出力する();
                Log.WriteLine( "" );

                // アプリを初期化する。
                using( var app = new App() )
                {
                    var WCFサービスホスト = (ServiceHost) null;

                    #region " WCFサービスホストを起動する。ついでに二重起動チェックも行う。"
                    //----------------
                    // WCFサービスのエンドポイントとURI。
                    string serviceUri = "net.pipe://localhost/DTXMania";
                    string endPointName = "Viewer";
                    string endPointUri = $"{serviceUri}/{endPointName}";

                    // アプリのWCFサービスホストを生成する。
                    WCFサービスホスト = new ServiceHost( app, new Uri( serviceUri ) );

                    // 名前付きパイプにバインドしたエンドポイントをサービスホストへ追加する。
                    WCFサービスホスト.AddServiceEndpoint(
                        typeof( IDTXManiaService ),     // 公開するインターフェース
                        new NetNamedPipeBinding( NetNamedPipeSecurityMode.None ),   // 名前付きパイプ
                        endPointName ); // 公開するエンドポイント

                    // WCFサービスの受付を開始する。
                    try
                    {
                        WCFサービスホスト.Open();
                        Log.Info( $"WCF サービスの受付を開始しました。[{endPointUri}]" );
                    }
                    catch( AddressAlreadyInUseException )
                    {
                        // エンドポイントが使用中なら、アプリの二重起動とみなして終了。
                        MessageBox.Show( "DTXMania はすでに起動しています。多重起動はできません。", "DTXMania Runtime Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
                        return;
                    }
                    //----------------
                    #endregion

                    try
                    {
                        // アプリを実行する。
                        app.Run();
                    }
                    finally
                    {
                        #region " WCFサービスホストを終了する。"
                        //----------------
                        WCFサービスホスト.Close( new TimeSpan( 0, 0, 2 ) );   // 最大2sec待つ
                        Log.Info( $"WCF サービス {endPointUri} の受付を終了しました。" );
                        //----------------
                        #endregion
                    }
                }
                Log.Header( "アプリケーションを終了しました。" );

                Log.WriteLine( "" );
                Log.WriteLine( "遊んでくれてありがとう！" );
            }
#if !DEBUG
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
#else
            finally
            {
                // DEBUG 時には、未処理の例外が発出されてもcatchしない。（デバッガでキャッチすることを想定。）
            }
#endif
        }
    }
}
