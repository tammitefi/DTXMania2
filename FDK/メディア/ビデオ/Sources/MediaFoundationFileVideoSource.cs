using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.MediaFoundation;
using FDK.メディア.ストリーミング;

namespace FDK.メディア.ビデオ.Sources
{
    public class MediaFoundationFileVideoSource : IVideoSource
    {
        public Size2F フレームサイズ { get; protected set; }
        public bool ループ再生する { get; set; } = false;

        public MediaFoundationFileVideoSource( VariablePath ファイルパス )
        {
            #region " ファイルから SourceReaderEx を生成する。"
            //----------------
            using( var ビデオ属性 = new MediaAttributes() )
            {
                // DXVAに対応しているGPUの場合には、それをデコードに利用するよう指定する。
                ビデオ属性.Set( SourceReaderAttributeKeys.D3DManager, グラフィックデバイス.Instance.DXGIDeviceManager );

                // 追加のビデオプロセッシングを有効にする。
                ビデオ属性.Set( SourceReaderAttributeKeys.EnableAdvancedVideoProcessing, true );  // 真偽値が bool だったり

                // 追加のビデオプロセッシングを有効にしたら、こちらは無効に。
                ビデオ属性.Set( SinkWriterAttributeKeys.ReadwriteDisableConverters, 0 );           // int だったり

                // 属性を使って、SourceReaderEx を生成。
                using( var sourceReader = new SourceReader( ファイルパス.変数なしパス, ビデオ属性 ) )    // パスは URI 扱い
                    this._SourceReaderEx = sourceReader.QueryInterface<SourceReaderEx>();
            }
            //----------------
            #endregion

            this._SourceReaderEx.SetStreamSelection( SourceReaderIndex.AllStreams, false );
            this._長さsec = FDKUtilities.変換_100ns単位からsec単位へ( this._SourceReaderEx.GetPresentationAttribute( SourceReaderIndex.MediaSource, PresentationDescriptionAttributeKeys.Duration ) );

            #region " Video の初期化 "
            //----------------
            this._SourceReaderEx.SetStreamSelection( SourceReaderIndex.FirstVideoStream, true );

            // 部分メディアタイプを設定する。
            using( var videoMediaType = new MediaType() )
            {
                // フォーマットは ARGB32 で固定とする。（SourceReaderEx を使わない場合、H264 では ARGB32 が選べないので注意。）
                videoMediaType.Set( MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video );
                videoMediaType.Set( MediaTypeAttributeKeys.Subtype, VideoFormatGuids.Argb32 );

                // 部分メディアタイプを SourceReaderEx にセットする。SourceReaderEx は、必要なデコーダをロードするだろう。
                this._SourceReaderEx.SetCurrentMediaType( SourceReaderIndex.FirstVideoStream, videoMediaType );
            }

            // 完成されたメディアタイプを取得する。
            this._VideoのMediaType = this._SourceReaderEx.GetCurrentMediaType( SourceReaderIndex.FirstVideoStream );

            // ビデオのフレームサイズを取得する。
            long packedFrameSize = this._VideoのMediaType.Get( MediaTypeAttributeKeys.FrameSize ); // 動画の途中でのサイズ変更には対応しない。
            this.フレームサイズ = new Size2F( ( packedFrameSize >> 32 ) & 0xFFFFFFFF, ( packedFrameSize ) & 0xFFFFFFFF );
            //----------------
            #endregion

            this._VideoSource = new StreamingVideoSource( this.フレームサイズ );

            #region " デコード開始。"
            //----------------
            this._デコードキャンセル = new CancellationTokenSource();
            this._デコード起動完了通知 = new ManualResetEvent( false );
            this._デコードタスク = Task.Factory.StartNew( this._デコードタスクエントリ, this._デコードキャンセル.Token );
            this._デコード起動完了通知.WaitOne();
            //----------------
            #endregion

            Thread.Sleep( 500 );    // デコードデータが蓄積されるまで待機（手抜き
        }
        public void Dispose()
        {
            #region " デコードタスクが稼働してたら停止する。"
            //----------------
            if( ( null != this._デコードタスク ) && !( this._デコードタスク.IsCompleted ) )
            {
                this._デコードキャンセル.Cancel();
                this._VideoSource?.Cancel();

                if( !( this._デコードタスク.Wait( 5000 ) ) )
                    Log.ERROR( $"デコードタスクの終了がタイムアウトしました。" );

                this._デコードタスク = null;
            }
            //----------------
            #endregion

            this._デコードキャンセル?.Dispose();
            this._デコードキャンセル = null;

            this._VideoSource?.Dispose();
            this._VideoSource = null;

            this._VideoのMediaType?.Dispose();
            this._VideoのMediaType = null;

            this._SourceReaderEx?.Dispose();
            this._SourceReaderEx = null;
        }
        /// <summary>
        ///     次に読みだされるフレームがあれば、その表示予定時刻[100ns単位]を返す。
        ///     フレームがなければ、ブロックせずにすぐ 負数 を返す。
        /// </summary>
        public long Peek()
            => this._VideoSource.Peek();
        /// <summary>
        ///     フレームを１つ読みだす。
        ///     再生中の場合、フレームが取得できるまでブロックする。
        ///     再生が終了している場合は null を返す。
        ///     取得したフレームは、使用が終わったら、呼び出し元で Dispose すること。
        /// </summary>
        public VideoFrame Read()
            => this._VideoSource.Read();

