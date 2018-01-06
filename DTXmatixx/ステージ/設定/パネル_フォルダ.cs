using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.設定
{
    /// <summary>
    ///		子パネルリストに入ることができるフォルダ。
    ///		子パネルリストの活性化・非活性化はこのクラスで行う。
    /// </summary>
    class パネル_フォルダ : パネル
    {
        /// <summary>
        ///		null ならルート階層。
        /// </summary>
        public パネル_フォルダ 親パネル
        {
            get;
            protected set;
        } = null;

        public SelectableList<パネル> 子パネルリスト
        {
            get
                => this._子パネルリスト;
            set
            {
                this._子パネルリスト = value;
                this._子パネルリスト.SelectFirst();
            }
        }

        public パネル_フォルダ( string パネル名, パネル_フォルダ 親パネル, IEnumerable<パネル> 初期子パネルリスト = null )
            : base( パネル名 )
        {
            this.親パネル = 親パネル;

            this.子パネルリスト = new SelectableList<パネル>();

            if( null != 初期子パネルリスト )
            {
                foreach( var panel in 初期子パネルリスト )
                    this.子パネルリスト.Add( panel );

                this._子パネルリスト.SelectFirst();
            }
        }

        protected override void On活性化( グラフィックデバイス gd )
        {
            base.On活性化( gd );   // 忘れないこと

            foreach( var panel in this.子パネルリスト )
                panel.活性化する( gd );
        }
        protected override void On非活性化( グラフィックデバイス gd )
        {
            foreach( var panel in this.子パネルリスト )
                panel.非活性化する( gd );

            base.On非活性化( gd );  // 忘れないこと
        }

        public override void 進行描画する( グラフィックデバイス gd, DeviceContext1 dc, float left, float top, bool 選択中 )
        {
            // パネルの共通部分を描画。
            base.進行描画する( gd, dc, left, top, 選択中 );

            // 項目部分はなし。
        }

        protected SelectableList<パネル> _子パネルリスト = null;
    }
}
