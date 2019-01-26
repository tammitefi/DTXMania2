using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using FDK;

namespace DTXMania.ステージ.演奏.EXPERT
{
    class ドラムキットとヒットバー : Activity
    {
        /// <summary>
        ///		0.0:閉じてる ～ 1.0:開いてる
        /// </summary>
        public float ハイハットの開度 { get; protected set; } = 1f;


        public ドラムキットとヒットバー()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._ドラムキット画像 = new テクスチャ( @"$(System)images\演奏\ドラムキットとヒットバー.png" ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 設定ファイルパス = new VariablePath( @"$(System)images\演奏\ドラムキットとヒットバー.yaml" );

                var yaml = File.ReadAllText( 設定ファイルパス.変数なしパス );
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                var yamlMap = deserializer.Deserialize<YAMLマップ>( yaml );

                this._パーツ画像の矩形リスト = new Dictionary<パーツ, RectangleF>();
                foreach( var kvp in yamlMap.矩形リスト )
                {
                    if( 4 == kvp.Value.Length )
                        this._パーツ画像の矩形リスト[ kvp.Key ] = new RectangleF( kvp.Value[ 0 ], kvp.Value[ 1 ], kvp.Value[ 2 ], kvp.Value[ 3 ] );
                }

                this._パーツ画像の中心位置 = new Dictionary<パーツ, (float X, float Y)>();
                foreach( var kvp in yamlMap.中心位置 )
                {
                    if( 2 == kvp.Value.Length )
                        this._パーツ画像の中心位置[ kvp.Key ] = (kvp.Value[ 0 ], kvp.Value[ 1 ]);
                }
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        /// <summary>
        ///		ベロシティ（開:0～80:閉）に応じたハイハット開度を設定する。
        ///		80超えの指定も可能。
        /// </summary>
        public void ハイハットの開度を設定する( int ベロシティ値 )
        {
            //  ベロシティ80超えはすべて1.0（完全閉じ）とする。
            this.ハイハットの開度 = 1f - ( Math.Min( ベロシティ値, 80 ) / 80f );
        }


        public void ドラムキットを進行描画する()
        {
            this._パーツを描画する( パーツ.Bass );
            this._パーツを描画する( パーツ.LowTom );
            this._パーツを描画する( パーツ.HiTom );
            this._パーツを描画する( パーツ.FloorTom );
            this._パーツを描画する( パーツ.Snare );
            this._パーツを描画する( パーツ.HiHatBottom );
            this._パーツを描画する( パーツ.HiHatTop, Y方向移動量: -20f * this.ハイハットの開度 );
            this._パーツを描画する( パーツ.RightCymbalStand );
            this._パーツを描画する( パーツ.RightCymbal );
            this._パーツを描画する( パーツ.RightCymbalTop );
            this._パーツを描画する( パーツ.LeftCymbalStand );
            this._パーツを描画する( パーツ.LeftCymbal );
            this._パーツを描画する( パーツ.LeftCymbalTop );
        }

        public void ヒットバーを進行描画する()
        {
            this._パーツを描画する( パーツ.Bar );
        }


        protected テクスチャ _ドラムキット画像;

        protected Dictionary<パーツ, RectangleF> _パーツ画像の矩形リスト = null;

        protected Dictionary<パーツ, (float X, float Y)> _パーツ画像の中心位置 = null;


        private void _パーツを描画する( パーツ パーツ名, float X方向移動量 = 0f, float Y方向移動量 = 0f )
        {
            var 中心位置 = this._パーツ画像の中心位置[ パーツ名 ];
            var srcRect = this._パーツ画像の矩形リスト[ パーツ名 ];

            this._ドラムキット画像.描画する(
                中心位置.X - srcRect.Width / 2 + X方向移動量,
                中心位置.Y - srcRect.Height / 2 + Y方向移動量,
                転送元矩形: srcRect );
        }


        protected enum パーツ
        {
            LeftCymbalStand,
            LeftCymbal,
            LeftCymbalTop,
            RightCymbalStand,
            RightCymbal,
            RightCymbalTop,
            HiHatBottom,
            HiHatTop,
            Bass,
            Snare,
            HiTom,
            LowTom,
            FloorTom,
            Bar,
        }

        private class YAMLマップ
        {
            public Dictionary<パーツ, float[]> 矩形リスト { get; set; }
            public Dictionary<パーツ, float[]> 中心位置 { get; set; }
        }
    }
}
