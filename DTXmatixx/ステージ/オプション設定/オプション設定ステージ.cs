using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;
using DTXmatixx.設定;

namespace DTXmatixx.ステージ.オプション設定
{
    class オプション設定ステージ : ステージ
    {
        public enum フェーズ
        {
            フェードイン,
            表示,
            入力割り当て,
            フェードアウト,
            確定,
            キャンセル,
        }
        public フェーズ 現在のフェーズ
        {
            get;
            protected set;
        }

        /// <summary>
        ///		パネルの識別に <see cref="パネル.パネル名"/> を使う場合、
        ///		その文字列を識別子として扱えるよう、ここで一元的に定義しておく。
        /// </summary>
        public class 項目名
        {
            public const string 入力割り当て = "入力割り当て";
            public const string 設定完了 = "設定完了";
            public const string 設定完了_戻る = "設定完了（戻る）";
        }

        public オプション設定ステージ()
        {
            this.子を追加する( this._舞台画像 = new 舞台画像() );
            this.子を追加する( this._パネルリスト = new パネルリスト() );
            //this.子を追加する( this._ルートパネルフォルダ = new パネル_フォルダ( "Root", null, null ) ); --> 活性化のたびに、子パネルとまとめて動的に追加する。
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.現在のフェーズ = フェーズ.フェードイン;
                this._初めての進行描画 = true;

                // フォルダツリーを構築する。

                this._ルートパネルフォルダ = new パネル_フォルダ( "Root", null, null );

                #region " (1) ルート＞自動演奏 "
                //----------------
                var user = App.ユーザ管理.ログオン中のユーザ;

                var 自動演奏 = new パネル_フォルダ( "自動演奏", this._ルートパネルフォルダ ) {
                    ヘッダ色 = パネル.ヘッダ色種別.赤,
                };

                自動演奏.子パネルリスト = new SelectableList<パネル>();

                foreach( AutoPlay種別 apType in Enum.GetValues( typeof( AutoPlay種別 ) ) )
                {
                    if( apType == AutoPlay種別.Unknown )
                        continue;

                    自動演奏.子パネルリスト.Add(
                        new パネル_ONOFFトグル(
                            パネル名: apType.ToString(),
                            初期状態はON: ( user.AutoPlay[ apType ] ),
                            値の変更処理: new Action<パネル>( ( panel ) => {
                                user.AutoPlay[ apType ] = ( (パネル_ONOFFトグル) panel ).ONである;
                            } ) ) );
                }

                自動演奏.子パネルリスト.Add( new パネル_システムボタン( 項目名.設定完了_戻る ) );
                自動演奏.子パネルリスト.SelectFirst();
                //----------------
                #endregion

                #region " (2) ルート "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト = new SelectableList<パネル>() {

                    #region " 画面モード "
                    //----------------
                    new パネル_文字列リスト(
                        パネル名: "画面モード",
                        初期選択肢番号: ( user.全画面モードである ) ? 1 : 0,
                        選択肢初期値s: new[] { "ウィンドウ", "全画面" },
                        値の変更処理: new Action<パネル>( ( panel ) => {
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
                        パネル名: "シンバルフリー",
                        初期状態はON: user.シンバルフリーモードである,
                        値の変更処理: new Action<パネル>( (panel) => {
                            user.シンバルフリーモードである = ( (パネル_ONOFFトグル) panel ).ONである;
                        } ) ),
                    //----------------
                    #endregion

					#region " 自動演奏（フォルダ）"
					//----------------
                    自動演奏,
                    //----------------
                    #endregion

					#region " 入力割り当て "
					//----------------
                    new パネル( 項目名.入力割り当て ) {
                        ヘッダ色 = パネル.ヘッダ色種別.赤,
                    },
                    //----------------
                    #endregion

   					#region " 設定完了（システムボタン）"
					//----------------
					new パネル_システムボタン( 項目名.設定完了 ),
					//----------------
					#endregion

                };
                //----------------
                #endregion

                this._パネルリスト.パネルリストを登録する( this._ルートパネルフォルダ );

                // 活性化する。
                this._ルートパネルフォルダ.活性化する();
            }
        }
        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._ルートパネルフォルダ.非活性化する();
                this._ルートパネルフォルダ = null;
            }
        }

        public override void 進行描画する( DeviceContext1 dc )
        {
            // 進行描画。

            if( this._初めての進行描画 )
            {
                this._舞台画像.ぼかしと縮小を適用する( 0.5 );
                this._初めての進行描画 = false;
            }

            this._舞台画像.進行描画する( dc );
            this._パネルリスト.進行描画する( dc, 613f, 0f );

            switch( this.現在のフェーズ )
            {
                case フェーズ.フェードイン:
                    this._パネルリスト.フェードインを開始する();
                    this.現在のフェーズ = フェーズ.表示;
                    break;

                case フェーズ.入力割り当て:
                    this._入力割り当てダイアログを表示する();
                    this._パネルリスト.フェードインを開始する();
                    this.現在のフェーズ = フェーズ.表示;
                    break;

                case フェーズ.フェードアウト:
                    App.ステージ管理.現在のアイキャッチ.進行描画する( dc );
                    if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.クローズ完了 )
                        this.現在のフェーズ = フェーズ.確定;
                    break;

                default:
                    break;
            }

            // 入力。

            App.入力管理.すべての入力デバイスをポーリングする();

            switch( this.現在のフェーズ )
            {
                case フェーズ.表示:

                    if( App.入力管理.キャンセルキーが入力された() )
                    {
                        this._パネルリスト.フェードアウトを開始する();
                        App.ステージ管理.アイキャッチを選択しクローズする( nameof( アイキャッチ.半回転黒フェード ) );
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
                        this._パネルリスト.現在選択中のパネル.左移動キーが入力された();
                    }
                    else if( App.入力管理.右移動キーが入力された() )
                    {
                        this._パネルリスト.現在選択中のパネル.右移動キーが入力された();
                    }
                    else if( App.入力管理.確定キーが入力された() )
                    {
                        switch( this._パネルリスト.現在選択中のパネル )
                        {
                            case パネル_フォルダ folder:
                                this._パネルリスト.子のパネルを選択する();
                                this._パネルリスト.フェードインを開始する();
                                break;

                            case パネル panel:
                                switch( panel.パネル名 )
                                {
                                    case 項目名.入力割り当て:
                                        this.現在のフェーズ = フェーズ.入力割り当て;
                                        break;

                                    case 項目名.設定完了:
                                        this._パネルリスト.フェードアウトを開始する();
                                        App.ステージ管理.アイキャッチを選択しクローズする( nameof( アイキャッチ.シャッター ) );
                                        this.現在のフェーズ = フェーズ.フェードアウト;
                                        break;

                                    case 項目名.設定完了_戻る:
                                        this._パネルリスト.親のパネルを選択する();
                                        this._パネルリスト.フェードインを開始する();
                                        break;

                                    default:
                                        this._パネルリスト.現在選択中のパネル.確定キーが入力された();
                                        break;
                                }
                                break;
                        }
                    }

                    break;
            }
        }

        private bool _初めての進行描画 = true;
        private 舞台画像 _舞台画像 = null;
        private パネルリスト _パネルリスト = null;
        private パネル_フォルダ _ルートパネルフォルダ = null;

        private void _入力割り当てダイアログを表示する()
        {
            using( var dlg = new 入力割り当てダイアログ() )
                dlg.表示する();
        }
    }
}
