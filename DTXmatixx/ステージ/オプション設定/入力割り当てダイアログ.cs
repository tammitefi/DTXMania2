using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using SharpDX.DirectInput;
using FDK;
using FDK.入力;

namespace DTXmatixx.ステージ.オプション設定
{
    public partial class 入力割り当てダイアログ : Form
    {
        public 入力割り当てダイアログ()
        {
            InitializeComponent();
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

                    this._デバイスをポーリングして表示する(
                        入力管理.Keyboard, 
                        ( ie ) => $"Keyboard: {ie.Key} '{( (Key) ie.Key ).ToString()}'" );

                    this._デバイスをポーリングして表示する(
                        入力管理.MidiIn, 
                        ( ie ) => $"MidiIn: [ID:{ie.DeviceID}], Note:{ie.Key}, Velocity:{ie.Velocity}" );

                };

                timer.Start();

                var dr = this.ShowDialog( App.Instance );

                timer.Stop();

                return dr;
            }
        }

        private void _デバイスをポーリングして表示する( IInputDevice device, Func<InputEvent,string> 表示文字 )
        {
            device.ポーリングする();

            for( int i = 0; i < device.入力イベントリスト.Count; i++ )
            {
                var ie = device.入力イベントリスト[ i ];

                if( ie.押された )
                {
                    this.listView入力リスト.Items.Add( 表示文字( ie ) );
                    this.listView入力リスト.EnsureVisible( this.listView入力リスト.Items.Count - 1 );
                }
            }

        }
    }
}
