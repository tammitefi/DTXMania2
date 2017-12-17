using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using FDK;
using FDK.メディア;
using DTXmatixx.設定;

namespace DTXmatixx.ステージ.設定
{
	class パネルリスト : Activity
	{
		/// <summary>
		///		パネルの識別に <see cref="パネル.パネル名"/> を使う も の だ け、
		///		ここで一元的に定義しておく。
		/// </summary>
		public class 項目名
		{
			public static readonly string 設定完了 = "設定完了";
			public static readonly string 設定完了_戻る = "設定完了（戻る）";
		}

		public パネル 現在選択中のパネル
			=> this._現在のパネルフォルダ.子パネルリスト.SelectedItem;

		public パネルリスト()
		{
			this.子リスト.Add( this._青い線 = new 青い線() );
			this.子リスト.Add( this._パッド矢印 = new パッド矢印() );
			this.子リスト.Add( this._ルートパネルフォルダ = new パネル_フォルダ( "root", null, null ) );

			this._現在のパネルフォルダ = this._ルートパネルフォルダ;
		}

		public void フェードインを開始する( グラフィックデバイス gd, double 速度倍率 = 1.0 )
		{
			for( int i = 0; i < this._現在のパネルフォルダ.子パネルリスト.Count; i++ )
			{
				this._現在のパネルフォルダ.子パネルリスト[ i ].フェードインを開始する( gd, 0.02, 速度倍率 );
			}
		}
		public void フェードアウトを開始する( グラフィックデバイス gd, double 速度倍率 = 1.0 )
		{
			for( int i = 0; i < this._現在のパネルフォルダ.子パネルリスト.Count; i++ )
			{
				this._現在のパネルフォルダ.子パネルリスト[ i ].フェードアウトを開始する( gd, 0.02, 速度倍率 );
			}
		}

		public void 前のパネルを選択する()
		{
			Trace.Assert( null != this._現在のパネルフォルダ?.子パネルリスト );

			this._現在のパネルフォルダ.子パネルリスト.SelectPrev( Loop: true );
		}
		public void 次のパネルを選択する()
		{
			Trace.Assert( null != this._現在のパネルフォルダ?.子パネルリスト );

			this._現在のパネルフォルダ.子パネルリスト.SelectNext( Loop: true );
		}
		public void 親のパネルを選択する()
		{
			Trace.Assert( null != this._現在のパネルフォルダ?.親パネル );

			this._現在のパネルフォルダ = this._現在のパネルフォルダ.親パネル;
		}
		public void 子のパネルを選択する()
		{
			Trace.Assert( null != this._現在のパネルフォルダ?.子パネルリスト?.SelectedItem );

			this._現在のパネルフォルダ = this._現在のパネルフォルダ.子パネルリスト.SelectedItem as パネル_フォルダ;
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				var user = App.ユーザ管理.ログオン中のユーザ;
				var autoplay = (パネル_フォルダ) null;

				this._ルートパネルフォルダ.子パネルリスト = new SelectableList<パネル>() {

					#region " 画面モード "
					//----------------
					new パネル_文字列リスト(
						"画面モード",
						( user.全画面モードである ) ? 1 : 0,
						new[] { "ウィンドウ", "全画面" },
						new Action<パネル>( ( panel ) => {
							user.全画面モードである = ( 1 == ( (パネル_文字列リスト)panel).現在選択されている選択肢の番号 );
							App.Instance.全画面モード = user.全画面モードである;
						} ) ),
					//----------------
					#endregion

					#region " 譜面スピード "
					//----------------
					new パネル_譜面スピード( "譜面スピード" ),
					//----------------
					#endregion

					#region " シンバルフリー "
					//----------------
					new パネル_ONOFFトグル(
						"シンバルフリー",
						user.シンバルフリーモードである,
						new Action<パネル>( (panel) => {
							user.シンバルフリーモードである = ( (パネル_ONOFFトグル) panel ).ONである;
						} ) ),
					//----------------
					#endregion

					#region " 自動演奏（フォルダ）"
					//----------------
					( autoplay = new パネル_フォルダ( "自動演奏", this._ルートパネルフォルダ ) {	// 子パネルリストの設定は後で。
						ヘッダ色 = パネル.ヘッダ色種別.赤,
					} ),
					//----------------
					#endregion

					#region " 設定完了（システムボタン）"
					//----------------
					new パネル_システムボタン( 項目名.設定完了 ),
					//----------------
					#endregion

				};

				#region " 自動演奏フォルダの子の設定。"
				//----------------
				autoplay.子パネルリスト = new SelectableList<パネル>();

				foreach( AutoPlay種別 apType in Enum.GetValues( typeof( AutoPlay種別 ) ) )
				{
					if( apType == AutoPlay種別.Unknown )
						continue;

					autoplay.子パネルリスト.Add(
						new パネル_ONOFFトグル(
							apType.ToString(),
							( user.AutoPlay[ apType ] ),
							new Action<パネル>( ( panel ) => {
								user.AutoPlay[ apType ] = ( (パネル_ONOFFトグル) panel ).ONである;
							} ) ) );
				}

				autoplay.子パネルリスト.Add( new パネル_システムボタン( 項目名.設定完了_戻る ) );
				autoplay.子パネルリスト.SelectFirst();
				//----------------
				#endregion

				this._現在のパネルフォルダ = this._ルートパネルフォルダ;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._現在のパネルフォルダ = null;	// 他の実体を参照してるだけなので Dispose 不要。
			}
		}

		public void 進行描画する( グラフィックデバイス gd, float left, float top )
		{
			const float パネルの下マージン = 4f;
			float パネルの高さ = パネル.サイズ.Height + パネルの下マージン;

			// フレーム１（たて線）を描画。
			this._青い線.描画する( gd, new Vector2( left, 0f ), 高さdpx: gd.設計画面サイズ.Height );

			// パネルを描画。（選択中のパネルの3つ上から7つ下まで、計11枚。）
			var panels = this._現在のパネルフォルダ.子パネルリスト;
			for( int i = 0; i < 11; i++ )
			{
				int 描画パネル番号 = ( ( panels.SelectedIndex - 3 + i ) + panels.Count * 3 ) % panels.Count;		// panels の末尾に達したら先頭に戻る。
				var 描画パネル = panels[ 描画パネル番号 ];

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

			// パッド矢印（上＆下）を描画。
			this._パッド矢印.描画する( gd, パッド矢印.種類.上_Tom1, new Vector2( left, パネルの高さ * 3f ) );
			this._パッド矢印.描画する( gd, パッド矢印.種類.下_Tom2, new Vector2( left, パネルの高さ * 4f ) );
		}

		private パネル_フォルダ _ルートパネルフォルダ = null;
		private パネル_フォルダ _現在のパネルフォルダ = null;

		private 青い線 _青い線 = null;
		private パッド矢印 _パッド矢印 = null;
	}
}
