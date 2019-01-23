using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using FDK;

namespace DTXMania.ステージ.演奏
{
    class ドラムキットとヒットバー : Activity
    {
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

                this._パーツ画像の矩形リスト = new Dictionary<string, RectangleF>();
                foreach( var kvp in yamlMap.矩形リスト )
                {
                    if( 4 == kvp.Value.Length )
                        this._パーツ画像の矩形リスト[ kvp.Key ] = new RectangleF( kvp.Value[ 0 ], kvp.Value[ 1 ], kvp.Value[ 2 ], kvp.Value[ 3 ] );
                }

                this._パーツ画像の中心位置 = new Dictionary<string, (float x, float y)>();
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

        public void ドラムキットを進行描画する()
        {
            this._パーツを描画する( "Bass" );
            this._パーツを描画する( "LowTom" );
            this._パーツを描画する( "HiTom" );
            this._パーツを描画する( "FloorTom" );
            this._パーツを描画する( "Snare" );
            this._パーツを描画する( "HiHatBottom" );
            this._パーツを描画する( "HiHatTop" );
            this._パーツを描画する( "RightCymbalStand" );
            this._パーツを描画する( "RightCymbal" );
            this._パーツを描画する( "RightCymbalTop" );
            this._パーツを描画する( "LeftCymbalStand" );
            this._パーツを描画する( "LeftCymbal" );
            this._パーツを描画する( "LeftCymbalTop" );
        }

        public void ヒットバーを進行描画する()
        {
            this._パーツを描画する( "Bar" );
        }


        protected テクスチャ _ドラムキット画像;

        protected Dictionary<string, RectangleF> _パーツ画像の矩形リスト = null;

        protected Dictionary<string, (float X, float Y)> _パーツ画像の中心位置 = null;


        private void _パーツを描画する( string パーツ名 )
        {
            var dstOffset = this._パーツ画像の中心位置[ パーツ名 ];
            var srcRect = this._パーツ画像の矩形リスト[ パーツ名 ];

            this._ドラムキット画像.描画する(
                dstOffset.X - srcRect.Width / 2,
                dstOffset.Y - srcRect.Height / 2,
                転送元矩形: srcRect );
        }


        private class YAMLマップ
        {
            public Dictionary<string, float[]> 矩形リスト { get; set; }
            public Dictionary<string, float[]> 中心位置 { get; set; }
        }
    }
}
