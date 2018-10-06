using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using SharpDX.Direct2D1;
using FDK.メディア;
using FDK.カウンタ;

namespace FDK.UI
{
    /// <summary>
    ///		ウィンドウ。
    /// </summary>
    /// <remarks>
    ///		指定された画像を3x3に9分割して、それを使ってウィンドウを描画する。
    ///		１　２　３	分割は、縦横すべて等サイズ。単純に、画像のサイズを縦横3で割った値。例えば、画像が48x48の場合、１～９の各サイズは16×16になる。3で割り切れないなら正しい描画は保証しない。
    ///		４　５　６	ウィンドウの拡大縮小は、中央（２・４・６・８）の部分を伸縮して実現する。（四隅１・３・７・９は伸縮しない。）
    ///		７　８　９	四隅（１・３・７・９）は必ず描画されるので、２・４・５・６・８のサイズがゼロのときのサイズが、そのウィンドウの最小サイズとなる。
    /// </remarks>
    public class Window : Element
    {
        public Size2F サイズdpx_枠込み
            => new Size2F(
                this.サイズdpx.Width + this._最大左枠幅dpx + this._最大右枠幅dpx,
                this.サイズdpx.Height + this._最大上枠高さdpx + this._最大下枠高さdpx );

        /// <summary>
        ///     クライアント領域から、（画像デザイン上の）枠のふちまでの距離[dpx]。
        ///     Top, Bottom, Left, Right メンバがそれぞれ上下左右のマージンを表す（矩形としての意味はない）。
        /// </summary>
        public RectangleF マージンdpx
        {
            get;
            protected set;
        }

        /// <summary>
        ///     ウィンドウ矩形は、クライアント矩形に上下左右の枠を足したサイズに修正。
        /// </summary>
        public override RectangleF ウィンドウ矩形dpx
            => new RectangleF(
                this.クライアント矩形のスクリーン座標dpx.X - this._最大左枠幅dpx,
                this.クライアント矩形のスクリーン座標dpx.Y - this._最大上枠高さdpx,
                this.クライアント矩形のスクリーン座標dpx.Width + this._最大左枠幅dpx + this._最大右枠幅dpx,
                this.クライアント矩形のスクリーン座標dpx.Height + this._最大上枠高さdpx + this._最大下枠高さdpx );

        /// <summary>
        ///		ウィンドウの画像とサイズを設定する。
        /// </summary>
        /// <param name="画像ファイル">ウィンドウを3x3に9分割したベース画像。</param>
        /// <param name="セル矩形ファイル">ベース画像を3x3に分割する矩形リストファイル。</param>
        /// <param name="サイズdpx">ウィンドウのクライアント領域のサイズ[dpx]。ウィンドウの枠部分は含まれない。</param>
        /// <param name="位置dpx">ウィンドウの表示位置[dpx]。親要素のクライアント領域からの相対座標。</param>
        public Window( VariablePath 画像ファイル, 矩形リスト セル矩形, Size2F サイズdpx, Vector2? 位置dpx = null, VariablePath 押下時画像ファイル = null, VariablePath フォーカス時画像ファイル = null )
            : base( サイズdpx, 位置dpx )
        {
            this.広い部分の描画方法 = 広い部分の描画方法種別.タイル描画;

            this.セル矩形 = セル矩形;

            this._左上矩形 = this.セル矩形[ "LeftTop" ].Value;
            this._上辺矩形 = this.セル矩形[ "Top" ].Value;
            this._右上矩形 = this.セル矩形[ "RightTop" ].Value;
            this._左辺矩形 = this.セル矩形[ "Left" ].Value;
            this._中央矩形 = this.セル矩形[ "Center" ].Value;
            this._右辺矩形 = this.セル矩形[ "Right" ].Value;
            this._左下矩形 = this.セル矩形[ "LeftBottom" ].Value;
            this._下辺矩形 = this.セル矩形[ "Bottom" ].Value;
            this._右下矩形 = this.セル矩形[ "RightBottom" ].Value;

            this.マージンdpx = 矩形リスト.Margin変換( this.セル矩形[ "Margin" ].Value );

            this._最大上枠高さdpx = Math.Max( this._左上矩形.Height, Math.Max( this._上辺矩形.Height, this._右上矩形.Height ) );
            this._最大下枠高さdpx = Math.Max( this._左下矩形.Height, Math.Max( this._下辺矩形.Height, this._右下矩形.Height ) );
            this._最大左枠幅dpx = Math.Max( this._左上矩形.Width, Math.Max( this._左辺矩形.Width, this._左下矩形.Width ) );
            this._最大右枠幅dpx = Math.Max( this._右上矩形.Width, Math.Max( this._右辺矩形.Width, this._右下矩形.Width ) );

            this.子を追加する( this.通常時セル画像 = new 画像( 画像ファイル ) );

            if( null != 押下時画像ファイル )
                this.子を追加する( this.押下時セル画像 = new 画像( 押下時画像ファイル ) );   // オプション

            if( null != フォーカス時画像ファイル )
                this.子を追加する( this.フォーカス時セル画像 = new 画像( フォーカス時画像ファイル ) );    // オプション

            this.子を追加する( this.全体画像 = new 描画可能画像( this.サイズdpx_枠込み ) );
        }

