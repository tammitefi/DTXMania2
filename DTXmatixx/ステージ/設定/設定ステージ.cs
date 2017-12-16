using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.設定
{
	class 設定ステージ : ステージ
	{
		public enum フェーズ
		{
			フェードイン,
			表示,
			フェードアウト,
			確定,
			キャンセル,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public 設定ステージ()
		{
			this.子リスト.Add( this._舞台画像 = new 舞台画像() );
			this.子リスト.Add( this._パネルリスト = new パネルリスト() );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.現在のフェーズ = フェーズ.フェードイン;
				this._初めての進行描画 = true;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
			}
		}

		public override void 進行描画する( グラフィックデバイス gd )
		{
			// 進行描画。

			if( this._初めての進行描画 )
			{
				this._舞台画像.ぼかしと縮小を適用する( gd, 0.5 );
				this._初めての進行描画 = false;
			}

			this._舞台画像.進行描画する( gd );
			this._パネルリスト.進行描画する( gd, 613f, 0f );

			switch( this.現在のフェーズ )
			{
				case フェーズ.フェードイン:
					this._パネルリスト.フェードインを開始する( gd );
					this.現在のフェーズ = フェーズ.表示;
					break;

				case フェーズ.表示:
					break;

				case フェーズ.フェードアウト:

					App.ステージ管理.現在のアイキャッチ.進行描画する( gd );

					if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.クローズ完了 )
					{
						this.現在のフェーズ = フェーズ.確定;
					}
					break;

				case フェーズ.確定:
				case フェーズ.キャンセル:
					break;
			}

			// 入力。

			App.入力管理.すべての入力デバイスをポーリングする();

			switch( this.現在のフェーズ )
			{
				case フェーズ.表示:

					if( App.入力管理.キャンセルキーが入力された() )
					{
						App.ステージ管理.アイキャッチを選択しクローズする( gd, nameof( アイキャッチ.半回転黒フェード ) );
						this.現在のフェーズ = フェーズ.フェードアウト;
					}
					else if( App.入力管理.上移動キーが入力された() )
					{
						this._パネルリスト.前のパネルを選択する();
					}
					else if( App.入力管理.下移動キーが入力された() )
					{
						this._パネルリスト.次のパネルを選択する();
					}
					else if( App.入力管理.左移動キーが入力された() )
					{
						// パネル側で何か処理が必要なら行う。
						this._パネルリスト.現在選択中のパネル.左移動キーが入力された();
					}
					else if( App.入力管理.右移動キーが入力された() )
					{
						// パネル側で何か処理が必要なら行う。
						this._パネルリスト.現在選択中のパネル.右移動キーが入力された();
					}
					else if( App.入力管理.確定キーが入力された() )
					{
						if( this._パネルリスト.現在選択中のパネル is パネル_システムボタン systemButton )
						{
							// (A) システムボタンの場合。
							if( systemButton.パネル名 == パネルリスト.項目名.設定完了 )
							{
								App.ステージ管理.アイキャッチを選択しクローズする( gd, nameof( アイキャッチ.シャッター ) );
								this.現在のフェーズ = フェーズ.フェードアウト;
							}
						}
						else
						{
							// (B) その他の場合 → パネル側で何か処理が必要なら行う。
							this._パネルリスト.現在選択中のパネル.確定キーが入力された();
						}
					}
					break;
			}
		}

		private bool _初めての進行描画 = true;
		private 舞台画像 _舞台画像 = null;
		private パネルリスト _パネルリスト = null;
	}
}
