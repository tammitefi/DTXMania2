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
	///		すべてのパネルのベースとなるクラス。
	/// </summary>
	class パネル : Activity
	{
		public string パネル名
		{
			get;
			protected set;
		} = "";

		/// <summary>
		///		パネル全体のサイズ。static。
		/// </summary>
		public static Size2F サイズ
			=> new Size2F( 642f, 96f );

		public パネル( string パネル名, Action<パネル> 値の変更処理 )
		{
			this.パネル名 = パネル名;
			this._値の変更処理 = 値の変更処理;

			this.子リスト.Add( this._パネル名画像 = new 文字列画像() { 表示文字列 = this.パネル名, フォントサイズpt = 34f, 前景色 = Color4.White } );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
		}

		public virtual void 確定キーが入力された()
		{
			// 必要あれば、派生クラスで実装すること。

			this._値の変更処理?.Invoke( this );
		}
		public virtual void 左移動キーが入力された()
		{
			// 必要あれば、派生クラスで実装すること。

			this._値の変更処理?.Invoke( this );
		}
		public virtual void 右移動キーが入力された()
		{
			// 必要あれば、派生クラスで実装すること。

			this._値の変更処理?.Invoke( this );
		}

		public virtual void 進行描画する( グラフィックデバイス gd, float left, float top, bool 選択中 )
		{
			var パネル矩形 = new RectangleF( left, top, サイズ.Width, サイズ.Height );
			var ヘッダ矩形 = new RectangleF( left, top, 40f, 96f );
			var テキスト矩形 = new RectangleF( left + 20f, top + 10f, 280f, 76f );

			if( 選択中 )
			{
				// 選択パネルは、パネル矩形を左右にちょっと大きくする。
				パネル矩形.Left -= 38f;
				パネル矩形.Width += 38f * 2f;
			}

			gd.D2DBatchDraw( ( dc ) => {

				using( var パネル背景色 = new SolidColorBrush( dc, new Color4( Color3.Black, 0.5f ) ) )
				using( var ヘッダ背景色 = new SolidColorBrush( dc, new Color4( 0xff725031 ) ) )   // ABGR
				using( var テキスト背景色 = new SolidColorBrush( dc, Color4.Black ) )
				{
					dc.FillRectangle( パネル矩形, パネル背景色 );
					dc.FillRectangle( ヘッダ矩形, ヘッダ背景色 );
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

		protected 文字列画像 _パネル名画像 = null;
		protected Action<パネル> _値の変更処理 = null;

		/// <summary>
		///		項目部分のサイズ。
		///		left と top は、パネルほ left,top からの相対値。
		/// </summary>
		protected RectangleF 項目領域
			=> new RectangleF( +322f, +0f, 342f, サイズ.Height );
	}
}
