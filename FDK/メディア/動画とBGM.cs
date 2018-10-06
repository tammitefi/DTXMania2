using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSCore;
using FDK.メディア.サウンド;
using FDK.メディア.ビデオ;
using FDK.メディア.ストリーミング;

namespace FDK.メディア
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
		public void 再生を開始する()
		{
			this.Video?.再生を開始する();
			this.Audio?.Play();
		}
		public void Dispose()
		{
			this.Audio?.Dispose();
			this.Audio = null;

			this.Video?.非活性化する();
			this.Video = null;

			this._sources?.Dispose();
			this._sources = null;
		}
		public void ビデオをキャンセルする()
		{
			this._sources?.ビデオをキャンセルする();
		}

		private MediaFoundationStreamingSources _sources = null;
	}
}
