using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSCore;

namespace FDK
{
	public class 動画とBGM : IDisposable
	{
        /// <remarks>
        ///     Video ストリームがなければ null。
        /// </remarks>
		public Video Video { get; protected set; } = null;
        
        /// <remarks>
        ///     Audio ストリームがなければ null。
        /// </remarks>
		public Sound Audio { get; protected set; } = null;

        public bool 再生中 { get; protected set; } = false;


		public 動画とBGM( VariablePath ファイルパス, SoundDevice soundDevice )
		{
			this._sources = MediaFoundationStreamingSources.CreateFromFile( ファイルパス, soundDevice.WaveFormat );

            this.Video = ( null != this._sources.VideoSource ) ? new Video( this._sources.VideoSource ) : null;
            this.Audio = ( null != this._sources.WaveSource ) ? new Sound( soundDevice, this._sources.WaveSource.ToSampleSource() ) : null;
		}

        public 動画とBGM( string user_id, string password, string video_id, SoundDevice soundDevice )
		{
			this._sources = MediaFoundationStreamingSources.CreateFromニコ動( user_id, password, video_id, soundDevice.WaveFormat );

            this.Video = ( null != this._sources.VideoSource ) ? new Video( this._sources.VideoSource ) : null;
            this.Audio = ( null != this._sources.WaveSource ) ? new Sound( soundDevice, this._sources.WaveSource.ToSampleSource() ) : null;
		}

        public void Dispose()
        {
            this.再生を終了する();

            this.Audio?.Dispose();
            this.Audio = null;

            this.Video?.非活性化する();
            this.Video = null;

            this._sources?.Dispose();
            this._sources = null;
        }

        public void 再生を開始する()
		{
			this.Video?.再生を開始する();
			this.Audio?.Play();

            this.再生中 = true;
		}

        public void 再生を終了する()
        {
            this.Video?.再生を終了する();
            this.Audio?.Stop();

            this.再生中 = false;
        }

        public void ビデオをキャンセルする()
		{
			this._sources?.ビデオをキャンセルする();
		}


		protected MediaFoundationStreamingSources _sources = null;
	}
}
