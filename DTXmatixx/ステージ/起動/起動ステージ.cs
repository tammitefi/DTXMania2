using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.起動
{
    class 起動ステージ : ステージ
    {
        public enum フェーズ
        {
            開始,
            構築中,
            確定,
            キャンセル,
        }
        public フェーズ 現在のフェーズ
        {
            get;
            protected set;
        }

        public 起動ステージ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._表示文字列 =
                    $"{App.属性<AssemblyTitleAttribute>().Title} {App.リリース番号:000}\n" +
                    $"{App.属性<AssemblyCopyrightAttribute>().Copyright}\n" +
                    "\n";

                this.子を追加する( this._コンソールフォント = new 画像フォント( @"$(System)images\コンソールフォント20x32.png", @"$(System)images\コンソールフォント20x32.json", 文字幅補正dpx: -6f ) );
            }
        }
        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.現在のフェーズ = フェーズ.開始;
            }
        }
        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }
        public override void 進行描画する( DeviceContext1 dc )
        {
            this._コンソールフォント.描画する( dc, 0f, 0f, $"{App.属性<AssemblyTitleAttribute>().Title} {App.リリース番号:000}" );
            this._コンソールフォント.描画する( dc, 0f, 32f, $"{App.属性<AssemblyCopyrightAttribute>().Copyright}" );
            this._コンソールフォント.描画する( dc, 0f, 96f, $"Enumerating and loading score properties from file ... {Interlocked.Read( ref this._ファイル検出数 )}" );

            App.入力管理.すべての入力デバイスをポーリングする();

            switch( this.現在のフェーズ )
            {
                case フェーズ.開始:
					App.曲ツリー.非活性化する();
					App.曲ツリー = new 曲.曲ツリー();
					App.曲ツリー.活性化する();

					// 曲検索タスクを起動し、構築中フェーズへ。

					App.曲ツリー.非活性化する();
                    this._ファイル検出数 = 0;

                    this._構築タスク = Task.Factory.StartNew( () => {

                        foreach( var varpath in App.システム設定.曲検索フォルダ )
                        {
                            App.曲ツリー.曲を検索して親ノードに追加する( App.曲ツリー.ルートノード, varpath, ( vpath ) => {
                                Interlocked.Increment( ref this._ファイル検出数 );
                            } );
                        }

                    } );

                    this.現在のフェーズ = フェーズ.構築中;
                    break;

                case フェーズ.構築中:
                    if( this._構築タスク.IsCompleted || this._構築タスク.IsCanceled )
                    {
                        App.曲ツリー.活性化する();
                        this.現在のフェーズ = フェーズ.確定;
                    }
                    break;

                case フェーズ.確定:
                    break;

                case フェーズ.キャンセル:
                    break;
            }

            if( App.入力管理.キャンセルキーが入力された() )
            {
                this.現在のフェーズ = フェーズ.キャンセル;
            }
        }

        private 画像フォント _コンソールフォント = null;
        private string _表示文字列 = "";
        private Task _構築タスク = null;
        private long _ファイル検出数 = 0;
    }
}
