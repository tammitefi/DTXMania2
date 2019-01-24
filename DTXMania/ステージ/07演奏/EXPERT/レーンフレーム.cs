using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.ステージ.演奏.EXPERT
{
    /// <summary>
    ///		チップの背景であり、レーン全体を示すフレーム画像。
    /// </summary>
    class レーンフレーム : Activity
    {
        /// <summary>
        ///		画面全体に対する、レーンフレームの表示位置と範囲。
        /// </summary>
        public static RectangleF 領域 => new RectangleF( 445f, 0f, 778f, 938f );


        public レーンフレーム()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 設定ファイルパス = new VariablePath( @"$(System)images\演奏\レーン配置\Expert.yaml" );

                var yaml = File.ReadAllText( 設定ファイルパス.変数なしパス );
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                var yamlMap = deserializer.Deserialize<YAMLマップ>( yaml );

                this._レーン中央位置X = new Dictionary<表示レーン種別, float>();
                this._レーン色 = new Dictionary<表示レーン種別, Color4>();
                foreach( 表示レーン種別 displayLaneType in Enum.GetValues( typeof( 表示レーン種別 ) ) )
                {
                    this._レーン中央位置X[ displayLaneType ] = yamlMap.中央位置[ displayLaneType ];
                    this._レーン色[ displayLaneType ] = new Color4( Convert.ToUInt32( yamlMap.色[ displayLaneType ], 16 ) );
                }
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        public void 描画する( DeviceContext1 dc, int BGAの透明度 )
        {
            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                // レーンエリアを描画する。

                var レーン色 = Color4.Black;
                レーン色.Alpha *= ( 100 - BGAの透明度 ) / 100.0f;   // BGAの透明度0→100 のとき Alpha×1→×0
                using( var laneBrush = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, レーン色 ) )
                {
                    dc.FillRectangle( レーンフレーム.領域, laneBrush );
                }


                // レーンラインを描画する。

                foreach( 表示レーン種別 displayLaneType in Enum.GetValues( typeof( 表示レーン種別 ) ) )
                {
                    var レーンライン色 = this._レーン色[ displayLaneType ];
                    レーンライン色.Alpha *= ( 100 - BGAの透明度 ) / 100.0f;   // BGAの透明度0→100 のとき Alpha×1→×0

                    using( var laneLineBrush = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, レーンライン色 ) )
                    {
                        var rc = new RectangleF( this._レーン中央位置X[ displayLaneType ] - 1, 0f, 3f, 領域.Height );
                        dc.FillRectangle( rc, laneLineBrush );
                    }
                }

            } );
        }


        private Dictionary<表示レーン種別, float> _レーン中央位置X;
        private Dictionary<表示レーン種別, Color4> _レーン色;


        private class YAMLマップ
        {
            public Dictionary<表示レーン種別, float> 中央位置 { get; set; }
            public Dictionary<表示レーン種別, string> 色 { get; set; }
        }
    }
}