        protected override void On活性化()
        {
            var am = グラフィックデバイス.Instance.Animation.Manager;

            this._通常時セル不透明度 = new Variable( am, initialValue: 1.0 );
            this._押下時セル不透明度 = new Variable( am, initialValue: 0.0 );
            this._フォーカス時セル不透明度 = new Variable( am, initialValue: 0.0 );
            this._不透明度のストーリーボード = null;

            base.On活性化();
        }
        protected override void On非活性化()
        {
            this._不透明度のストーリーボード?.Abandon();

            this._不透明度のストーリーボード?.Dispose();
            this._不透明度のストーリーボード = null;

            this._通常時セル不透明度?.Dispose();
            this._通常時セル不透明度 = null;

            this._押下時セル不透明度?.Dispose();
            this._押下時セル不透明度 = null;

            this._フォーカス時セル不透明度?.Dispose();
            this._フォーカス時セル不透明度 = null;

            base.On非活性化();
        }

        /// <summary>
        ///     指定した位置が自要素のクライアント矩形に収まっていれば true を返す。
        /// </summary>
        /// <param name="位置dpx">
        ///     調査する位置。スクリーン座標[dpx]。
        /// </param>
        public override bool ヒットチェック( Vector2 位置dpx )
        {
            var clientRect = this.クライアント矩形のスクリーン座標dpx;

            // 実際に目に見える部分（クライアント矩形＋マージン）でヒットチェックを行うよう修正。
            var visibleWinRect = new RectangleF(
                clientRect.X - this.マージンdpx.Left,
                clientRect.Y - this.マージンdpx.Top,
                clientRect.Width + this.マージンdpx.Left + this.マージンdpx.Right,
                clientRect.Height + this.マージンdpx.Top + this.マージンdpx.Bottom );

            return visibleWinRect.Contains( 位置dpx );
        }

        protected 画像 通常時セル画像 = null;
        protected 画像 押下時セル画像 = null;
        protected 画像 フォーカス時セル画像 = null;
        protected 描画可能画像 全体画像 = null;
        protected 矩形リスト セル矩形 = null;

        /// <summary>
        ///     上下左右の辺と中央のサイズが、セル矩形で指定されたものより大きかったり小さかったりしたときに、どのように描画するかを指定する。
        /// </summary>
        protected enum 広い部分の描画方法種別
        {
            タイル描画,
            拡大縮小描画,
        }
        protected 広い部分の描画方法種別 広い部分の描画方法 = 広い部分の描画方法種別.タイル描画;

        private RectangleF _左上矩形;
        private RectangleF _上辺矩形;
        private RectangleF _右上矩形;
        private RectangleF _左辺矩形;
        private RectangleF _中央矩形;
        private RectangleF _右辺矩形;
        private RectangleF _左下矩形;
        private RectangleF _下辺矩形;
        private RectangleF _右下矩形;
        protected float _最大上枠高さdpx;
        protected float _最大下枠高さdpx;
        protected float _最大左枠幅dpx;
        protected float _最大右枠幅dpx;