        private SourceReaderEx _SourceReaderEx = null;
        private double _長さsec = 0.0;
        private MediaType _VideoのMediaType = null;
        private StreamingVideoSource _VideoSource = null;

        private Task _デコードタスク = null;
        private CancellationTokenSource _デコードキャンセル = null;
        private ManualResetEvent _デコード起動完了通知 = null;

        private void _デコードタスクエントリ()
        {
            Log.現在のスレッドに名前をつける( "ビデオデコード" );

            this._デコード起動完了通知.Set();

            while( true )
            {
                if( this._デコードキャンセル.Token.IsCancellationRequested )
                {
                    Log.Info( $"キャンセル通知を受信しました。" );
                    break;
                }

                // 次のサンプルをひとつデコードする。
                var サンプル = this._SourceReaderEx.ReadSample(
                    SourceReaderIndex.AnyStream,
                    SourceReaderControlFlags.None,
                    out int ストリーム番号,
                    out var ストリームフラグ,
                    out long サンプルの表示時刻100ns );

                // デコード結果を確認する。
                if( ストリームフラグ.HasFlag( SourceReaderFlags.Endofstream ) )
                {
                    Log.Info( $"ストリームが終了しました。" );
                    if( this.ループ再生する )
                    {
                        Log.Info( $"ループ再生します。" );
                        this._SourceReaderEx.SetCurrentPosition( 0 );
                        continue;
                    }
                    else
                    {
                        this._VideoSource?.Cancel();
                        break;
                    }
                }
                if( ストリームフラグ.HasFlag( SourceReaderFlags.Error ) ||
                    ストリームフラグ.HasFlag( SourceReaderFlags.AllEffectsremoved ) ||
                    //ストリームフラグ.HasFlag( SourceReaderFlags.Currentmediatypechanged ) ||
                    ストリームフラグ.HasFlag( SourceReaderFlags.Nativemediatypechanged ) ||
                    ストリームフラグ.HasFlag( SourceReaderFlags.Newstream ) ||
                    ストリームフラグ.HasFlag( SourceReaderFlags.StreamTick ) )
                {
                    Log.ERROR( $"デコード中にエラーが発生または未対応の状態変化が発生しました。" );
                    break;
                }

                // ビデオフレームを生成してキューに格納。キューがいっぱいならブロックする。
                this._VideoSource?.Write( new VideoFrame() {
                    Sample = サンプル,
                    Bitmap = this._VideoSource.サンプルからビットマップを取得する( サンプル ),
                    表示時刻100ns = サンプルの表示時刻100ns,
                } );
            }

            Log.Info( $"タスクを終了します。" );
        }
    }
}
