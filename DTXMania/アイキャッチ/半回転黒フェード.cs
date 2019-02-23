﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using SharpDX.Direct2D1;
using FDK;
using DTXMania2.Language;

namespace DTXMania.アイキャッチ
{
    class 半回転黒フェード : アイキャッチ
    {
        public 半回転黒フェード()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._ロゴ画像 = new 画像( @"$(System)images\タイトルロゴ・影.png" ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._黒ブラシ = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, Color.Black );
                this.現在のフェーズ = フェーズ.未定;
            }
        }
        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._黒ブラシ?.Dispose();
                this._黒ブラシ = null;

                this._アニメーション?.Dispose();
                this._アニメーション = null;
            }
        }

        /// <summary>
        ///     アイキャッチのクローズアニメーションを開始する。
        /// </summary>
        public override void クローズする( float 速度倍率 = 1.0f )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                double 秒( double v ) => ( v / 速度倍率 );

                var animation = グラフィックデバイス.Instance.Animation;

                this._アニメーション?.Dispose();
                this._アニメーション = new アニメ( animation.Manager );

                const double 期間sec = 0.4;

                #region " (1) 背景マスク のアニメーション構築 "
                //----------------
                this._アニメーション.背景_不透明度 = new Variable( animation.Manager, initialValue: 0.0 );
                using( var 不透明度の遷移 = animation.TrasitionLibrary.Linear( duration: 秒( 期間sec ), finalValue: 0.7 ) )
                {
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.背景_不透明度, 不透明度の遷移 );
                }
                //----------------
                #endregion
                #region " (2) 黒幕1（左下） のアニメーション構築 "
                //----------------
                this._アニメーション.黒幕1左下_基点位置X = new Variable( animation.Manager, initialValue: -500.0 );
                this._アニメーション.黒幕1左下_回転角rad = new Variable( animation.Manager, initialValue: Math.PI * 0.75 );
                using( var 基点位置Xの遷移1 = animation.TrasitionLibrary.Linear( duration: 秒( 期間sec * 0.2 ), finalValue: 0.0 ) )
                using( var 基点位置Xの遷移2 = animation.TrasitionLibrary.Linear( duration: 秒( 期間sec * 0.8 ), finalValue: グラフィックデバイス.Instance.設計画面サイズ.Width / 2.0 ) )
                using( var 回転角の遷移 = animation.TrasitionLibrary.Linear( duration: 秒( 期間sec ), finalValue: 0.0 ) )
                {
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.黒幕1左下_基点位置X, 基点位置Xの遷移1 );
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.黒幕1左下_基点位置X, 基点位置Xの遷移2 );
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.黒幕1左下_回転角rad, 回転角の遷移 );
                }
                //----------------
                #endregion
                #region " (3) 黒幕2（右上） のアニメーション構築 "
                //----------------
                this._アニメーション.黒幕2右上_基点位置X = new Variable( animation.Manager, initialValue: グラフィックデバイス.Instance.設計画面サイズ.Width + 500.0 );
                this._アニメーション.黒幕2右上_回転角rad = new Variable( animation.Manager, initialValue: Math.PI * 0.75f );
                using( var 基点位置Xの遷移1 = animation.TrasitionLibrary.Linear( duration: 秒( 期間sec * 0.2 ), finalValue: グラフィックデバイス.Instance.設計画面サイズ.Width ) )
                using( var 基点位置Xの遷移2 = animation.TrasitionLibrary.Linear( duration: 秒( 期間sec * 0.8 ), finalValue: グラフィックデバイス.Instance.設計画面サイズ.Width / 2.0 ) )
                using( var 回転角の遷移 = animation.TrasitionLibrary.Linear( duration: 秒( 期間sec ), finalValue: 0.0 ) )
                {
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.黒幕2右上_基点位置X, 基点位置Xの遷移1 );
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.黒幕2右上_基点位置X, 基点位置Xの遷移2 );
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.黒幕2右上_回転角rad, 回転角の遷移 );
                }
                //----------------
                #endregion
                #region " (4) ロゴ のアニメーション構築 "
                //----------------
                this._アニメーション.ロゴ_位置X = new Variable( animation.Manager, initialValue: 1222.0 - 150.0 );
                this._アニメーション.ロゴ_不透明度 = new Variable( animation.Manager, initialValue: 0.0 );
                using( var 位置Xの遷移 = animation.TrasitionLibrary.AccelerateDecelerate( duration: 秒( 期間sec ), finalValue: 1222.0, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
                using( var 不透明度の遷移 = animation.TrasitionLibrary.Linear( duration: 秒( 期間sec ), finalValue: 1.0 ) )
                {
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.ロゴ_位置X, 位置Xの遷移 );
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.ロゴ_不透明度, 不透明度の遷移 );
                }
                //----------------
                #endregion

                using( var 時間稼ぎ = animation.TrasitionLibrary.Constant( duration: 秒( 0.5 ) ) )
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.ロゴ_位置X, 時間稼ぎ );

                // 今すぐ開始。
                this._アニメーション.ストーリーボード.Schedule( animation.Timer.Time );
                this.現在のフェーズ = フェーズ.クローズ;
            }
        }

        /// <summary>
        ///     アイキャッチのオープンアニメーションを開始する。
        /// </summary>
        public override void オープンする( float 速度倍率 = 1.0f )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                double 秒( double v ) => ( v / 速度倍率 );

                var animation = グラフィックデバイス.Instance.Animation;

                this._アニメーション?.Dispose();
                this._アニメーション = new アニメ( animation.Manager );

                const double 期間sec = 0.6;

                #region " (1) 背景マスク のアニメーション構築 "
                //----------------
                this._アニメーション.背景_不透明度 = new Variable( animation.Manager, initialValue: 0.0 );
                using( var 不透明度の遷移 = animation.TrasitionLibrary.Constant( duration: 秒( 期間sec ) ) )
                {
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.背景_不透明度, 不透明度の遷移 );
                }
                //----------------
                #endregion
                #region " (2) 黒幕1（左下） のアニメーション構築 "
                //----------------
                this._アニメーション.黒幕1左下_基点位置X = new Variable( animation.Manager, initialValue: グラフィックデバイス.Instance.設計画面サイズ.Width / 2.0 );
                this._アニメーション.黒幕1左下_回転角rad = new Variable( animation.Manager, initialValue: 0.0 );
                using( var 基点位置Xの遷移1 = animation.TrasitionLibrary.Linear( duration: 秒( 期間sec * 0.8 ), finalValue: 0.0 ) )
                using( var 基点位置Xの遷移2 = animation.TrasitionLibrary.AccelerateDecelerate( duration: 秒( 期間sec * 0.2 ), finalValue: -500.0, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
                using( var 回転角の遷移 = animation.TrasitionLibrary.Linear( duration: 秒( 期間sec ), finalValue: Math.PI * 0.75f ) )
                {
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.黒幕1左下_基点位置X, 基点位置Xの遷移1 );
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.黒幕1左下_基点位置X, 基点位置Xの遷移2 );
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.黒幕1左下_回転角rad, 回転角の遷移 );
                }
                //----------------
                #endregion
                #region " (3) 黒幕2（右上） のアニメーション構築 "
                //----------------
                this._アニメーション.黒幕2右上_基点位置X = new Variable( animation.Manager, initialValue: グラフィックデバイス.Instance.設計画面サイズ.Width / 2.0 );
                this._アニメーション.黒幕2右上_回転角rad = new Variable( animation.Manager, initialValue: 0.0 );
                using( var 基点位置Xの遷移1 = animation.TrasitionLibrary.Linear( duration: 秒( 期間sec * 0.8 ), finalValue: グラフィックデバイス.Instance.設計画面サイズ.Width ) )
                using( var 基点位置Xの遷移2 = animation.TrasitionLibrary.AccelerateDecelerate( duration: 秒( 期間sec * 0.2 ), finalValue: グラフィックデバイス.Instance.設計画面サイズ.Width + 500.0, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
                using( var 回転角の遷移 = animation.TrasitionLibrary.Linear( duration: 秒( 期間sec ), finalValue: Math.PI * 0.75f ) )
                {
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.黒幕2右上_基点位置X, 基点位置Xの遷移1 );
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.黒幕2右上_基点位置X, 基点位置Xの遷移2 );
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.黒幕2右上_回転角rad, 回転角の遷移 );
                }
                //----------------
                #endregion
                #region " (4) ロゴ のアニメーション構築 "
                //----------------
                this._アニメーション.ロゴ_位置X = new Variable( animation.Manager, initialValue: 1222.0 );
                this._アニメーション.ロゴ_不透明度 = new Variable( animation.Manager, initialValue: 1.0 );
                using( var 位置Xの遷移 = animation.TrasitionLibrary.AccelerateDecelerate( duration: 秒( 期間sec ), finalValue: 1222.0 - 150.0, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
                using( var 不透明度の遷移 = animation.TrasitionLibrary.Linear( duration: 秒( 期間sec ), finalValue: 0.0 ) )
                {
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.ロゴ_位置X, 位置Xの遷移 );
                    this._アニメーション.ストーリーボード.AddTransition( this._アニメーション.ロゴ_不透明度, 不透明度の遷移 );
                }
                //----------------
                #endregion

                // 今すぐ開始。
                this._アニメーション.ストーリーボード.Schedule( animation.Timer.Time );
                this.現在のフェーズ = フェーズ.オープン;
            }
        }

        /// <summary>
        ///     アイキャッチのアニメーションを進行し、アイキャッチ画像を描画する。
        /// </summary>
        protected override void 進行描画する( DeviceContext1 dc, StoryboardStatus 描画しないStatus )
        {
            bool すべて完了 = true;

            switch( this.現在のフェーズ )
            {
                case フェーズ.クローズ:
                    #region " *** "
                    //----------------
                    if( this._アニメーション.ストーリーボード.Status != StoryboardStatus.Ready )
                        すべて完了 = false;

                    if( this._アニメーション.ストーリーボード.Status != 描画しないStatus )
                    {
                        グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                            var pretrans = dc.Transform;

                            #region " 背景マスク "
                            //----------------
                            using( var ブラシ = new SolidColorBrush( dc, new Color4( Color3.Black, (float) this._アニメーション.背景_不透明度.Value ) ) )
                            {
                                dc.FillRectangle( new RectangleF( 0f, 0f, グラフィックデバイス.Instance.設計画面サイズ.Width, グラフィックデバイス.Instance.設計画面サイズ.Width ), ブラシ );
                            }
                            //----------------
                            #endregion
                            #region " (2) 黒幕1（左下）"
                            //----------------
                            {
                                float w = グラフィックデバイス.Instance.設計画面サイズ.Width * 1.5f;
                                float h = グラフィックデバイス.Instance.設計画面サイズ.Height;
                                var rc = new RectangleF( -w / 2f, -h / 2f, w, h );

                                dc.Transform =
                                    Matrix3x2.Rotation( // 上辺中央を中心として回転
                                        angle: (float) this._アニメーション.黒幕1左下_回転角rad.Value,
                                        center: new Vector2( 0f, -rc.Height / 2f ) ) *
                                    Matrix3x2.Translation(  // (基点X, H×3/4) へ移動
                                        x: (float) this._アニメーション.黒幕1左下_基点位置X.Value,
                                        y: グラフィックデバイス.Instance.設計画面サイズ.Height ) *
                                    pretrans;

                                dc.FillRectangle( rc, this._黒ブラシ );
                            }
                            //----------------
                            #endregion
                            #region " (3) 黒幕2（右上）"
                            //----------------
                            {
                                float w = グラフィックデバイス.Instance.設計画面サイズ.Width * 1.5f;
                                float h = グラフィックデバイス.Instance.設計画面サイズ.Height;
                                var rc = new RectangleF( -w / 2f, -h / 2f, w, h );

                                dc.Transform =
                                    Matrix3x2.Rotation( // 下辺中央を中心として回転
                                        angle: (float) this._アニメーション.黒幕2右上_回転角rad.Value,
                                        center: new Vector2( 0f, rc.Height / 2f ) ) *
                                    Matrix3x2.Translation(  // (基点X, H×1/4) へ移動
                                        x: (float) this._アニメーション.黒幕2右上_基点位置X.Value,
                                        y: 0f ) *
                                    pretrans;

                                dc.FillRectangle( rc, this._黒ブラシ );
                            }
                            //----------------
                            #endregion
                            #region " (4) ロゴ "
                            //----------------
                            dc.Transform =
                                Matrix3x2.Scaling( 639f / this._ロゴ画像.サイズ.Width, 262f / this._ロゴ画像.サイズ.Height ) *
                                Matrix3x2.Translation( (float) this._アニメーション.ロゴ_位置X.Value, 771f ) *
                                pretrans;

                            dc.DrawBitmap( this._ロゴ画像.Bitmap, (float) this._アニメーション.ロゴ_不透明度.Value, BitmapInterpolationMode.Linear );
                            //----------------
                            #endregion

                        } );
                    }
                    //----------------
                    #endregion
                    break;

                case フェーズ.クローズ完了:
                    break;

                case フェーズ.オープン:
                    #region " *** "
                    //----------------
                    if( this._アニメーション.ストーリーボード.Status != StoryboardStatus.Ready )
                        すべて完了 = false;

                    if( this._アニメーション.ストーリーボード.Status != 描画しないStatus )
                    {
                        グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                            var pretrans = dc.Transform;

                            #region " (1) 背景マスク "
                            //----------------
                            using( var ブラシ = new SolidColorBrush( dc, new Color4( Color3.Black, (float) this._アニメーション.背景_不透明度.Value ) ) )
                            {
                                dc.FillRectangle( new RectangleF( 0f, 0f, グラフィックデバイス.Instance.設計画面サイズ.Width, グラフィックデバイス.Instance.設計画面サイズ.Width ), ブラシ );
                            }
                            //----------------
                            #endregion
                            #region " (2) 黒幕1（左下）"
                            //----------------
                            {
                                float w = グラフィックデバイス.Instance.設計画面サイズ.Width * 1.5f;
                                float h = グラフィックデバイス.Instance.設計画面サイズ.Height;
                                var rc = new RectangleF( -w / 2f, -h / 2f, w, h );

                                dc.Transform =
                                    Matrix3x2.Rotation( // 上辺中央を中心として回転
                                        angle: (float) this._アニメーション.黒幕1左下_回転角rad.Value,
                                        center: new Vector2( 0f, -rc.Height / 2f ) ) *
                                    Matrix3x2.Translation(  // (基点X, H×3/4) へ移動
                                        x: (float) this._アニメーション.黒幕1左下_基点位置X.Value,
                                        y: グラフィックデバイス.Instance.設計画面サイズ.Height ) *
                                    pretrans;

                                dc.FillRectangle( rc, this._黒ブラシ );
                            }
                            //----------------
                            #endregion
                            #region " (3) 黒幕2（右上）"
                            //----------------
                            {
                                float w = グラフィックデバイス.Instance.設計画面サイズ.Width * 1.5f;
                                float h = グラフィックデバイス.Instance.設計画面サイズ.Height;
                                var rc = new RectangleF( -w / 2f, -h / 2f, w, h );

                                dc.Transform =
                                    Matrix3x2.Rotation( // 下辺中央を中心として回転
                                        angle: (float) this._アニメーション.黒幕2右上_回転角rad.Value,
                                        center: new Vector2( 0f, rc.Height / 2f ) ) *
                                    Matrix3x2.Translation(  // (基点X, H×1/4) へ移動
                                        x: (float) this._アニメーション.黒幕2右上_基点位置X.Value,
                                        y: 0f ) *
                                    pretrans;

                                dc.FillRectangle( rc, this._黒ブラシ );
                            }
                            //----------------
                            #endregion
                            #region " (4) ロゴ "
                            //----------------
                            dc.Transform =
                                Matrix3x2.Scaling( 639f / this._ロゴ画像.サイズ.Width, 262f / this._ロゴ画像.サイズ.Height ) *
                                Matrix3x2.Translation( (float) this._アニメーション.ロゴ_位置X.Value, 771f ) *
                                pretrans;

                            dc.DrawBitmap( this._ロゴ画像.Bitmap, (float) this._アニメーション.ロゴ_不透明度.Value, BitmapInterpolationMode.Linear );
                            //----------------
                            #endregion

                        } );
                    }
                    //----------------
                    #endregion
                    break;

                case フェーズ.オープン完了:
                    break;
            }

            if( すべて完了 )
            {
                if( this.現在のフェーズ == フェーズ.クローズ )
                {
                    this.現在のフェーズ = フェーズ.クローズ完了;
                }
                else if( this.現在のフェーズ == フェーズ.オープン )
                {
                    this.現在のフェーズ = フェーズ.オープン完了;
                }
            }
        }


        private class アニメ : IDisposable
        {
            public Variable 背景_不透明度 = null;
            public Variable 黒幕1左下_基点位置X = null;
            public Variable 黒幕1左下_回転角rad = null;
            public Variable 黒幕2右上_基点位置X = null;
            public Variable 黒幕2右上_回転角rad = null;
            public Variable ロゴ_位置X = null;
            public Variable ロゴ_不透明度 = null;
            public Storyboard ストーリーボード = null;

            public アニメ( Manager am )
            {
                this.ストーリーボード = new Storyboard( am );
            }
            public void Dispose()
            {
                this.ストーリーボード?.Abandon();

                this.ストーリーボード?.Dispose();
                this.ストーリーボード = null;

                this.背景_不透明度?.Dispose();
                this.背景_不透明度 = null;

                this.黒幕1左下_基点位置X?.Dispose();
                this.黒幕1左下_基点位置X = null;

                this.黒幕1左下_回転角rad?.Dispose();
                this.黒幕1左下_回転角rad = null;

                this.黒幕2右上_基点位置X?.Dispose();
                this.黒幕2右上_基点位置X = null;

                this.黒幕2右上_回転角rad?.Dispose();
                this.黒幕2右上_回転角rad = null;

                this.ロゴ_位置X?.Dispose();
                this.ロゴ_位置X = null;

                this.ロゴ_不透明度?.Dispose();
                this.ロゴ_不透明度 = null;
            }
        }
        private アニメ _アニメーション = null;

        private 画像 _ロゴ画像 = null;
        private SolidColorBrush _黒ブラシ = null;
    }
}