        protected override void OnPaint( DCEventArgs e )
        {
            Debug.Assert( this.活性化している );
            base.OnPaint( e );

            // セル画像を直接画面に描画すると（設計画面から物理画面への変換で）ズレるので、全体画像にセルを全部描画し、それから全体画像を描画する。

            if( this.サイズdpx_枠込み != this.全体画像.サイズ )
            {
                this.全体画像.非活性化する();
                this.子を削除する( this.全体画像 );
                this.子を追加する( this.全体画像 = new 描画可能画像( this.サイズdpx_枠込み ) );
                this.全体画像.活性化する();
            }

            this.全体画像.画像へ描画する( ( dcw ) => {

                dcw.Clear( Color.Transparent );

                // 以下の画像を、以下の順番に、以下の不透明度で描画する。
                var 画像s = new[] {
                    ( セル画像: this.通常時セル画像, 不透明度: (float) this._通常時セル不透明度.Value ),
                    ( セル画像: this.押下時セル画像, 不透明度: (float) this._押下時セル不透明度.Value ),
                    ( セル画像: this.フォーカス時セル画像, 不透明度: (float) this._フォーカス時セル不透明度.Value ),
                };

                for( int i = 0; i < 画像s.Length; i++ )
                {
                    if( ( null == 画像s[ i ].セル画像 ) || ( 0f == 画像s[ i ].不透明度 ) )
                        continue;

                    #region " 中央 "
                    //----------------
                    {
                        var 描画領域 = new RectangleF(
                            this._最大左枠幅dpx,
                            this._最大上枠高さdpx,
                            this.サイズdpx.Width,
                            this.サイズdpx.Height );

                        if( ( 0f < 描画領域.Width ) && ( 0f < 描画領域.Height ) )
                        {
                            switch( this.広い部分の描画方法 )
                            {
                                case 広い部分の描画方法種別.タイル描画:

                                    dcw.PushAxisAlignedClip( 描画領域, AntialiasMode.PerPrimitive );

                                    for( float x = 描画領域.Left; x < 描画領域.Right; x += this._中央矩形.Width )
                                    {
                                        for( float y = 描画領域.Top; y < 描画領域.Bottom; y += this._中央矩形.Height )
                                        {
                                            画像s[ i ].セル画像.描画する(
                                                dcw,
                                                左位置: x,
                                                上位置: y,
                                                転送元矩形: this._中央矩形,
                                                不透明度0to1: 画像s[ i ].不透明度,
                                                描画先矩形を整数境界に合わせる: true );
                                        }
                                    }

                                    dcw.PopAxisAlignedClip();

                                    break;

                                case 広い部分の描画方法種別.拡大縮小描画:

                                    画像s[ i ].セル画像.描画する(
                                        dcw,
                                        左位置: 描画領域.Left,
                                        上位置: 描画領域.Top,
                                        X方向拡大率: ( 描画領域.Width / this._中央矩形.Width ),
                                        Y方向拡大率: ( 描画領域.Height / this._中央矩形.Height ),
                                        転送元矩形: this._中央矩形,
                                        不透明度0to1: 画像s[ i ].不透明度,
                                        描画先矩形を整数境界に合わせる: true );

                                    break;
                            }
                        }
                    }
                    //----------------
                    #endregion
                    #region " 左上隅 "
                    //----------------
                    {
                        var 描画位置 = new Vector2(
                            this._最大左枠幅dpx - this._左上矩形.Width,
                            this._最大上枠高さdpx - this._左上矩形.Height );

                        switch( this.広い部分の描画方法 )
                        {
                            case 広い部分の描画方法種別.タイル描画:
                            case 広い部分の描画方法種別.拡大縮小描画:
                                画像s[ i ].セル画像.描画する(
                                    dcw,
                                    左位置: 描画位置.X,
                                    上位置: 描画位置.Y,
                                    転送元矩形: this._左上矩形,
                                    不透明度0to1: 画像s[ i ].不透明度,
                                    描画先矩形を整数境界に合わせる: true );
                                break;
                        }
                    }
                    //----------------
                    #endregion
                    #region " 上辺 "
                    //----------------
                    {
                        var 描画領域 = new RectangleF(
                            this._最大左枠幅dpx,
                            this._最大上枠高さdpx - this._上辺矩形.Height,
                            this.サイズdpx.Width,
                            this._上辺矩形.Height );

                        if( ( 0f < 描画領域.Width ) && ( 0f < 描画領域.Height ) )
                        {
                            switch( this.広い部分の描画方法 )
                            {
                                case 広い部分の描画方法種別.タイル描画:

                                    dcw.PushAxisAlignedClip( 描画領域, AntialiasMode.PerPrimitive );

                                    for( float x = 描画領域.Left; x < 描画領域.Right; x += this._上辺矩形.Width )
                                    {
                                        画像s[ i ].セル画像.描画する(
                                            dcw,
                                            左位置: x,
                                            上位置: 描画領域.Y,
                                            転送元矩形: this._上辺矩形,
                                            不透明度0to1: 画像s[ i ].不透明度,
                                            描画先矩形を整数境界に合わせる: true );
                                    }

                                    dcw.PopAxisAlignedClip();

                                    break;

                                case 広い部分の描画方法種別.拡大縮小描画:

                                    画像s[ i ].セル画像.描画する(
                                        dcw,
                                        左位置: 描画領域.Left,
                                        上位置: 描画領域.Top,
                                        X方向拡大率: ( 描画領域.Width / this._上辺矩形.Width ),
                                        Y方向拡大率: ( 描画領域.Height / this._上辺矩形.Height ),
                                        転送元矩形: this._上辺矩形,
                                        不透明度0to1: 画像s[ i ].不透明度,
                                        描画先矩形を整数境界に合わせる: true );

                                    break;
                            }
                        }
                    }
                    //----------------
                    #endregion
                    #region " 右上隅 "
                    //----------------
                    {
                        var 描画位置 = new Vector2(
                            this.全体画像.サイズ.Width - this._最大右枠幅dpx,
                            this._最大上枠高さdpx - this._右上矩形.Height );

                        switch( this.広い部分の描画方法 )
                        {
                            case 広い部分の描画方法種別.タイル描画:
                            case 広い部分の描画方法種別.拡大縮小描画:
                                画像s[ i ].セル画像.描画する(
                                    dcw,
                                    左位置: 描画位置.X,
                                    上位置: 描画位置.Y,
                                    転送元矩形: this._右上矩形,
                                    不透明度0to1: 画像s[ i ].不透明度,
                                    描画先矩形を整数境界に合わせる: true );
                                break;
                        }
                    }
                    //----------------
                    #endregion
                    #region " 左下隅 "
                    //----------------
                    {
                        var 描画位置 = new Vector2(
                            this._最大左枠幅dpx - this._左下矩形.Width,
                            this.全体画像.サイズ.Height - this._最大下枠高さdpx );

                        switch( this.広い部分の描画方法 )
                        {
                            case 広い部分の描画方法種別.タイル描画:
                            case 広い部分の描画方法種別.拡大縮小描画:
                                画像s[ i ].セル画像.描画する(
                                    dcw,
                                    左位置: 描画位置.X,
                                    上位置: 描画位置.Y,
                                    転送元矩形: this._左下矩形,
                                    不透明度0to1: 画像s[ i ].不透明度,
                                    描画先矩形を整数境界に合わせる: true );
                                break;
                        }
                    }
                    //----------------
                    #endregion
                    #region " 下辺 "
                    //----------------
                    {
                        var 描画領域 = new RectangleF(
                            this._最大左枠幅dpx,
                            this.全体画像.サイズ.Height - this._最大下枠高さdpx,
                            this.サイズdpx.Width,
                            this._下辺矩形.Height );

                        if( ( 0f < 描画領域.Width ) && ( 0f < 描画領域.Height ) )
                        {
                            switch( this.広い部分の描画方法 )
                            {
                                case 広い部分の描画方法種別.タイル描画:

                                    dcw.PushAxisAlignedClip( 描画領域, AntialiasMode.PerPrimitive );

                                    for( float x = 描画領域.Left; x < 描画領域.Right; x += this._上辺矩形.Width )
                                    {
                                        画像s[ i ].セル画像.描画する(
                                            dcw,
                                            左位置: x,
                                            上位置: 描画領域.Y,
                                            転送元矩形: this._下辺矩形,
                                            不透明度0to1: 画像s[ i ].不透明度,
                                            描画先矩形を整数境界に合わせる: true );
                                    }

                                    dcw.PopAxisAlignedClip();

                                    break;

                                case 広い部分の描画方法種別.拡大縮小描画:

                                    画像s[ i ].セル画像.描画する(
                                        dcw,
                                        左位置: 描画領域.Left,
                                        上位置: 描画領域.Top,
                                        X方向拡大率: ( 描画領域.Width / this._下辺矩形.Width ),
                                        Y方向拡大率: ( 描画領域.Height / this._下辺矩形.Height ),
                                        転送元矩形: this._下辺矩形,
                                        不透明度0to1: 画像s[ i ].不透明度,
                                        描画先矩形を整数境界に合わせる: true );

                                    break;
                            }
                        }
                    }
                    //----------------
                    #endregion
                    #region " 右下隅 "
                    //----------------
                    {
                        var 描画位置 = new Vector2(
                            this.全体画像.サイズ.Width - this._最大右枠幅dpx,
                            this.全体画像.サイズ.Height - this._最大下枠高さdpx );

                        switch( this.広い部分の描画方法 )
                        {
                            case 広い部分の描画方法種別.タイル描画:
                            case 広い部分の描画方法種別.拡大縮小描画:
                                画像s[ i ].セル画像.描画する(
                                    dcw,
                                    左位置: 描画位置.X,
                                    上位置: 描画位置.Y,
                                    転送元矩形: this._右下矩形,
                                    不透明度0to1: 画像s[ i ].不透明度,
                                    描画先矩形を整数境界に合わせる: true );
                                break;
                        }
                    }
                    //----------------
                    #endregion
                    #region " 左辺 "
                    //----------------
                    {
                        var 描画領域 = new RectangleF(
                            this._最大左枠幅dpx - this._左辺矩形.Width,
                            this._最大上枠高さdpx,
                            this._左辺矩形.Width,
                            this.サイズdpx.Height );

                        if( ( 0f < 描画領域.Width ) && ( 0f < 描画領域.Height ) )
                        {
                            switch( this.広い部分の描画方法 )
                            {
                                case 広い部分の描画方法種別.タイル描画:

                                    dcw.PushAxisAlignedClip( 描画領域, AntialiasMode.PerPrimitive );

                                    for( float y = 描画領域.Top; y < 描画領域.Bottom; y += this._左辺矩形.Height )
                                    {
                                        画像s[ i ].セル画像.描画する(
                                            dcw,
                                            左位置: 描画領域.X,
                                            上位置: y,
                                            転送元矩形: this._左辺矩形,
                                            不透明度0to1: 画像s[ i ].不透明度,
                                            描画先矩形を整数境界に合わせる: true );
                                    }

                                    dcw.PopAxisAlignedClip();

                                    break;

                                case 広い部分の描画方法種別.拡大縮小描画:

                                    画像s[ i ].セル画像.描画する(
                                        dcw,
                                        左位置: 描画領域.Left,
                                        上位置: 描画領域.Top,
                                        X方向拡大率: ( 描画領域.Width / this._左辺矩形.Width ),
                                        Y方向拡大率: ( 描画領域.Height / this._左辺矩形.Height ),
                                        転送元矩形: this._左辺矩形,
                                        不透明度0to1: 画像s[ i ].不透明度,
                                        描画先矩形を整数境界に合わせる: true );

                                    break;
                            }
                        }
                    }
                    //----------------
                    #endregion
                    #region " 右辺 "
                    //----------------
                    {
                        var 描画領域 = new RectangleF(
                            this.全体画像.サイズ.Width - this._最大右枠幅dpx,
                            this._最大上枠高さdpx,
                            this._右辺矩形.Width,
                            this.サイズdpx.Height );

                        if( ( 0f < 描画領域.Width ) && ( 0f < 描画領域.Height ) )
                        {
                            switch( this.広い部分の描画方法 )
                            {
                                case 広い部分の描画方法種別.タイル描画:

                                    dcw.PushAxisAlignedClip( 描画領域, AntialiasMode.PerPrimitive );

                                    for( float y = 描画領域.Top; y < 描画領域.Bottom; y += this._右辺矩形.Height )
                                    {
                                        画像s[ i ].セル画像.描画する(
                                            dcw,
                                            左位置: 描画領域.X,
                                            上位置: y,
                                            転送元矩形: this._右辺矩形,
                                            不透明度0to1: 画像s[ i ].不透明度,
                                            描画先矩形を整数境界に合わせる: true );
                                    }

                                    dcw.PopAxisAlignedClip();

                                    break;

                                case 広い部分の描画方法種別.拡大縮小描画:

                                    画像s[ i ].セル画像.描画する(
                                        dcw,
                                        左位置: 描画領域.Left,
                                        上位置: 描画領域.Top,
                                        X方向拡大率: ( 描画領域.Width / this._右辺矩形.Width ),
                                        Y方向拡大率: ( 描画領域.Height / this._右辺矩形.Height ),
                                        転送元矩形: this._右辺矩形,
                                        不透明度0to1: 画像s[ i ].不透明度,
                                        描画先矩形を整数境界に合わせる: true );

                                    break;
                            }
                        }
                    }
                    //----------------
                    #endregion
                }

            } );

            this.全体画像.描画する( e.dc, this.ウィンドウ矩形dpx.X, this.ウィンドウ矩形dpx.Y );
        }
        protected override void OnMouseEnter( UIEventArgs e )
        {
            base.OnMouseEnter( e );

            if( null != this.フォーカス時セル画像 )
            {
                // フォーカス状態へ遷移
                this._不透明度アニメーション開始( 0.0, 0.0, 1.0 );
            }
        }
        protected override void OnMouseLeave( UIEventArgs e )
        {
            base.OnMouseLeave( e );

            // 通常状態へ遷移
            this._不透明度アニメーション開始( 1.0, 0.0, 0.0 );
        }
        protected override bool OnMouseDown( UIMouseEventArgs e )
        {
            if( null != this.押下時セル画像 )
            {
                var am = グラフィックデバイス.Instance.Animation.Manager;

                // 押下状態へ遷移。アニメなしで即終値。
                this._不透明度のストーリーボード?.Abandon();
                this._不透明度のストーリーボード?.Dispose();
                this._不透明度のストーリーボード = null;
                this._通常時セル不透明度?.Dispose();
                this._通常時セル不透明度 = new Variable( am, initialValue: 0.0 );
                this._押下時セル不透明度?.Dispose();
                this._押下時セル不透明度 = new Variable( am, initialValue: 1.0 );
                this._フォーカス時セル不透明度?.Dispose();
                this._フォーカス時セル不透明度 = new Variable( am, initialValue: 0.0 );
            }

            return base.OnMouseDown( e );
        }
        protected override bool OnMouseUp( UIMouseEventArgs e )
        {
            var am = グラフィックデバイス.Instance.Animation.Manager;

            // アニメなしで即終値
            this._不透明度のストーリーボード?.Abandon();
            this._不透明度のストーリーボード?.Dispose();
            this._不透明度のストーリーボード = null;
            this._通常時セル不透明度?.Dispose();
            this._押下時セル不透明度?.Dispose();
            this._フォーカス時セル不透明度?.Dispose();

            if( null != this.フォーカス時セル画像 )
            {
                // フォーカス状態へ遷移。
                this._通常時セル不透明度 = new Variable( am, initialValue: 0.0 );
                this._押下時セル不透明度 = new Variable( am, initialValue: 0.0 );
                this._フォーカス時セル不透明度 = new Variable( am, initialValue: 1.0 );
            }
            else
            {
                // 通常状態へ遷移。
                this._通常時セル不透明度 = new Variable( am, initialValue: 1.0 );
                this._押下時セル不透明度 = new Variable( am, initialValue: 0.0 );
                this._フォーカス時セル不透明度 = new Variable( am, initialValue: 0.0 );
            }

            return base.OnMouseUp( e );
        }

