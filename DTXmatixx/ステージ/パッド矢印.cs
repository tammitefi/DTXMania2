using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ
{
    class パッド矢印 : Activity
    {
        public enum 種類 { 上_Tom1, 下_Tom2, 左_Snare, 右_Tom3 };

        public パッド矢印()
        {
            this.子リスト.Add( this._矢印画像 = new 画像( @"$(System)images\パッド矢印.png" ) );
        }

        protected override void On活性化( グラフィックデバイス gd )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._矢印の矩形リスト = new 矩形リスト( @"$(System)images\パッド矢印矩形.xml" );
            }
        }
        protected override void On非活性化( グラフィックデバイス gd )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        public void 描画する( グラフィックデバイス gd, DeviceContext1 dc, 種類 type, Vector2 中央位置dpx, float 拡大率 = 1f )
        {
            var 矩形 = new RectangleF();

            switch( type )
            {
                case 種類.上_Tom1:  矩形 = this._矢印の矩形リスト[ "Up" ].Value; break;
                case 種類.下_Tom2:  矩形 = this._矢印の矩形リスト[ "Down" ].Value; break;
                case 種類.左_Snare: 矩形 = this._矢印の矩形リスト[ "Left" ].Value; break;
                case 種類.右_Tom3:  矩形 = this._矢印の矩形リスト[ "Right" ].Value; break;
            }

            var 左上位置dpx = new Vector2( 中央位置dpx.X - 矩形.Width * 拡大率 / 2f, 中央位置dpx.Y - 矩形.Height * 拡大率 / 2f );

            var 変換行列 =
                Matrix3x2.Scaling( 拡大率 ) *
                Matrix3x2.Translation( 左上位置dpx );

            this._矢印画像.描画する( gd, dc, 変換行列, 転送元矩形: 矩形 );
        }

        private 画像 _矢印画像 = null;
        private 矩形リスト _矢印の矩形リスト = null;
    }
}
