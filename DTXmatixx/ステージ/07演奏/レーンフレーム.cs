using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using Newtonsoft.Json.Linq;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.演奏
{
    /// <summary>
    ///		チップの背景であり、レーン全体を示すフレーム画像。
    /// </summary>
    class レーンフレーム : Activity
    {
        /// <summary>
        ///		画面全体に対する、レーンフレームの表示位置と範囲。
        /// </summary>
        public static RectangleF 領域
            => new RectangleF( 445f, 0f, 778f, 938f );
        public static Dictionary<string, レーン配置> レーン配置リスト;

        public class レーン配置
        {
            /// <summary>
            ///     配置名。
            ///     「$(System)images\レーン配置\"配置名".json」ファイルがあれば、それがこのインスタンスの設定ファイルになる。
            /// </summary>
            public string 配置名
            {
                get;
                set;
            }

            /// <summary>
            ///		表示レーンの左端X位置を、レーンフレームの左端からの相対位置で示す。
            /// </summary>
            public Dictionary<表示レーン種別, float> 表示レーンの左端位置dpx
            {
                get;
                set;
            }

            /// <summary>
            ///		表示レーンの幅。
            /// </summary>
            public Dictionary<表示レーン種別, float> 表示レーンの幅dpx
            {
                get;
                set;
            }

            /// <summary>
            ///		レーンラインの領域。
            ///		<see cref="レーンフレーム.領域"/>.Left からの相対値[dpx]。
            /// </summary>
            public List<RectangleF> レーンライン;

            public Color4 レーン色;
            public Color4 レーンライン色;
        }
        public static レーン配置 現在のレーン配置
            => _現在のレーン配置;

        public static void 初期化する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var root = new VariablePath( @"$(System)images\演奏\レーン配置" );
                レーン配置リスト = new Dictionary<string, レーン配置>();

                // images フォルダから、レーン配置に適合する設定ファイルを検索。

                foreach( var fileInfo in new DirectoryInfo( root.変数なしパス ).GetFiles( "*.json", SearchOption.TopDirectoryOnly ) )
                {
                    var 拡張子なしのファイル名 = Path.GetFileNameWithoutExtension( fileInfo.Name );

                    レーン配置リスト.Add( 拡張子なしのファイル名, new レーン配置 { 配置名 = 拡張子なしのファイル名 } );   // 名前だけ
                }

                // 各設定ファイルから設定を読み込む。

                foreach( var kvp in レーン配置リスト )
                {
                    var 設定ファイルパス = Path.Combine( root.変数なしパス, kvp.Key + ".json" );
                    var 設定 = JObject.Parse( File.ReadAllText( 設定ファイルパス ) );

                    kvp.Value.表示レーンの左端位置dpx = new Dictionary<表示レーン種別, float>();
                    kvp.Value.表示レーンの幅dpx = new Dictionary<表示レーン種別, float>();

                    foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
                    {
                        kvp.Value.表示レーンの左端位置dpx.Add( lane, (float) 設定[ "左端位置" ][ lane.ToString() ] );
                        kvp.Value.表示レーンの幅dpx.Add( lane, (float) 設定[ "幅" ][ lane.ToString() ] );
                    }

                    kvp.Value.レーンライン = new List<RectangleF>();

                    foreach( var rcline in 設定[ "レーンライン" ] )
                        kvp.Value.レーンライン.Add( FDKUtilities.JsonToRectangleF( rcline ) );

                    kvp.Value.レーン色 = new Color4( Convert.ToUInt32( (string) 設定[ "レーン色" ], 16 ) );
                    kvp.Value.レーンライン色 = new Color4( Convert.ToUInt32( (string) 設定[ "レーンライン色" ], 16 ) );

                    Log.Info( $"{new VariablePath( 設定ファイルパス ).変数付きパス} ... 完了" );
                }

                レーン配置を設定する( "TypeA" );
            }
        }
        public static void レーン配置を設定する( string レーン配置名 )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                if( レーン配置リスト.ContainsKey( レーン配置名 ) )
                {
                    _現在のレーン配置 = レーン配置リスト[ レーン配置名 ];
                }
                else
                {
                    Log.ERROR( $"指定されたレーン配置名「{レーン配置名}」が存在しません。" );

                    if( 0 < レーン配置リスト.Count )
                    {
                        _現在のレーン配置 = レーン配置リスト.ElementAt( 0 ).Value;
                        Log.WARNING( $"既定のレーン配置名「{_現在のレーン配置.配置名}」を選択しました。" );
                    }
                    else
                    {
                        throw new Exception( "既定のレーン配置名を選択しようとしましたが、存在しません。" );
                    }
                }

                Log.Info( $"レーン配置「{_現在のレーン配置.配置名}」を選択しました。" );
            }
        }

        public void 描画する( DeviceContext1 dc )
        {
            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                // レーンエリアを描画する。
                using( var laneBrush = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, 現在のレーン配置.レーン色 ) )
                {
                    dc.FillRectangle( レーンフレーム.領域, laneBrush );
                }

                // レーンラインを描画する。
                using( var laneLineBrush = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, 現在のレーン配置.レーンライン色 ) )
                {
                    for( int i = 0; i < 現在のレーン配置.レーンライン.Count; i++ )
                    {
                        var rc = 現在のレーン配置.レーンライン[ i ];
                        rc.Left += レーンフレーム.領域.Left;
                        rc.Right += レーンフレーム.領域.Left;
                        dc.FillRectangle( rc, laneLineBrush );
                    }
                }

            } );
        }

        private static レーン配置 _現在のレーン配置;
    }
}
