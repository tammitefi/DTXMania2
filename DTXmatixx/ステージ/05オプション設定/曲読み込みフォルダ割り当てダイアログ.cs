using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;
using FDK.入力;
using DTXmatixx.設定;
using DTXmatixx.入力;

namespace DTXmatixx.ステージ.オプション設定
{
    public partial class 曲読み込みフォルダ割り当てダイアログ : Form
    {
        public 曲読み込みフォルダ割り当てダイアログ()
        {
            InitializeComponent();

            // 物理画面のサイズに応じて、フォームのサイズを変更。
            //this.Scale(
            //    new System.Drawing.SizeF(
            //        Math.Max( 1f, グラフィックデバイス.Instance.拡大率DPXtoPX横 ),
            //        Math.Max( 1f, グラフィックデバイス.Instance.拡大率DPXtoPX縦 ) ) );
        }
        public bool 表示する()
        {
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				bool 変更された = false;

				// メインウィンドウ用の入力管理をいったん破棄し、このウィンドウ用の入力管理を生成する。
				App.入力管理.Dispose();
				using( var 入力管理 = new 入力管理( this.Handle ) )
				{
					入力管理.キーバインディングを取得する = () => App.システム設定.キーバインディング;
					入力管理.キーバインディングを保存する = () => App.システム設定.保存する();
					入力管理.初期化する();

					#region " 現在の設定で初期値。"
					//----------------
					foreach( var vpath in App.システム設定.曲検索フォルダ )
						this.listViewフォルダ一覧.Items.Add( new ListViewItem( $"{vpath.変数なしパス}" ) );	// ここでは変数なしでパスを表示する。

					this._変更あり = false;
					//----------------
					#endregion

					var dr = DialogResult.OK;

					#region " ダイアログの表示から終了まで "
					//----------------
					Cursor.Show();

					dr = this.ShowDialog( App.Instance );

					if( App.Instance.全画面モード )
						Cursor.Hide();
					//----------------
					#endregion

					if( dr == DialogResult.OK )
					{
						#region " 変更後の設定を保存。"
						//----------------
						App.システム設定.曲検索フォルダ.Clear();
						foreach( ListViewItem item in this.listViewフォルダ一覧.Items )
							App.システム設定.曲検索フォルダ.Add( new VariablePath( item.SubItems[ 0 ].Text ) );

						App.システム設定.保存する();
						//----------------
						#endregion

						変更された = true;
					}
					else
					{
						Log.Info( "キャンセルされました。" );
						変更された = false;
					}
				}

				// メインウィンドウ用の入力管理を復活。
				App.入力管理 = new 入力管理( App.Instance.Handle ) {
					キーバインディングを取得する = () => App.システム設定.キーバインディング,
					キーバインディングを保存する = () => App.システム設定.保存する(),
				};
				App.入力管理.初期化する();

				return 変更された;
			}
        }

        private bool _変更あり;
        private void _FormClosing( object sender, FormClosingEventArgs e )
        {
            // ※ウィンドウを閉じようとした時も Cancel になる。
            if( this.DialogResult == DialogResult.Cancel && this._変更あり )
            {
                var dr = MessageBoxEx.Show( "変更を破棄していいですか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2 );

                if( dr == DialogResult.No )
                    e.Cancel = true;
            }
        }
		private void listViewフォルダ一覧_SelectedIndexChanged( object sender, EventArgs e )
		{
			if( 0 < this.listViewフォルダ一覧.SelectedItems.Count )
			{
				// フォルダが選択された
				this.button削除.Enabled = true;
			}
			else
			{
				// フォルダが１つも選択されていない
				this.button削除.Enabled = false;
			}
		}
		private void button選択_Click( object sender, EventArgs e )
		{
			// フォルダ選択ダイアログを生成する。
			using( var dialog = new CommonOpenFileDialog( "曲読み込みフォルダの選択" ) {
				IsFolderPicker = true,  // ファイルじゃなくフォルダを開く
				EnsureReadOnly = true,
				AllowNonFileSystemItems = false,
			} )
			{
				// ダイアログの表示から終了。
				if( dialog.ShowDialog( this.Handle ) == CommonFileDialogResult.Ok )
				{
					// 選択されたフォルダを曲読み込みフォルダリストに追加する。
					foreach( var name in dialog.FileNames )
					{
						var vpath = new VariablePath( name );
						this.listViewフォルダ一覧.Items.Add( new ListViewItem( $"{vpath.変数なしパス}" ) );
						Log.Info( $"曲読み込みフォルダを追加しました。[{vpath.変数付きパス}]" );
					}

					this.listViewフォルダ一覧.Refresh();
				}
			}
		}
		private void button削除_Click( object sender, EventArgs e )
		{
			foreach( int selectedIndex in this.listViewフォルダ一覧.SelectedIndices )
				this.listViewフォルダ一覧.Items.RemoveAt( selectedIndex );

			this.listViewフォルダ一覧.Refresh();
		}
	}
}
