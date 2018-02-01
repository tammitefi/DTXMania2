using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;
using FDK.入力;
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

            // パッドリストを初期化。
            foreach( ドラム入力種別 drum in Enum.GetValues( typeof( ドラム入力種別 ) ) )
            {
                if( drum != ドラム入力種別.Unknown )   // Unknown 以外を登録。
                    this.comboBoxパッドリスト.Items.Add( drum.ToString() );
            }

            // 変更後のキーバインディングを初期化。
            this._変更後のキーバインディング = (キーバインディング) App.システム設定.キーバインディング.Clone();

            // 最初のパッドを選択し、割り当て済みリストを更新。
            this.comboBoxパッドリスト.SelectedIndex = 0;
        }
        public DialogResult 表示する()
        {
            // このウィンドウ用の入力管理インスタンスを生成。
            using( var 入力管理 = new 入力.入力管理( this.Handle ) )
            using( var timer = new Timer() )
            {
                入力管理.Initialize();

                timer.Interval = 100;
                timer.Tick += ( sender, arg ) => {
                    this._デバイスをポーリングして表示する( 入力管理.Keyboard, ( ie ) => $"Keyboard, {ie.Key}, '{( (Key) ie.Key ).ToString()}'" );
                    this._デバイスをポーリングして表示する( 入力管理.MidiIn, ( ie ) => $"MidiIn[{ie.DeviceID}], Note:{ie.Key}, Velocity:{ie.Velocity}" );
                };

                timer.Start();
                var dr = this.ShowDialog( App.Instance );
                timer.Stop();

                return dr;
            }
        }

        private キーバインディング _変更後のキーバインディング;

        private void _割り当て済みリストを更新する()
        {
            var 選択されているパッド = (ドラム入力種別) Enum.Parse( typeof( ドラム入力種別 ), (string) this.comboBoxパッドリスト.SelectedItem );

            this.listBox割り当て済み入力リスト.Items.Clear();

            var キー割り当て = this._変更後のキーバインディング.キーボードtoドラム.Where( ( kvp ) => ( kvp.Value == 選択されているパッド ) );
            foreach( var key in キー割り当て )
                this.listBox割り当て済み入力リスト.Items.Add( $"Keyboard, {key.Key.key}, '{( (Key) key.Key.key ).ToString()}'" );

            var MidiIn割り当て = this._変更後のキーバインディング.MIDItoドラム.Where( ( kvp ) => ( kvp.Value == 選択されているパッド ) );
            foreach( var note in MidiIn割り当て )
                this.listBox割り当て済み入力リスト.Items.Add( $"MidiIn[{note.Key.deviceId}], Note:{note.Key.key}" );
        }
        private void _デバイスをポーリングして表示する( IInputDevice device, Func<InputEvent, string> 行を取得 )
        {
            device.ポーリングする();

            for( int i = 0; i < device.入力イベントリスト.Count; i++ )
            {
                var ie = device.入力イベントリスト[ i ];

                if( ie.押された )
                {
                    this.listView入力リスト.Items.Add( 行を取得( ie ) );
                    this.listView入力リスト.EnsureVisible( this.listView入力リスト.Items.Count - 1 );
                }
            }

        }

        private void comboBoxパッドリスト_SelectedIndexChanged( object sender, EventArgs e )
        {
            this._割り当て済みリストを更新する();
        }
    }
}
