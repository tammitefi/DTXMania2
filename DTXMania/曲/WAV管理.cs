﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSCore;
using FDK;
using SSTFormat.v4;
using DTXMania.設定;

namespace DTXMania.曲
{
    /// <summary>
    ///		<see cref="スコア.WAVリスト"/> の各サウンドインスタンスを管理する。
    ///		サウンドの作成には <see cref="App.WAVキャッシュレンタル"/> を使用する。
    /// </summary>
    class WAV管理 : IDisposable
    {
        /// <param name="多重度">
        ///     １サウンドの最大多重発声数。1以上。
        /// </param>
        public WAV管理( int 多重度 = 4 )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                if( 1 > 多重度 )
                    throw new ArgumentException( $"多重度が1未満に設定されています。[{多重度}]" );

                this._既定の多重度 = 多重度;

                this._WAV情報リスト = new Dictionary<int, WAV情報>();
            }
        }

        public void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                foreach( var kvp in this._WAV情報リスト )
                    kvp.Value.Dispose();

                this._WAV情報リスト = null;
            }
        }

        /// <summary>
        ///		指定したWAV番号にサウンドファイルを登録する。
        /// </summary>
        /// <param name="wav番号">登録する番号。0～1295。すでに登録されている場合は上書き更新される。</param>
        /// <param name="サウンドファイル">登録するサウンドファイルのパス。</param>
        /// <remarks>
        ///     サウンドの生成に失敗した場合には登録を行わない。
        /// </remarks>
        public void 登録する( SoundDevice device, int wav番号, VariablePath サウンドファイル, bool 多重再生する, bool BGMである )
        {
            #region " パラメータチェック。"
            //----------------
            if( null == device )
                throw new ArgumentNullException();

            if( ( 0 > wav番号 ) || ( 36 * 36 <= wav番号 ) )
                throw new ArgumentOutOfRangeException( $"WAV番号が範囲(0～1295)を超えています。[{wav番号}]" );

            if( !( File.Exists( サウンドファイル.変数なしパス ) ) )
            {
                Log.WARNING( $"サウンドファイルが存在しません。[{サウンドファイル.変数付きパス}]" );
                return;
            }
            //----------------
            #endregion

            // 先に ISampleSource を生成する。

            var サンプルソース = App.WAVキャッシュレンタル.作成する( サウンドファイル );

            if( null == サンプルソース )
            {
                Log.WARNING( $"サウンドのデコードに失敗しました。[{サウンドファイル.変数付きパス}" );
                return;
            }

            // サウンドを登録する。

            if( this._WAV情報リスト.ContainsKey( wav番号 ) )
                this._WAV情報リスト[ wav番号 ].Dispose();  // すでに登録済みなら解放する。

            int 多重度 = ( 多重再生する ) ? this._既定の多重度 : 1;

            this._WAV情報リスト[ wav番号 ] = new WAV情報( wav番号, 多重度, BGMである );
            this._WAV情報リスト[ wav番号 ].サウンドを生成する( device, サンプルソース );

            Log.Info( $"サウンドを読み込みました。[{サウンドファイル.変数付きパス}]" );
        }

        /// <summary>
        ///		指定した番号のWAVを、指定したチップ種別として発声する。
        /// </summary>
        /// <param name="音量">0:無音～1:原音</param>
        public void 発声する( int WAV番号, チップ種別 chipType, bool 発声前に消音する, 消音グループ種別 muteGroupType, bool BGM以外も再生する, float 音量 = 1f, double 再生開始時刻sec = 0.0 )
        {
            if( !( this._WAV情報リスト.ContainsKey( WAV番号 ) ) ||
                ( !( BGM以外も再生する ) && !( this._WAV情報リスト[ WAV番号 ].BGMである ) ) )
                return;


            // 消音する（必要あれば）。

            if( 発声前に消音する && muteGroupType != 消音グループ種別.Unknown )
            {
                // 発生時に指定された消音グループ種別に属するWAVサウンドをすべて停止する。
                var 停止するWavContexts = this._WAV情報リスト.Where( ( kvp ) => ( kvp.Value.最後に発声したときの消音グループ種別 == muteGroupType ) );

                foreach( var kvp in 停止するWavContexts )
                    kvp.Value.発声を停止する();
            }


            // 発声する。

            this._WAV情報リスト[ WAV番号 ].発声する( muteGroupType, 音量, 再生開始時刻sec );
        }

        public void すべての発声を停止する()
        {
            foreach( var kvp in this._WAV情報リスト )
                kvp.Value.発声を停止する();
        }


        private readonly int _既定の多重度;

        /// <summary>
        ///		全WAVの管理DB。KeyはWAV番号。
        /// </summary>
        private Dictionary<int, WAV情報> _WAV情報リスト = null;

        /// <summary>
        ///		WAV ごとの管理情報。
        /// </summary>
        private class WAV情報 : IDisposable
        {
            /// <summary>
            ///		0～1295。
            /// </summary>
            public int WAV番号;

            /// <summary>
            ///		この WAV に対応するサウンド。
            /// </summary>
            /// <remarks>
            ///		サウンドデータとして <see cref="SampleSource"/> を使用し、多重度の数だけ存在することができる。
            /// </remarks>
            public Sound[] Sounds;

            public 消音グループ種別 最後に発声したときの消音グループ種別 { get; protected set; } = 消音グループ種別.Unknown;

            public bool BGMである = false;


            public WAV情報( int wav番号, int 多重度, bool BGMである )
            {
                if( ( 0 > wav番号 ) || ( 36 * 36 <= wav番号 ) )
                    throw new ArgumentOutOfRangeException( "WAV番号が不正です。" );

                this.WAV番号 = wav番号;
                this.Sounds = new Sound[ 多重度 ];
                this.BGMである = BGMである;
            }

            public void Dispose()
            {
                if( null != this.Sounds )
                {
                    foreach( var sd in this.Sounds )
                        sd.Dispose();
                    this.Sounds = null;
                }
            }


            /// <summary>
            ///     多重度の数だけ Sound を生成する。ただしソースは共通。
            /// </summary>
            public void サウンドを生成する( SoundDevice device, ISampleSource sampleSource )
            {
                for( int i = 0; i < this.Sounds.Length; i++ )
                    this.Sounds[ i ] = new Sound( device, sampleSource );
            }

            /// <summary>
            ///		指定したチップ種別扱いでWAVサウンドを発声する。
            /// </summary>
            /// <param name="音量">0:無音～1:原音</param>
            public void 発声する( 消音グループ種別 muteGroupType, float 音量, double 再生開始時刻sec = 0.0 )
            {
                this.最後に発声したときの消音グループ種別 = muteGroupType;

                // 発声。
                音量 =
                    ( 0f > 音量 ) ? 0f : 
                    ( 1f < 音量 ) ? 1f : 音量;
                this.Sounds[ this._次に再生するSound番号 ].Volume = 音量;
                this.Sounds[ this._次に再生するSound番号 ].Play( 再生開始時刻sec );

                // サウンドローテーション。
                this._次に再生するSound番号 = ( this._次に再生するSound番号 + 1 ) % this.Sounds.Length;
            }

            public void 発声を停止する()
            {
                foreach( var sound in this.Sounds )
                    sound.Stop();
            }


            private int _次に再生するSound番号 = 0;
        }
    }
}
