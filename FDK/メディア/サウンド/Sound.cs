using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSCore;

namespace FDK
{
    public class Sound : ISampleSource
    {
        public bool 再生中である
            => this._DeviceRef.TryGetTarget( out SoundDevice device ) && device.Mixer.Contains( this );
        public bool 再生中ではない
            => !( this.再生中である );

        /// <summary>
        ///		シークが可能なら true。
        /// </summary>
        public bool CanSeek
            => this._BaseSampleSource?.CanSeek ?? false;

        /// <summary>
        ///		このサウンドのフォーマット。
        /// </summary>
        public WaveFormat WaveFormat
            => this._BaseSampleSource?.WaveFormat ?? null;

        /// <remarks>
        ///		１つの <see cref="SampleSource"/>を複数の<see cref="Sound"/>インスタンスで共有できるように、
        ///		このプロパティは<see cref="Sound"/>インスタンスごとに独立して管理する。
        /// </remarks>
        public long Position
        {
            get
                => this._Position;
            set
                => this._Position = Math.Min( Math.Max( value, 0 ), this.Length );
        }

        public long Length
            => this._BaseSampleSource?.Length ?? 0;

        /// <summary>
        ///		音量。0.0(無音)～1.0(原音)～...上限なし
        /// </summary>
        /// <remarks>
        ///		このクラスではなく、<see cref="Mixer"/>クラスから参照して使用する。
        /// </remarks>
        public float Volume
        {
            get
                => this._Volume;
            set
                => this._Volume = Math.Max( value, 0 );
        }


        public Sound( SoundDevice device, ISampleSource sampleSource )
            : this( device )
        {
            this._BaseSampleSource = sampleSource ?? throw new ArgumentNullException();
        }

        protected Sound( SoundDevice device )
        {
            if( null == device )
                throw new ArgumentNullException();

            this._DeviceRef = new WeakReference<SoundDevice>( device );
        }

        public void Dispose()
        {
            this.Stop();

            //this._BaseSampleSource?.Dispose();	Dispose は外部で。（SampleSource は複数の Sound で共有されている可能性があるため。）
            this._BaseSampleSource = null;

            this._DeviceRef = null;
        }

        public void Play( long 再生開始位置frame = 0 )
        {
            if( null == this._BaseSampleSource )
                throw new InvalidOperationException( "サンプルソースが null です。" );

            if( this._DeviceRef.TryGetTarget( out SoundDevice device ) )
            {
                // BaseSampleSource の位置を、再生開始位置へ移動。
                if( this._BaseSampleSource.CanSeek )
                {
                    this._Position = 再生開始位置frame * this.WaveFormat.Channels;
                    //this._BaseSampleSource.Position = this._Position;		--> ここではまだ設定しない。Read() で設定する。
                }
                else
                {
                    throw new Exception( $"このサンプルソースの再生位置を変更することができません。既定の位置から再生を開始します。" );
                }

                // ミキサーに追加（＝再生開始）。
                device.Mixer.AddSound( this );
            }
        }

        public void Play( double 再生開始位置sec )
            => this.Play( this.秒ToFrame( 再生開始位置sec ) );

        public int Read( float[] buffer, int offset, int count )
        {
            if( null == this._BaseSampleSource )
                return 0;

            if( this._BaseSampleSource.Length <= this._Position )   // 最後まで再生済み、または次のストリームデータが届いていない
            {
                Array.Clear( buffer, offset, count );   // 全部ゼロで埋めて返す。
                return count;
            }

            // １つの BaseSampleSource を複数の Sound で共有するために、Position は Sound ごとに管理している。
            this._BaseSampleSource.Position = this._Position;
            var readCount = this._BaseSampleSource.Read( buffer, offset, count );   // 読み込み。
            this._Position = this._BaseSampleSource.Position;

            return readCount;
        }

        public void Stop()
        {
            if( ( null != this._DeviceRef ) && this._DeviceRef.TryGetTarget( out SoundDevice device ) )
            {
                device.Mixer?.RemoveSound( this );
            }
        }

        internal long 秒ToFrame( double 時間sec )
        {
            if( null == this._BaseSampleSource )
                return 0;

            var wf = this._BaseSampleSource.WaveFormat;
            return (long) ( 時間sec * wf.SampleRate + 0.5 ); // +0.5 で四捨五入ができる
        }

        internal double FrameTo秒( long 時間frame )
        {
            if( null == this._BaseSampleSource )
                return 0;

            var wf = this._BaseSampleSource.WaveFormat;
            return (double) 時間frame / wf.SampleRate;
        }


        private WeakReference<SoundDevice> _DeviceRef = null;

        private ISampleSource _BaseSampleSource = null;

        private long _Position = 0;

        private float _Volume = 1.0f;
    }
}
