using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSCore;
using FDK;
using FDK.メディア.サウンド;
using SSTFormat.v3;
using DTXmatixx.設定;

namespace DTXmatixx.ステージ
{
    class ドラムサウンド : IDisposable
    {
        public ドラムサウンド()
        {
        }
        public void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                lock( this._Sound利用権 )
                {
                    if( null != this._チップtoコンテキスト )
                    {
                        foreach( var kvp in this._チップtoコンテキスト )
                            kvp.Value.Dispose();
                        this._チップtoコンテキスト.Clear();
                        this._チップtoコンテキスト = null;
                    }
                }
            }
        }
        /// <summary>
        ///		サブチップID = 0（SSTの規定ドラムサウンド）以外をクリアする。
        /// </summary>
        public void 初期化する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                lock( this._Sound利用権 )
                {
                    if( null != this._チップtoコンテキスト )
                    {
                        foreach( var kvp in this._チップtoコンテキスト )
                            kvp.Value.Dispose();
                    }

                    this._チップtoコンテキスト = new Dictionary<(チップ種別 chipType, int サブチップID), SoundContext>();

                    // SSTの既定のサウンドを、subChipId = 0 としてプリセット登録する。
                    this.登録する( チップ種別.LeftCrash, 0, @"$(System)sounds\drums\LeftCrash.wav" );
                    this.登録する( チップ種別.Ride, 0, @"$(System)sounds\drums\Ride.wav" );
                    this.登録する( チップ種別.Ride_Cup, 0, @"$(System)sounds\drums\RideCup.wav" );
                    this.登録する( チップ種別.China, 0, @"$(System)sounds\drums\China.wav" );
                    this.登録する( チップ種別.Splash, 0, @"$(System)sounds\drums\Splash.wav" );
                    this.登録する( チップ種別.HiHat_Open, 0, @"$(System)sounds\drums\HiHatOpen.wav" );
                    this.登録する( チップ種別.HiHat_HalfOpen, 0, @"$(System)sounds\drums\HiHatHalfOpen.wav" );
                    this.登録する( チップ種別.HiHat_Close, 0, @"$(System)sounds\drums\HiHatClose.wav" );
                    this.登録する( チップ種別.HiHat_Foot, 0, @"$(System)sounds\drums\HiHatFoot.wav" );
                    this.登録する( チップ種別.Snare, 0, @"$(System)sounds\drums\Snare.wav" );
                    this.登録する( チップ種別.Snare_OpenRim, 0, @"$(System)sounds\drums\SnareOpenRim.wav" );
                    this.登録する( チップ種別.Snare_ClosedRim, 0, @"$(System)sounds\drums\SnareClosedRim.wav" );
                    this.登録する( チップ種別.Snare_Ghost, 0, @"$(System)sounds\drums\SnareGhost.wav" );
                    this.登録する( チップ種別.Bass, 0, @"$(System)sounds\drums\Bass.wav" );
                    this.登録する( チップ種別.Tom1, 0, @"$(System)sounds\drums\Tom1.wav" );
                    this.登録する( チップ種別.Tom1_Rim, 0, @"$(System)sounds\drums\Tom1Rim.wav" );
                    this.登録する( チップ種別.Tom2, 0, @"$(System)sounds\drums\Tom2.wav" );
                    this.登録する( チップ種別.Tom2_Rim, 0, @"$(System)sounds\drums\Tom2Rim.wav" );
                    this.登録する( チップ種別.Tom3, 0, @"$(System)sounds\drums\Tom3.wav" );
                    this.登録する( チップ種別.Tom3_Rim, 0, @"$(System)sounds\drums\Tom3Rim.wav" );
                    this.登録する( チップ種別.RightCrash, 0, @"$(System)sounds\drums\RightCrash.wav" );
                    this.登録する( チップ種別.LeftCymbal_Mute, 0, @"$(System)sounds\drums\LeftCymbalMute.wav" );
                    this.登録する( チップ種別.RightCymbal_Mute, 0, @"$(System)sounds\drums\RightCymbalMute.wav" );
                }
            }
        }
        public void 登録する( チップ種別 chipType, int subChipId, VariablePath サウンドファイルパス )
        {
            if( File.Exists( サウンドファイルパス.変数なしパス ) )
            {
                lock( this._Sound利用権 )
                {
                    // すでに辞書に存在してるなら、解放して削除する。
                    if( this._チップtoコンテキスト.ContainsKey( (chipType, subChipId) ) )
                    {
                        this._チップtoコンテキスト[ (chipType, subChipId) ]?.Dispose();
                        this._チップtoコンテキスト.Remove( (chipType, subChipId) );
                    }

                    // コンテキストを作成する。
                    var context = new SoundContext( this._多重度 );

                    // サウンドファイルを読み込んでデコードする。
                    context.SampleSource = SampleSourceFactory.Create( App.サウンドデバイス, サウンドファイルパス );

                    // 多重度分のサウンドを生成する。
                    for( int i = 0; i < context.Sounds.Length; i++ )
                        context.Sounds[ i ] = new Sound( App.サウンドデバイス, context.SampleSource );

                    // コンテキストを辞書に追加する。
                    this._チップtoコンテキスト.Add( (chipType, subChipId), context );

                    Log.Info( $"ドラムサウンドを生成しました。[({chipType.ToString()},{subChipId}) = {サウンドファイルパス.変数付きパス}]" );
                }
            }
            else
            {
                Log.ERROR( $"サウンドファイルが存在しません。[{サウンドファイルパス.変数付きパス}]" );
            }
        }
        public void 発声する( チップ種別 chipType, int subChipId, bool 発声前に消音する, 消音グループ種別 muteGroupType, float 音量0to1 = 1f )
        {
            lock( this._Sound利用権 )
            {
                if( this._チップtoコンテキスト.TryGetValue( (chipType, subChipId), out SoundContext context ) )
                {
                    // 必要あれば消音する。
                    if( 発声前に消音する && muteGroupType != 消音グループ種別.Unknown )
                    {
                        // 指定された消音グループ種別に属する Sound をすべて停止する。
                        var 停止するSoundContexts = this._チップtoコンテキスト.Where( ( kvp ) => ( kvp.Value.最後に発声したときの消音グループ種別 == muteGroupType ) );

                        foreach( var wavContext in 停止するSoundContexts )
                            foreach( var sound in wavContext.Value.Sounds )
                                sound.Stop();
                    }

                    // 発声する。
                    context.発声する( muteGroupType, 音量0to1 );
                }
                else
                {
                    // コンテキストがないなら何もしない。
                }
            }
        }

        private class SoundContext : IDisposable
        {
            public ISampleSource SampleSource = null;
            public Sound[] Sounds = null;
            public 消音グループ種別 最後に発声したときの消音グループ種別
            {
                get;
                protected set;
            } = 消音グループ種別.Unknown;

            public SoundContext( int 多重度 = 4 )
            {
                this._多重度 = 多重度;
                this.Sounds = new Sound[ this._多重度 ];
            }
            public void Dispose()
            {
                for( int i = 0; i < this.Sounds.Length; i++ )
                {
                    if( this.Sounds[ i ].再生中である )
                        this.Sounds[ i ].Stop();

                    this.Sounds[i]?.Dispose();
                    this.Sounds[i] = null;
                }

                this.SampleSource?.Dispose();
                this.SampleSource = null;
            }
            public void 発声する( 消音グループ種別 muteGroupType, float 音量 )
            {
                this.最後に発声したときの消音グループ種別 = muteGroupType;

                // 発声。
                if( null != this.Sounds[ this._次に再生するSound番号 ] )
                {
                    音量 = 
                        ( 0f > 音量 ) ? 0f :
                        ( 1f < 音量 ) ? 1f : 音量;
                    this.Sounds[ this._次に再生するSound番号 ].Volume = 音量;
                    this.Sounds[ this._次に再生するSound番号 ].Play();
                }

                // サウンドローテーション。
                this._次に再生するSound番号 = ( this._次に再生するSound番号 + 1 ) % this._多重度;
            }

            private readonly int _多重度 = 4;
            private int _次に再生するSound番号 = 0;
        };

        private readonly int _多重度 = 4;
        private Dictionary<(チップ種別 chipType, int サブチップID), SoundContext> _チップtoコンテキスト = null;
        private readonly object _Sound利用権 = new object();
    }
}
