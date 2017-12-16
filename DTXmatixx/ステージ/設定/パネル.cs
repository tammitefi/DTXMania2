using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
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

		public void フェードインを開始する( グラフィックデバイス gd, double 遅延sec, double 速度倍率 = 1.0 )
		{
			Trace.Assert( this.活性化している );

			double 秒( double v ) => ( v / 速度倍率 );

			this._パネルの高さ割合?.Dispose();
			this._パネルの高さ割合 = new Variable( gd.Animation.Manager, initialValue: 1.0 );

			this._パネルのストーリーボード?.Abandon();
			this._パネルのストーリーボード?.Dispose();
			this._パネルのストーリーボード = new Storyboard( gd.Animation.Manager );

			using( var 遅延繊維 = gd.Animation.TrasitionLibrary.Constant( duration: 秒( 遅延sec ) ) )
			using( var 縮む繊維 = gd.Animation.TrasitionLibrary.Linear( duration: 秒( 0.1 ), finalValue: 0.0 ) )
			using( var 膨らむ繊維 = gd.Animation.TrasitionLibrary.Linear( duration: 秒( 0.1 ), finalValue: 1.0 ) )
			{
				this._パネルのストーリーボード.AddTransition( this._パネルの高さ割合, 遅延繊維 );
				this._パネルのストーリーボード.AddTransition( this._パネルの高さ割合, 縮む繊維 );
				this._パネルのストーリーボード.AddTransition( this._パネルの高さ割合, 膨らむ繊維 );
			}
			this._パネルのストーリーボード.Schedule( gd.Animation.Timer.Time );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			this._パネルの高さ割合 = new Variable( gd.Animation.Manager, initialValue: 1.0 );
			this._パネルのストーリーボード = null;
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			this._パネルのストーリーボード?.Abandon();
			FDKUtilities.解放する( ref this._パネルのストーリーボード );
			FDKUtilities.解放する( ref this._パネルの高さ割合 );
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
			float 拡大率Y = (float) this._パネルの高さ割合.Value;

			float パネルとヘッダの上下マージン = サイズ.Height * ( 1f - 拡大率Y ) / 2f;
			float テキストの上下マージン = 76f * ( 1f - 拡大率Y ) / 2f;
			var パネル矩形 = new RectangleF( left, top + パネルとヘッダの上下マージン, サイズ.Width, サイズ.Height * 拡大率Y );
			var ヘッダ矩形 = new RectangleF( left, top + パネルとヘッダの上下マージン, 40f, サイズ.Height * 拡大率Y );
			var テキスト矩形 = new RectangleF( left + 20f, top + 10f + テキストの上下マージン, 280f, 76f * 拡大率Y );

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

			float 拡大率X = Math.Min( 1f, ( テキスト矩形.Width - 20f ) / this._パネル名画像.サイズ.Width );    // -20 は左右マージンの最低値[dpx]

			this._パネル名画像.描画する(
				gd,
				テキスト矩形.Left + ( テキスト矩形.Width - this._パネル名画像.サイズ.Width * 拡大率X ) / 2f,
				テキスト矩形.Top + ( テキスト矩形.Height - this._パネル名画像.サイズ.Height * 拡大率Y ) / 2f,
				X方向拡大率: 拡大率X,
				Y方向拡大率: 拡大率Y );
		}

		protected 文字列画像 _パネル名画像 = null;
		protected Action<パネル> _値の変更処理 = null;

		/// <summary>
		///		項目部分のサイズ。
		///		left と top は、パネルほ left,top からの相対値。
		/// </summary>
		protected RectangleF 項目領域
			=> new RectangleF( +322f, +0f, 342f, サイズ.Height );

		/// <summary>
		///		0.0:ゼロ ～ 1.0:原寸
		/// </summary>
		private Variable _パネルの高さ割合 = null;
		private Storyboard _パネルのストーリーボード = null;
	}
}
