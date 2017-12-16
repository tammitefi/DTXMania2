using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.設定
{
	/// <summary>
	///		システム制御用のボタン。「設定完了」など。
	///		描画が普通のパネルと違う。
	/// </summary>
	class パネル_システムボタン : パネル
	{
		public パネル_システムボタン( string パネル名 )
			: base( パネル名, null )
		{
			this._パネル名画像.前景色 = Color.Black;
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
		}

		public override void 進行描画する( グラフィックデバイス gd, float left, float top, bool 選択中 )
		{
			var テキスト矩形 = new RectangleF( left + 32f, top + 12f, 294f, 72f );

			gd.D2DBatchDraw( ( dc ) => {

				using( var テキスト背景色 = new SolidColorBrush( dc, Color.LightGray ) )
				{
					dc.FillRectangle( テキスト矩形, テキスト背景色 );
				}

			} );

			float 拡大X = Math.Min( 1f, ( テキスト矩形.Width - 20f ) / this._パネル名画像.サイズ.Width );    // -20 は左右マージンの最低値[dpx]
			this._パネル名画像.描画する(
				gd,
				テキスト矩形.Left + ( テキスト矩形.Width - this._パネル名画像.サイズ.Width * 拡大X ) / 2f,
				テキスト矩形.Top + ( テキスト矩形.Height - this._パネル名画像.サイズ.Height ) / 2f,
				X方向拡大率: 拡大X );
		}
	}
}
