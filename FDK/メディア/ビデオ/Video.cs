using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;

namespace FDK
{
    public class Video : Activity
    {
        public bool 加算合成 { get; set; } = false;

        public Video( VariablePath ファイルパス )
        {
            this._MFFileVideoSource = new MediaFoundationFileVideoSource( ファイルパス );
            this._VideoSource = this._MFFileVideoSource;
        }
        public Video( IVideoSource videoSource )
        {
            this._MFFileVideoSource = null;
            this._VideoSource = videoSource;
        }
        protected override void On活性化()
        {
        }
        protected override void On非活性化()
        {
            this.再生を終了する();

            this._MFFileVideoSource?.Dispose();
            this._MFFileVideoSource = null;
        }
        public void 再生を開始する()
        {
            this._再生タイマ = new QPCTimer();
        }
        public void 再生を終了する()
        {
            this._最後のフレーム?.Dispose();
            this._最後のフレーム = null;
        }
        public void 描画する( DeviceContext1 dc, RectangleF 描画先矩形, float 不透明度0to1 = 1.0f )
        {
            if( null == this._VideoSource )
                return;

            var 変換行列2D =
                Matrix3x2.Scaling( 描画先矩形.Width / this._VideoSource.フレームサイズ.Width, 描画先矩形.Height / this._VideoSource.フレームサイズ.Height ) *  // 拡大縮小
                Matrix3x2.Translation( 描画先矩形.Left, 描画先矩形.Top );  // 平行移動

            this.描画する( dc, 変換行列2D, 不透明度0to1 );
        }
        public void 描画する( DeviceContext1 dc, Matrix3x2 変換行列2D, float 不透明度0to1 = 1.0f )
        {
            if( null == this._VideoSource )
                return;

            long 次のフレームの表示予定時刻100ns = this._VideoSource.Peek(); // なければ負数

            if( 0 <= 次のフレームの表示予定時刻100ns )
            {
                if( ( null != this._最後のフレーム ) && ( 次のフレームの表示予定時刻100ns < this._最後のフレーム.表示時刻100ns ) )
                {
                    // (A) 次のフレームが前のフレームより過去 → ループしたので、タイマをリセットしてから描画する。
                    this._再生タイマ.リセットする( QPCTimer.秒をカウントに変換して返す( FDKUtilities.変換_100ns単位からsec単位へ( 次のフレームの表示予定時刻100ns ) ) );
                    this._次のフレームを描画する( dc, 変換行列2D, 不透明度0to1 );
                }
                else if( 次のフレームの表示予定時刻100ns <= this._再生タイマ.現在のリアルタイムカウント100ns )
                {
                    // (B) 次のフレームの表示時刻に達したので描画する。
                    this._次のフレームを描画する( dc, 変換行列2D, 不透明度0to1 );
                }
                else
                {
                    // (C) 次のフレームの表示時刻にはまだ達していない → 最後に描画したフレームを再描画しておく
                    this.最後のフレームを再描画する( dc, 変換行列2D, 不透明度0to1 );
                }
            }
            else
            {
                // (D) デコードが追い付いてない、またはループせず再生が終わっている　→ 何も表示しない。デコードが追い付いてないなら点滅するだろう。
            }
        }
        public void 最後のフレームを再描画する( DeviceContext1 dc, RectangleF 描画先矩形, float 不透明度0to1 = 1.0f )
        {
            if( null == this._VideoSource )
                return;

            var 変換行列2D =
                Matrix3x2.Scaling( 描画先矩形.Width / this._VideoSource.フレームサイズ.Width, 描画先矩形.Height / this._VideoSource.フレームサイズ.Height ) *  // 拡大縮小
                Matrix3x2.Translation( 描画先矩形.Left, 描画先矩形.Top );  // 平行移動

            this.最後のフレームを再描画する( dc, 変換行列2D, 不透明度0to1 );
        }
        public void 最後のフレームを再描画する( DeviceContext1 dc, Matrix3x2 変換行列2D, float 不透明度0to1 = 1.0f )
        {
            if( null != this._最後のフレーム )
                this._指定されたフレームを描画する( dc, 変換行列2D, this._最後のフレーム, 不透明度0to1 );
        }

        private IVideoSource _VideoSource = null;
        private MediaFoundationFileVideoSource _MFFileVideoSource = null;
        private VideoFrame _最後のフレーム = null;
        private QPCTimer _再生タイマ = null;

        private void _次のフレームを描画する( DeviceContext1 dc, Matrix3x2 変換行列2D, float 不透明度0to1 = 1.0f )
        {
            if( null == this._VideoSource )
                return;

            var 次のフレーム = this._VideoSource.Read();   // ブロックされたくない場合は先に Peek しておくこと。

            this._最後のフレーム?.Dispose();
            this._最後のフレーム = 次のフレーム;

            this._指定されたフレームを描画する( dc, 変換行列2D, 次のフレーム, 不透明度0to1 );
        }
        private void _指定されたフレームを描画する( DeviceContext1 dc, Matrix3x2 変換行列2D, VideoFrame 描画するフレーム, float 不透明度 )
        {
            if( null == 描画するフレーム )
                return;

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {
                dc.Transform = ( 変換行列2D ) * dc.Transform;
                dc.PrimitiveBlend = ( this.加算合成 ) ? PrimitiveBlend.Add : PrimitiveBlend.SourceOver;
                dc.DrawBitmap( 描画するフレーム.Bitmap, 不透明度, InterpolationMode.NearestNeighbor );
            } );
        }
    }
}
