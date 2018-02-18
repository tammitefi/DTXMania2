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

namespace DTXmatixx.ステージ.曲ツリー構築
{
    class 曲ツリー構築ステージ : ステージ
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

        public 曲ツリー構築ステージ()
        {
            this._表示文字列 =
                $"{App.属性<AssemblyTitleAttribute>().Title} {App.リリース番号:000}\n" +
                $"{App.属性<AssemblyCopyrightAttribute>().Copyright}\n" +
                "\n";

            this.子を追加する( this._文字列画像 = new 文字列画像() {
                フォント名 = "Meiryo UI",
                フォントサイズpt = 20f,
                前景色 = Color4.White,
                表示文字列 = this._表示文字列,
            } );
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
            this._文字列画像.表示文字列 = this._表示文字列 + $"Enumerating and loading score properties from file ... {Interlocked.Read( ref this._ファイル検出数 )}";
            this._文字列画像.描画する( dc, 0f, 0f );

            App.入力管理.すべての入力デバイスをポーリングする();

            switch( this.現在のフェーズ )
            {
                case フェーズ.開始:
                    // 状態チェック； ここの時点で曲ツリーが初期状態であること。
                    Debug.Assert( null != App.曲ツリー.ルートノード );
                    Debug.Assert( null == App.曲ツリー.フォーカスノード );
                    Debug.Assert( null == App.曲ツリー.フォーカスリスト );

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

                        this._文字列画像.表示文字列 += "\n\nOK";
                        this._文字列画像.描画する( dc, 0f, 0f );

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

        private 文字列画像 _文字列画像 = null;
        private string _表示文字列 = "";
        private Task _構築タスク = null;
        private long _ファイル検出数 = 0;
    }
}
