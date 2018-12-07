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

namespace DTXmatixx.ステージ
{
    class パッド矢印 : Activity
    {
        public enum 種類 { 上_Tom1, 下_Tom2, 左_Snare, 右_Tom3 };

        public パッド矢印()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子を追加する( this._矢印画像 = new 画像( @"$(System)images\パッド矢印.png" ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._矢印の矩形リスト = JObject.Parse( File.ReadAllText( new VariablePath( @"$(System)images\パッド矢印.json" ).変数なしパス ) );
            }
        }
        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        public void 描画する( DeviceContext1 dc, 種類 type, Vector2 中央位置dpx, float 拡大率 = 1f )
        {
            var 矩形 = new RectangleF();

            switch( type )
            {
                case 種類.上_Tom1:  矩形 = FDKUtilities.JsonToRectangleF( this._矢印の矩形リスト[ "矩形リスト" ][ "Up" ] ); break;
                case 種類.下_Tom2:  矩形 = FDKUtilities.JsonToRectangleF( this._矢印の矩形リスト[ "矩形リスト" ][ "Down" ] ); break;
                case 種類.左_Snare: 矩形 = FDKUtilities.JsonToRectangleF( this._矢印の矩形リスト[ "矩形リスト" ][ "Left" ] ); break;
                case 種類.右_Tom3:  矩形 = FDKUtilities.JsonToRectangleF( this._矢印の矩形リスト[ "矩形リスト" ][ "Right" ] ); break;
            }

            var 左上位置dpx = new Vector2( 中央位置dpx.X - 矩形.Width * 拡大率 / 2f, 中央位置dpx.Y - 矩形.Height * 拡大率 / 2f );

            var 変換行列 =
                Matrix3x2.Scaling( 拡大率 ) *
                Matrix3x2.Translation( 左上位置dpx );

            this._矢印画像.描画する( dc, 変換行列, 転送元矩形: 矩形 );
        }

        private 画像 _矢印画像 = null;
        private JObject _矢印の矩形リスト = null;
    }
}
