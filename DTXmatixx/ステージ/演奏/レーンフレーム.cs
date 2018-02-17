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
        {
            get;
            protected set;
        } = new RectangleF( 445f, 0f, 778f, 938f );

        /// <summary>
        ///		表示レーンの左端X位置を、レーンフレームの左端からの相対位置で示す。
        /// </summary>
        public static Dictionary<表示レーン種別, float> 表示レーンの左端位置dpx
        {
            get;
            protected set;
        }

        /// <summary>
        ///		表示レーンの幅。
        /// </summary>
        public static Dictionary<表示レーン種別, float> 表示レーンの幅dpx
        {
            get;
            protected set;
        }

        public static void 初期化する()
        {
            表示レーンの左端位置dpx = new Dictionary<表示レーン種別, float>();
            表示レーンの幅dpx = new Dictionary<表示レーン種別, float>();

            var jobj = JObject.Parse( File.ReadAllText( new VariablePath( @"$(System)images\演奏画面_レーンフレーム.json" ).変数なしパス ) );

            foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
            {
                表示レーンの左端位置dpx.Add( lane, (float) jobj[ "左端位置" ][ lane.ToString() ] );
                表示レーンの幅dpx.Add( lane, (float) jobj[ "幅" ][ lane.ToString() ] );
            }

            _レーンライン = new List<RectangleF>();

            foreach( var rcline in jobj[ "レーンライン" ] )
                _レーンライン.Add( FDKUtilities.JsonToRectangleF( rcline ) );

            _レーン色 = new Color4( Convert.ToUInt32( (string) jobj[ "レーン色" ], 16 ) );
            _レーンライン色 = new Color4( Convert.ToUInt32( (string) jobj[ "レーンライン色" ], 16 ) );
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._レーン色ブラシ = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, _レーン色 );
                this._レーンライン色ブラシ = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, _レーンライン色 );
            }
        }
        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                FDKUtilities.解放する( ref this._レーンライン色ブラシ );
                FDKUtilities.解放する( ref this._レーン色ブラシ );
            }
        }

        public void 描画する( DeviceContext1 dc )
        {
            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                // レーンエリアを描画する。
                dc.FillRectangle( レーンフレーム.領域, this._レーン色ブラシ );

                // レーンラインを描画する。
                for( int i = 0; i < _レーンライン.Count; i++ )
                {
                    var rc = _レーンライン[ i ];
                    rc.Left += レーンフレーム.領域.Left;
                    rc.Right += レーンフレーム.領域.Left;
                    dc.FillRectangle( rc, _レーンライン色ブラシ );
                }

            } );
        }

        /// <summary>
        ///		レーンラインの領域。
        ///		<see cref="レーンフレーム.領域"/>.Left からの相対値[dpx]。
        /// </summary>
        private static List<RectangleF> _レーンライン;

        private static Color4 _レーン色;
        private static Color4 _レーンライン色;
        private SolidColorBrush _レーン色ブラシ = null;
        private SolidColorBrush _レーンライン色ブラシ = null;
    }
}
