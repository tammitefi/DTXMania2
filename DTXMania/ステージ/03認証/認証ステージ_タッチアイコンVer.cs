﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using DTXMania.アイキャッチ;
using DTXMania2.Language;

namespace DTXMania.ステージ.認証
{
    // 現在は未使用。
    class 認証ステージ_タッチアイコンVer : ステージ
    {
        public enum フェーズ
        {
            フェードイン,
            表示,
            フェードアウト,
            時間つぶし,
            確定,
            キャンセル,
        }
        public フェーズ 現在のフェーズ
        {
            get;
            protected set;
        }

        public 認証ステージ_タッチアイコンVer()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._舞台画像 = new 舞台画像() );
                this.子Activityを追加する( this._タッチアイコン = new 画像( @"$(System)images\認証\タッチアイコン.png" ) );
                this.子Activityを追加する( this._確認できませんでした = new 文字列画像() { 表示文字列 = Resources.NotConfirm /*"確認できませんでした。"*/, フォントサイズpt = 40f, 描画効果 = 文字列画像.効果.ドロップシャドウ } );
            }
        }
        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.現在のフェーズ = フェーズ.フェードイン;
                this._初めての進行描画 = true;
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
            if( this._初めての進行描画 )
            {
                App.ステージ管理.現在のアイキャッチ.オープンする();
                this._初めての進行描画 = false;
            }

            App.入力管理.すべての入力デバイスをポーリングする();

            switch( this.現在のフェーズ )
            {
                case フェーズ.フェードイン:
                    this._舞台画像.進行描画する( dc, true );
                    this._タッチアイコン.描画する( dc, this._タッチアイコン表示行列 );
                    App.ステージ管理.現在のアイキャッチ.進行描画する( dc );

                    if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.アイキャッチ.フェーズ.オープン完了 )
                    {
                        this._表示フェーズカウンタ = new Counter( 0, 5000, 1 );   // 全5秒
                        this.現在のフェーズ = フェーズ.表示;
                    }
                    break;

                case フェーズ.表示:
                    this._舞台画像.進行描画する( dc, true );
                    this._タッチアイコン.描画する( dc, this._タッチアイコン表示行列 );
                    if( 1000 < this._表示フェーズカウンタ.現在値 )
                        this._確認できませんでした.描画する( dc, this._確認できませんでした表示行列 );

                    if( 2000 < this._表示フェーズカウンタ.現在値 )
                    {
                        App.ステージ管理.アイキャッチを選択しクローズする( nameof( 回転幕 ) );
                        this.現在のフェーズ = フェーズ.フェードアウト;
                    }
                    break;

                case フェーズ.フェードアウト:
                    this._舞台画像.進行描画する( dc, true );
                    App.ステージ管理.現在のアイキャッチ.進行描画する( dc );

                    if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.アイキャッチ.フェーズ.クローズ完了 )
                    {
                        this._待機フェーズカウンタ = new Counter( 0, 500, 1 );   // 全0.5秒
                        this.現在のフェーズ = フェーズ.時間つぶし;
                    }
                    break;

                case フェーズ.時間つぶし:
                    App.ステージ管理.現在のアイキャッチ.進行描画する( dc );

                    if( this._待機フェーズカウンタ.終了値に達した )
                    {
                        this.現在のフェーズ = フェーズ.確定;
                    }
                    break;

                case フェーズ.確定:
                case フェーズ.キャンセル:
                    break;
            }
        }

        private bool _初めての進行描画 = true;
        private 舞台画像 _舞台画像 = null;
        private 画像 _タッチアイコン = null;
        private 文字列画像 _確認できませんでした = null;
        private Counter _表示フェーズカウンタ = null;
        private Counter _待機フェーズカウンタ = null;
        private readonly Matrix3x2 _タッチアイコン表示行列 = Matrix3x2.Translation( 960f - 128f, 540f - 128f );
        private readonly Matrix3x2 _確認できませんでした表示行列 = Matrix3x2.Translation( 960f - 180f, 540f - 200f );
    }
}
