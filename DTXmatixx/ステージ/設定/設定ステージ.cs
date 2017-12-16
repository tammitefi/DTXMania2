using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
			this.子リスト.Add( this._背景画像 = new 舞台画像() );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
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
		}

		private 舞台画像 _背景画像 = null;
	}
}
