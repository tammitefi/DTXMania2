using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;
using FDK.入力;
using FDK.カウンタ;
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
            this.Scale(
                new System.Drawing.SizeF(
                    Math.Max( 1f, グラフィックデバイス.Instance.拡大率DPXtoPX横 ),
                    Math.Max( 1f, グラフィックデバイス.Instance.拡大率DPXtoPX縦 ) ) );
        }

        public void 表示する()
        {
            using( var 入力管理 = new 入力.入力管理( this.Handle ) )  // このウィンドウ用の入力管理インスタンスを生成。
            using( var timer = new Timer() )
            {
                #region " 初期化。"
                //----------------
                入力管理.Initialize();

                // パッドリストを初期化。
                foreach( ドラム入力種別 drum in Enum.GetValues( typeof( ドラム入力種別 ) ) )
                {
                    if( drum != ドラム入力種別.Unknown )   // Unknown 以外を登録。
                        this.comboBoxパッドリスト.Items.Add( drum.ToString() );
                }

                // 変更後のキーバインディングを、現在の設定値で初期化。
                this._変更後のキーバインディング = (キーバインディング) App.システム設定.キーバインディング.Clone();

                // 最初のパッドを選択し、割り当て済みリストを更新。
                this.comboBoxパッドリスト.SelectedIndex = 0;

                this._前回の入力リスト追加時刻 = QPCTimer.生カウント相対値を秒へ変換して返す( QPCTimer.生カウント );

                this._変更あり = false;
                //----------------
                #endregion

                #region " タイマーイベント処理。"
                //----------------
                timer.Interval = 100;
                timer.Tick += ( sender, arg ) => {

                    // (A) キーボードのポーリングと、入力リストへの出力。

                    入力管理.Keyboard.ポーリングする();

                    for( int i = 0; i < 入力管理.Keyboard.入力イベントリスト.Count; i++ )
                    {
                        var ie = 入力管理.Keyboard.入力イベントリスト[ i ];

                        if( ie.押された )
                        {
                            var item = new ListViewItem入力リスト用( InputDeviceType.Keyboard, ie );

                            this._一定時間が経っていれば空行を挿入する();

                            this.listView入力リスト.Items.Add( item );
                            this.listView入力リスト.EnsureVisible( this.listView入力リスト.Items.Count - 1 );
                        }
                    }

                    // (B) Midi入力のポーリングと、入力リストへの出力。

                    入力管理.MidiIn.ポーリングする();

                    for( int i = 0; i < 入力管理.MidiIn.入力イベントリスト.Count; i++ )
                    {
                        var ie = 入力管理.MidiIn.入力イベントリスト[ i ];

                        if( ie.押された )
                        {
                            var item = new ListViewItem入力リスト用( InputDeviceType.MidiIn, ie );

                            this._一定時間が経っていれば空行を挿入する();

                            this.listView入力リスト.Items.Add( item );
                            this.listView入力リスト.EnsureVisible( this.listView入力リスト.Items.Count - 1 );
                        }
                    }

                };
                //----------------
                #endregion

                DialogResult dr;

                #region " ダイアログ表示→終了。"
                //----------------
                timer.Start();

                dr = this.ShowDialog( App.Instance );

                timer.Stop();
                //----------------
                #endregion

                if( dr == DialogResult.OK )
                {
                    #region " 設定値の反映。"
                    //----------------
                    App.システム設定.キーバインディング = (キーバインディング) this._変更後のキーバインディング.Clone();
                    App.システム設定.保存する();
                    //----------------
                    #endregion
                }
            }
        }

        private class ListViewItem入力リスト用 : ListViewItem
        {
            public InputDeviceType deviceType;  // Device種別
            public InputEvent inputEvent;       // DeviceID, key, velocity

            public ListViewItem入力リスト用( InputDeviceType deviceType, InputEvent inputEvent )
            {
                this.deviceType = deviceType;
                this.inputEvent = inputEvent;

                this.Text = 
                    ( deviceType == InputDeviceType.Keyboard ) ? $"Keyboard, {inputEvent.Key}, '{( (Key) inputEvent.Key ).ToString()}'" :
                    ( deviceType == InputDeviceType.MidiIn ) ? $"MidiIn[{inputEvent.DeviceID}], Note:{inputEvent.Key}, Velocity:{inputEvent.Velocity}" :
                    throw new ArgumentException( "未対応のデバイスです。" );
            }
        }
        private class ListViewItem割り当て済み入力リスト用 : ListViewItem
        {
            public InputDeviceType deviceType;      // Device種別
            public キーバインディング.IdKey idKey;   // DeviceID, key

            public ListViewItem割り当て済み入力リスト用( InputDeviceType deviceType, キーバインディング.IdKey idKey )
            {
                this.deviceType = deviceType;
                this.idKey = idKey;

                this.Text = 
                    ( deviceType == InputDeviceType.Keyboard ) ? $"Keyboard, {idKey.key}, '{( (Key) idKey.key ).ToString()}'" :
                    ( deviceType == InputDeviceType.MidiIn ) ? $"MidiIn[{idKey.deviceId}], Note:{idKey.key}" :
                    throw new ArgumentException( "未対応のデバイスです。" );
            }
        }
        private キーバインディング _変更後のキーバインディング;
        private bool _変更あり;
        private double _前回の入力リスト追加時刻;
        private ドラム入力種別 _現在選択されているドラム入力種別 = ドラム入力種別.Unknown;

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
                // 選択されているのが ListViewItem入力リスト用 じゃなければ何もしない。
                if( itemobj is ListViewItem入力リスト用 item )
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
