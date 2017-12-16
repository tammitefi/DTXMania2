using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.設定
{
	class パネルリスト : Activity
	{
		/// <summary>
		///		パネルの識別に <see cref="パネル.パネル名"/> を使うので、
		///		ここで一元的に定義しておく。
		/// </summary>
		public class 項目名
		{
			public static readonly string 画面モード = "画面モード";
			public static readonly string 譜面スピード = "譜面スピード";
			public static readonly string シンバルフリー = "シンバルフリー";
			public static readonly string 自動演奏 = "自動演奏";
			public static readonly string 設定完了 = "設定完了";
		}

		public パネル 現在選択中のパネル
			=> this._パネルリスト[ this._選択パネル番号 ];

		public パネルリスト()
		{
			this.子リスト.Add( this._青い線 = new 青い線() );
		}

		public void 前のパネルを選択する()
		{
			this._選択パネル番号 = ( this._選択パネル番号 - 1 + this._パネルリスト.Count ) % this._パネルリスト.Count;
		}
		public void 次のパネルを選択する()
		{
			this._選択パネル番号 = ( this._選択パネル番号 + 1 ) % this._パネルリスト.Count;
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				var user = App.ユーザ管理.ログオン中のユーザ;

				this._パネルリスト = new List<パネル>() {

					// from ユーザ設定

					new パネル_文字列リスト(
						項目名.画面モード, 
						( user.全画面モードである ) ? 1 : 0,
						new[] { "ウィンドウ", "全画面" },
						new Action<パネル>( ( panel ) => {
							user.全画面モードである = ( 1 == ( (パネル_文字列リスト)panel).現在選択されている選択肢の番号 );
							App.Instance.全画面モード = user.全画面モードである;
						} ) ),

					new パネル_譜面スピード( 項目名.譜面スピード ),

					new パネル_文字列リスト(
						項目名.シンバルフリー,
						( user.シンバルフリーモードである ) ? 1 : 0,
						new[] { "OFF", "ON" },
						new Action<パネル>( ( panel ) => {
							user.シンバルフリーモードである = ( 1 == ( (パネル_文字列リスト)panel).現在選択されている選択肢の番号 );
						} ) ),

					new パネル_文字列リスト(
						項目名.自動演奏,
						( user.AutoPlayがすべてONである ) ? 1 : 0,
						new[] { "All OFF", "All ON" },
						new Action<パネル>( ( panel ) => {
							bool on = ( 1 == ( (パネル_文字列リスト)panel).現在選択されている選択肢の番号 );
							if( on )
							{
								foreach( DTXmatixx.設定.AutoPlay種別 play in Enum.GetValues( typeof( DTXmatixx.設定.AutoPlay種別 ) ) )
									user.AutoPlay[ play ] = true;
							}
							else
							{
								foreach( DTXmatixx.設定.AutoPlay種別 play in Enum.GetValues( typeof( DTXmatixx.設定.AutoPlay種別 ) ) )
									user.AutoPlay[ play ] = false;
							}
						} ) ),

					new パネル_システムボタン( 項目名.設定完了 ),
				};

				foreach( var panel in this._パネルリスト )
					this.子リスト.Add( panel );

				this._選択パネル番号 = 0;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				foreach( var panel in this._パネルリスト )
					this.子リスト.Remove( panel );

				this._パネルリスト = null;
			}
		}

		public void 進行描画する( グラフィックデバイス gd, float left, float top )
		{
			const float パネルの下マージン = 4f;
			float パネルの高さ = パネル.サイズ.Height + パネルの下マージン;

			// フレーム１（たて線）を描画。
			this._青い線.描画する( gd, new Vector2( left, 0f ), 高さdpx: gd.設計画面サイズ.Height );

			// パネルを描画。（選択中のパネルの3つ上から7つ下まで、計11枚。）
			for( int i = 0; i < 11; i++ )
			{
				int 描画パネル番号 = ( ( this._選択パネル番号 - 3 + i ) + this._パネルリスト.Count ) % this._パネルリスト.Count;
				var 描画パネル = this._パネルリスト[ 描画パネル番号 ];

				描画パネル.進行描画する(
					gd,
					left + 22f,
					top + i * パネルの高さ,
					選択中: ( i == 3 ) );
			}

			// フレーム２（選択パネル周囲）を描画。
			float 幅 = パネル.サイズ.Width + 22f * 2f;
			this._青い線.描画する( gd, new Vector2( left, パネルの高さ * 3f ), 幅dpx: 幅 );
			this._青い線.描画する( gd, new Vector2( left, パネルの高さ * 4f ), 幅dpx: 幅 );
			this._青い線.描画する( gd, new Vector2( left + 幅, パネルの高さ * 3f ), 高さdpx: パネルの高さ );
		}

		private 青い線 _青い線 = null;
		private List<パネル> _パネルリスト = null;
		private int _選択パネル番号 = 0;
	}
}