        private Variable _通常時セル不透明度 = null;
        private Variable _押下時セル不透明度 = null;
        private Variable _フォーカス時セル不透明度 = null;
        private Storyboard _不透明度のストーリーボード = null;

        private void _不透明度アニメーション開始( double 通常時不透明度の終値, double 押下時不透明度の終値, double フォーカス時不透明度の終値 )
        {
            double 秒( double sec ) => sec;

            var animation = グラフィックデバイス.Instance.Animation;

            using( var 通常時セル不透明度の遷移 = animation.TrasitionLibrary.Linear( 秒( 0.25 ), finalValue: 通常時不透明度の終値 ) )
            using( var 押下時セル不透明度の遷移 = animation.TrasitionLibrary.Linear( 秒( 0.25 ), finalValue: 押下時不透明度の終値 ) )
            using( var フォーカス時セル不透明度の遷移 = animation.TrasitionLibrary.Linear( 秒( 0.25 ), finalValue: フォーカス時不透明度の終値 ) )
            {
                this._不透明度のストーリーボード?.Abandon();
                this._不透明度のストーリーボード?.Dispose();
                this._不透明度のストーリーボード = new Storyboard( animation.Manager );
                this._不透明度のストーリーボード.AddTransition( this._通常時セル不透明度, 通常時セル不透明度の遷移 );
                this._不透明度のストーリーボード.AddTransition( this._押下時セル不透明度, 押下時セル不透明度の遷移 );
                this._不透明度のストーリーボード.AddTransition( this._フォーカス時セル不透明度, フォーカス時セル不透明度の遷移 );
            }

            this._不透明度のストーリーボード.Schedule( animation.Timer.Time );
        }
    }
}
