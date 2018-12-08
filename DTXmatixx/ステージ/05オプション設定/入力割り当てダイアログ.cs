using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using SharpDX.DirectInput;
using FDK;
using DTXmatixx.設定;
using DTXmatixx.入力;

namespace DTXmatixx.ステージ.オプション設定
{
    public partial class 入力割り当てダイアログ : Form
    {
        public 入力割り当てダイアログ()
        {
            InitializeComponent();

            // 物理画面のサイズに応じて、フォームのサイズを変更。
            //this.Scale(
            //    new System.Drawing.SizeF(
            //        Math.Max( 1f, グラフィックデバイス.Instance.拡大率DPXtoPX横 ),
            //        Math.Max( 1f, グラフィックデバイス.Instance.拡大率DPXtoPX縦 ) ) );
        }

        public void 表示する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
				// メインウィンドウ用の入力管理をいったん破棄し、このウィンドウ用の入力管理を生成する。
				App.入力管理.Dispose();
				using( var 入力管理 = new 入力管理( this.Handle ) )
				{
					入力管理.キーバインディングを取得する = () => App.システム設定.キーバインディング;
					入力管理.キーバインディングを保存する = () => App.システム設定.保存する();
					入力管理.初期化する();

					using( var timer = new Timer() )
					{
						#region " 設定値から初期化。"
						//----------------
						// パッドリストを初期化。
						foreach( ドラム入力種別 drum in Enum.GetValues( typeof( ドラム入力種別 ) ) )
						{
							if( drum == ドラム入力種別.Unknown ||
								drum == ドラム入力種別.HiHat_Control )
								continue;   // 除外（設定変更不可）

							this.comboBoxパッドリスト.Items.Add( drum.ToString() );
						}

						// 変更後のキーバインディングを、現在の設定値で初期化。
						this._変更後のキーバインディング = (キーバインディング) App.システム設定.キーバインディング.Clone();

						// 最初のパッドを選択し、割り当て済みリストを更新。
						this.comboBoxパッドリスト.SelectedIndex = 0;

						this._前回の入力リスト追加時刻 = QPCTimer.生カウント相対値を秒へ変換して返す( QPCTimer.生カウント );

						this._FootPedal現在値 = 0;
						this.textBoxFootPedal現在値.Text = "0";
						this.textBoxFootPedal最小値.Text = this._変更後のキーバインディング.FootPedal最小値.ToString();
						this.textBoxFootPedal最大値.Text = this._変更後のキーバインディング.FootPedal最大値.ToString();

						this._変更あり = false;

						// 初期メッセージを出力。
						this.listView入力リスト.Items.Add( $"KEYBOARD \"{入力管理.Keyboard.DeviceName}\" の受付を開始しました。" );
						for( int i = 0; i < 入力管理.MidiIn.DeviceName.Count; i++ )
							this.listView入力リスト.Items.Add( $"MIDI IN [{i}] \"{入力管理.MidiIn.DeviceName[ i ]}\" の受付を開始しました。" );
						this.listView入力リスト.Items.Add( "" );
						this.listView入力リスト.Items.Add( "* タイミングクロック信号、アクティブ信号は無視します。" );
						this.listView入力リスト.Items.Add( "* 入力と入力の間が500ミリ秒以上開いた場合は、間に空行を表示します。" );
						this.listView入力リスト.Items.Add( "" );
						this.listView入力リスト.Items.Add( "キーボードまたはMIDI信号を入力してください。" );
						//----------------
						#endregion

						// タイマーイベント＆入力の表示。
						timer.Interval = 100;
						timer.Tick += ( sender, arg ) => {

							#region " (A) キーボードのポーリングと、入力リストへの出力。"
							//----------------
							入力管理.Keyboard.ポーリングする();

							for( int i = 0; i < 入力管理.Keyboard.入力イベントリスト.Count; i++ )
							{
								var ie = 入力管理.Keyboard.入力イベントリスト[ i ];

								if( ie.押された )
								{
									var item = new ListViewItem入力リスト用( InputDeviceType.Keyboard, ie );

									if( ie.Key == (int) Key.Escape )    // 割り当てされてほしくないキーはここへ。
									{
										item.割り当て可能 = false;
									}

									this._一定時間が経っていれば空行を挿入する();

									this.listView入力リスト.Items.Add( item );
									this.listView入力リスト.EnsureVisible( this.listView入力リスト.Items.Count - 1 );
								}
								else if( ie.離された )
								{
									// キーボードについては表示しない。
								}
							}
							//----------------
							#endregion

							#region " (B) Midi入力のポーリングと、入力リストへの出力。"
							//----------------
							入力管理.MidiIn.ポーリングする();

							for( int i = 0; i < 入力管理.MidiIn.入力イベントリスト.Count; i++ )
							{
								var ie = 入力管理.MidiIn.入力イベントリスト[ i ];

								// MidiInChecker の機能もかねて、NoteOFF や ControlChange も一応表示しておく。（割り当てはできない。）

								if( ie.押された && ( 255 == ie.Key ) && ( 4 == ie.Control ) )
								{
									// (A) フットペダルコントロールは、入力リストではなく、専用のUIパーツへ表示。

									if( this._FootPedal現在値 != ie.Velocity )
									{
										// "現在地"
										this._FootPedal現在値 = ie.Velocity;
										this.textBoxFootPedal現在値.Text = this._FootPedal現在値.ToString();

										// "最大値"
										if( this._FootPedal現在値 > this._変更後のキーバインディング.FootPedal最大値 )
										{
											this._変更後のキーバインディング.FootPedal最大値 = this._FootPedal現在値;
											this.textBoxFootPedal最大値.Text = this._変更後のキーバインディング.FootPedal最大値.ToString();
										}

										// "最小値"
										if( this._FootPedal現在値 <= this._変更後のキーバインディング.FootPedal最小値 )
										{
											this._変更後のキーバインディング.FootPedal最小値 = this._FootPedal現在値;
											this.textBoxFootPedal最小値.Text = this._変更後のキーバインディング.FootPedal最小値.ToString();
										}
									}
								}
								else
								{
									// (B) その他は入力リストに表示。

									var item = new ListViewItem入力リスト用( InputDeviceType.MidiIn, ie );

									this._一定時間が経っていれば空行を挿入する();

									this.listView入力リスト.Items.Add( item );
									this.listView入力リスト.EnsureVisible( this.listView入力リスト.Items.Count - 1 );

								}
							}
							//----------------
							#endregion

							#region " MIDI フットペダル開度ゲージの描画 "
							//----------------
							using( var g = pictureBoxFootPedal.CreateGraphics() )
							{
								var 全体矩形 = pictureBoxFootPedal.ClientRectangle;
								var 背景色 = new System.Drawing.SolidBrush( pictureBoxFootPedal.BackColor );
								var 最大値ゲージ色 = System.Drawing.Brushes.LightBlue;
								var ゲージ色 = System.Drawing.Brushes.Blue;

								g.FillRectangle( 背景色, 全体矩形 );

								int 最大値用差分 = (int) ( 全体矩形.Height * ( 1.0 - this._変更後のキーバインディング.FootPedal最大値 / 127.0 ) );
								var 最大値ゲージ矩形 = new System.Drawing.Rectangle(
									全体矩形.X,
									全体矩形.Y + 最大値用差分,
									全体矩形.Width,
									全体矩形.Height - 最大値用差分 );
								g.FillRectangle( 最大値ゲージ色, 最大値ゲージ矩形 );

								int 現在値用差分 = (int) ( 全体矩形.Height * ( 1.0 - this._FootPedal現在値 / 127.0 ) );
								var ゲージ矩形 = new System.Drawing.Rectangle(
									全体矩形.X,
									全体矩形.Y + 現在値用差分,
									全体矩形.Width,
									全体矩形.Height - 現在値用差分 );
								g.FillRectangle( ゲージ色, ゲージ矩形 );
							}
							//----------------
							#endregion
						};

						DialogResult dr;

						#region " ダイアログ表示→終了。"
						//----------------
						timer.Start();

						Cursor.Show();

						dr = this.ShowDialog( App.Instance );

						if( App.Instance.全画面モード )
							Cursor.Hide();

						timer.Stop();
						//----------------
						#endregion

						if( dr == DialogResult.OK )
						{
							#region " 設定値の反映。"
							//----------------
							App.システム設定.キーバインディング = (キーバインディング) this._変更後のキーバインディング.Clone();
							入力管理.キーバインディングを保存する();
							//----------------
							#endregion
						}
					}

					// メインウィンドウ用の入力管理を復活。
					App.入力管理 = new 入力管理( App.Instance.Handle ) {
						キーバインディングを取得する = () => App.システム設定.キーバインディング,
						キーバインディングを保存する = () => App.システム設定.保存する(),
					};
					App.入力管理.初期化する();
				}
            }
        }


        /// <summary>
        ///     <see cref="listView入力リスト"/> 用の ListViewItem 拡張。
        ///     表示テキストのほかに、入力情報も持つ。
        /// </summary>
        private class ListViewItem入力リスト用 : ListViewItem
        {
            public bool 割り当て可能;
            public InputDeviceType deviceType;  // Device種別
            public InputEvent inputEvent;       // DeviceID, key, velocity

            public ListViewItem入力リスト用( InputDeviceType deviceType, InputEvent inputEvent )
            {
                this.割り当て可能 = true;
                this.deviceType = deviceType;
                this.inputEvent = inputEvent;

                if( deviceType == InputDeviceType.Keyboard )
                {
                    this.Text = $"Keyboard, {inputEvent.Key}, '{( (Key) inputEvent.Key ).ToString()}'";
                }
                else if( deviceType == InputDeviceType.MidiIn )
                {
                    if( inputEvent.押された )
                    {
                        if( 255 != inputEvent.Key )
                        {
                            this.Text = $"MidiIn[{inputEvent.DeviceID}], {inputEvent.Extra}, ノートオン, Note={inputEvent.Key}, Velocity={inputEvent.Velocity}";
                            this.割り当て可能 = true;                        // 割り当て可
                            this.ForeColor = System.Drawing.Color.Black;    // 黒
                        }
                        else
                        {
                            this.Text = $"MidiIn[{inputEvent.DeviceID}], {inputEvent.Extra}, コントロールチェンジ, Control={inputEvent.Control}(0x{inputEvent.Control:X2}), Value={inputEvent.Velocity}";
                            this.割り当て可能 = false;                       // 割り当て不可
                            this.ForeColor = System.Drawing.Color.Green;    // 緑
                        }
                    }
                    else if( inputEvent.離された )
                    {
                        this.Text = $"MidiIn[{inputEvent.DeviceID}], {inputEvent.Extra}, ノートオフ, Note={inputEvent.Key}, Velocity={inputEvent.Velocity}";
                        this.割り当て可能 = false;                           // 割り当て不可
                        this.ForeColor = System.Drawing.Color.Gray;         // 灰
                    }
                }
                else
                {
                    throw new ArgumentException( "未対応のデバイスです。" );
                }
            }
        }

        /// <summary>
        ///     <see cref="listView割り当て済み入力リスト"/> 用の ListViewItem 拡張。
        ///     表示テキストのほかに、入力情報も持つ。
        /// </summary>
        private class ListViewItem割り当て済み入力リスト用 : ListViewItem
        {
            public bool 割り当て可能;
            public InputDeviceType deviceType;      // Device種別
            public キーバインディング.IdKey idKey;   // DeviceID, key

            public ListViewItem割り当て済み入力リスト用( InputDeviceType deviceType, キーバインディング.IdKey idKey )
            {
                this.割り当て可能 = true;
                this.deviceType = deviceType;
                this.idKey = idKey;

                this.Text = 
                    ( deviceType == InputDeviceType.Keyboard ) ? $"Keyboard, {idKey.key}, '{( (Key) idKey.key ).ToString()}'" :
                    ( deviceType == InputDeviceType.MidiIn ) ? $"MidiIn[{idKey.deviceId}], Note:{idKey.key}" :
                    throw new ArgumentException( "未対応のデバイスです。" );
            }
        }

        /// <summary>
        ///     ダイアログで編集した内容は、このメンバにいったん保存される。
        /// </summary>
        private キーバインディング _変更後のキーバインディング;

        private ドラム入力種別 _現在選択されているドラム入力種別 = ドラム入力種別.Unknown;

        private int _FootPedal現在値;

        /// <summary>
        ///     <see cref="_現在選択されているドラム入力種別"/> について、<see cref="_変更後のキーバインディング"/> の内容を、
        ///     割り当て済みリストに反映する。
        /// </summary>
        private void _割り当て済みリストを更新する( ListViewItem入力リスト用 選択する項目 = null )
        {
            this.listView割り当て済み入力リスト.Items.Clear();

            // (A) キーボード

            var キー割り当て = this._変更後のキーバインディング
                .キーボードtoドラム
                .Where( ( kvp ) => ( kvp.Value == this._現在選択されているドラム入力種別 ) );

            foreach( var key in キー割り当て )
                this.listView割り当て済み入力リスト.Items.Add( new ListViewItem割り当て済み入力リスト用( InputDeviceType.Keyboard, key.Key ) );

            // (B) MIDI入力

            var MidiIn割り当て = this._変更後のキーバインディング
                .MIDItoドラム
                .Where( ( kvp ) => ( kvp.Value == this._現在選択されているドラム入力種別 ) );

            foreach( var note in MidiIn割り当て )
                this.listView割り当て済み入力リスト.Items.Add( new ListViewItem割り当て済み入力リスト用( InputDeviceType.MidiIn, note.Key ) );

            // フォーカス

            if( null != 選択する項目 )
            {
                foreach( ListViewItem割り当て済み入力リスト用 item in this.listView割り当て済み入力リスト.Items )
                {
                    if( item.deviceType == 選択する項目.deviceType &&
                        item.idKey.deviceId == 選択する項目.inputEvent.DeviceID &&
                        item.idKey.key == 選択する項目.inputEvent.Key )
                    {
                        // > 項目をプログラムで選択しても、フォーカスは自動的に ListView コントロールには変更されません。
                        // > そのため、項目を選択するときは、通常、その項目をフォーカスがある状態に設定します。（MSDNより）
                        // https://msdn.microsoft.com/ja-jp/library/y4x56c0b(v=vs.110).aspx
                        item.Focused = true;
                        item.Selected = true;
                    }
                }
            }
        }
        private void _現在選択されている入力リストの入力行を割り当て済み入力リストに追加する()
        {
            foreach( var itemobj in this.listView入力リスト.SelectedItems )
            {
                if( ( itemobj is ListViewItem入力リスト用 item ) &&   // 選択されているのが ListViewItem入力リスト用 じゃなければ何もしない。
                  ( item.割り当て可能 ) )                             // 割り当て可能のもののみ割り当てる。
                {
                    var idKey = new キーバインディング.IdKey( item.inputEvent );

                    // (A) キーボード
                    if( item.deviceType == InputDeviceType.Keyboard )
                    {
                        // すでに割り当て済みの場合は、先にそれを削除する。
                        this._変更後のキーバインディング
                            .キーボードtoドラム
                            .Remove( idKey );

                        // 追加し、更新。
                        this._変更後のキーバインディング
                            .キーボードtoドラム
                            .Add( idKey, this._現在選択されているドラム入力種別 );

                        this._割り当て済みリストを更新する( item );
                        this.listView割り当て済み入力リスト.Focus();
                        this._変更あり = true;
                    }

                    // (B) Midi入力
                    else if( item.deviceType == InputDeviceType.MidiIn )
                    {
                        // すでに割り当て済みの場合は、先にそれを削除する。
                        this._変更後のキーバインディング
                            .MIDItoドラム
                            .Remove( idKey );

                        // 追加し、更新。
                        this._変更後のキーバインディング
                            .MIDItoドラム
                            .Add( idKey, this._現在選択されているドラム入力種別 );

                        this._割り当て済みリストを更新する( item );
                        this.listView割り当て済み入力リスト.Focus();
                        this._変更あり = true;
                    }
                }
            }
        }

        private double _前回の入力リスト追加時刻;
        private void _一定時間が経っていれば空行を挿入する()
        {
            double 今回の入力リスト追加時刻 = QPCTimer.生カウント相対値を秒へ変換して返す( QPCTimer.生カウント );

            if( 1.0 < ( 今回の入力リスト追加時刻 - this._前回の入力リスト追加時刻 ) )   // 1秒以上経っていれば
                this.listView入力リスト.Items.Add( "" );

            this._前回の入力リスト追加時刻 = 今回の入力リスト追加時刻;
        }

        private void comboBoxパッドリスト_SelectedIndexChanged( object sender, EventArgs e )
        {
            this._現在選択されているドラム入力種別 = (ドラム入力種別) Enum.Parse( typeof( ドラム入力種別 ), (string) this.comboBoxパッドリスト.SelectedItem );

            this._割り当て済みリストを更新する();
        }
        private void button追加_Click( object sender, EventArgs e )
        {
            this._現在選択されている入力リストの入力行を割り当て済み入力リストに追加する();
        }
        private void listView入力リスト_DoubleClick( object sender, EventArgs e )
        {
            this._現在選択されている入力リストの入力行を割り当て済み入力リストに追加する();
        }
        private void button割り当て解除_Click( object sender, EventArgs e )
        {
            // 選択されている項目に対応する入力をキーバインディングから削除。
            foreach( ListViewItem割り当て済み入力リスト用 item in this.listView割り当て済み入力リスト.SelectedItems )
            {
                // (A) キーボード。
                if( item.deviceType == InputDeviceType.Keyboard )
                {
                    // 割り当て済みの場合は、削除する。
                    this._変更後のキーバインディング
                        .キーボードtoドラム
                        .Remove( item.idKey );

                    this._変更あり = true;
                }

                // (B) MIDI入力。
                else if( item.deviceType == InputDeviceType.MidiIn )
                {
                    // 割り当て済みの場合は、削除する。
                    this._変更後のキーバインディング
                        .MIDItoドラム
                        .Remove( item.idKey );

                    this._変更あり = true;
                }
            }

            this._割り当て済みリストを更新する();
        }

        private bool _変更あり;
        private void 入力割り当てダイアログ_FormClosing( object sender, FormClosingEventArgs e )
        {
            // ※ウィンドウを閉じようとした時も Cancel になる。
            if( this.DialogResult == DialogResult.Cancel && this._変更あり )
            {
                var dr = MessageBoxEx.Show( "変更を破棄していいですか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2 );

                if( dr == DialogResult.No )
                    e.Cancel = true;
            }
        }
    }
}
