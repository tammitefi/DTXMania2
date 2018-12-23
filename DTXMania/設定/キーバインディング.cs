using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using SharpDX.DirectInput;
using FDK;
using DTXMania.入力;

namespace DTXMania.設定
{
    [DataContract( Name = "KeyBindings", Namespace = "" )]
    class キーバインディング : IExtensibleDataObject, ICloneable
    {
        /// <summary>
        ///		入力コードのマッピング用 Dictionary のキーとなる型。
        ///		入力は、デバイスID（入力デバイスの内部識別用ID; FDKのIInputEvent.DeviceIDと同じ）と、
        ///		キー（キーコード、ノート番号などデバイスから得られる入力値）の組で定義される。
        /// </summary>
        [DataContract( Name = "IDとキー", Namespace = "" )]
        [TypeConverter( typeof( IdKeyConverter ) )]
        public struct IdKey
        {
            [DataMember]
            public int deviceId;

            [DataMember]
            public int key;

            public IdKey( int deviceId, int key )
            {
                this.deviceId = deviceId;
                this.key = key;
            }
            public IdKey( InputEvent ie )
            {
                this.deviceId = ie.DeviceID;
                this.key = ie.Key;
            }
            public IdKey( string 文字列 )
            {
                // 変なの食わせたらそのまま例外発出する。
                string[] v = 文字列.Split( new char[] { ',' } );

                this.deviceId = int.Parse( v[ 0 ] );
                this.key = int.Parse( v[ 1 ] );
            }
            public override string ToString()
                => $"{this.deviceId},{this.key}";

            /// <summary>
            ///     IdKey と string との相互変換。シリアライズ時に必要。
            /// </summary>
            public class IdKeyConverter : TypeConverter
            {
                public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType )
                    => ( sourceType == typeof( string ) ) ? true : base.CanConvertFrom( context, sourceType );
                public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object value )
                    => ( value is string strvalue ) ? new IdKey( strvalue ) : base.ConvertFrom( context, culture, value );
                public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType )
                    => ( destinationType == typeof( string ) ) ? true : base.CanConvertTo( context, destinationType );
                public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType )
                    => ( destinationType == typeof( string ) && ( value is IdKey idkey ) ) ? idkey.ToString() : base.ConvertTo( context, culture, value, destinationType );
            }
        }


        /// <summary>
        ///		MIDI番号(0～7)とMIDIデバイス名のマッピング用 Dictionary。
        /// </summary>
        [DataMember]
        public Dictionary<int, string> MIDIデバイス番号toデバイス名 { get; protected set; }

        /// <summary>
        ///		キーボードの入力（DirectInputのKey値）からドラム入力へのマッピング用 Dictionary 。
        /// </summary>
        [DataMember]
        public Dictionary<IdKey, ドラム入力種別> キーボードtoドラム { get; protected set; }

        /// <summary>
        ///		MIDI入力の入力（MIDIノート番号）からドラム入力へのマッピング用 Dictionary 。
        /// </summary>
        [DataMember]
        public Dictionary<IdKey, ドラム入力種別> MIDItoドラム { get; protected set; }

        [DataMember]
        public int FootPedal最小値 { get; set; }

        [DataMember]
        public int FootPedal最大値 { get; set; }


        /// <summary>
        ///		コンストラクタ。
        /// </summary>
        public キーバインディング()
        {
            this.OnDeserializing( new StreamingContext() );
        }

        /// <summary>
        ///     メンバを共有しない深いコピーを返す。
        /// </summary>
        public object Clone()
        {
            var clone = new キーバインディング();


            clone.MIDIデバイス番号toデバイス名 = new Dictionary<int, string>();

            foreach( var kvp in this.MIDIデバイス番号toデバイス名 )
                clone.MIDIデバイス番号toデバイス名.Add( kvp.Key, kvp.Value );


            clone.キーボードtoドラム = new Dictionary<IdKey, ドラム入力種別>();

            foreach( var kvp in this.キーボードtoドラム )
                clone.キーボードtoドラム.Add( kvp.Key, kvp.Value );


            clone.MIDItoドラム = new Dictionary<IdKey, ドラム入力種別>();

            foreach( var kvp in this.MIDItoドラム )
                clone.MIDItoドラム.Add( kvp.Key, kvp.Value );

            clone.FootPedal最小値 = this.FootPedal最小値;
            clone.FootPedal最大値 = this.FootPedal最大値;

            return clone;
        }


        /// <summary>
        ///		コンストラクタまたは逆シリアル化前（復元前）に呼び出される。
        ///		ここでは主に、メンバを規定値で初期化する。
        /// </summary>
        /// <param name="sc">未使用。</param>
        [OnDeserializing]
        private void OnDeserializing( StreamingContext sc )
        {
            this.FootPedal最小値 = 0;
            this.FootPedal最大値 = 90; // VH-11 の Normal Resolution での最大値

            this.MIDIデバイス番号toデバイス名 = new Dictionary<int, string>();

            this.キーボードtoドラム = new Dictionary<IdKey, ドラム入力種別>() {
                { new IdKey( 0, (int) Key.Q ),      ドラム入力種別.LeftCrash },
                { new IdKey( 0, (int) Key.Return ), ドラム入力種別.LeftCrash },
                { new IdKey( 0, (int) Key.A ),      ドラム入力種別.HiHat_Open },
                { new IdKey( 0, (int) Key.Z ),      ドラム入力種別.HiHat_Close },
                { new IdKey( 0, (int) Key.S ),      ドラム入力種別.HiHat_Foot },
                { new IdKey( 0, (int) Key.X ),      ドラム入力種別.Snare },
                { new IdKey( 0, (int) Key.C ),      ドラム入力種別.Bass },
                { new IdKey( 0, (int) Key.Space ),  ドラム入力種別.Bass },
                { new IdKey( 0, (int) Key.V ),      ドラム入力種別.Tom1 },
                { new IdKey( 0, (int) Key.B ),      ドラム入力種別.Tom2 },
                { new IdKey( 0, (int) Key.N ),      ドラム入力種別.Tom3 },
                { new IdKey( 0, (int) Key.M ),      ドラム入力種別.RightCrash },
                { new IdKey( 0, (int) Key.K ),      ドラム入力種別.Ride },
            };

            this.MIDItoドラム = new Dictionary<IdKey, ドラム入力種別>() {
				// うちの環境(2017.6.11)
				{ new IdKey( 0,  36 ), ドラム入力種別.Bass },
                { new IdKey( 0,  30 ), ドラム入力種別.RightCrash },
                { new IdKey( 0,  29 ), ドラム入力種別.RightCrash },
                { new IdKey( 1,  51 ), ドラム入力種別.RightCrash },
                { new IdKey( 1,  52 ), ドラム入力種別.RightCrash },
                { new IdKey( 1,  57 ), ドラム入力種別.RightCrash },
                { new IdKey( 0,  52 ), ドラム入力種別.RightCrash },
                { new IdKey( 0,  43 ), ドラム入力種別.Tom3 },
                { new IdKey( 0,  58 ), ドラム入力種別.Tom3 },
                { new IdKey( 0,  42 ), ドラム入力種別.HiHat_Close },
                { new IdKey( 0,  22 ), ドラム入力種別.HiHat_Close },
                { new IdKey( 0,  26 ), ドラム入力種別.HiHat_Open },
                { new IdKey( 0,  46 ), ドラム入力種別.HiHat_Open },
                { new IdKey( 0,  44 ), ドラム入力種別.HiHat_Foot },
                { new IdKey( 0, 255 ), ドラム入力種別.HiHat_Control },	// FDK の MidiIn クラスは、FootControl を ノート 255 として扱う。
				{ new IdKey( 0,  48 ), ドラム入力種別.Tom1 },
                { new IdKey( 0,  50 ), ドラム入力種別.Tom1 },
                { new IdKey( 0,  49 ), ドラム入力種別.LeftCrash },
                { new IdKey( 0,  55 ), ドラム入力種別.LeftCrash },
                { new IdKey( 1,  48 ), ドラム入力種別.LeftCrash },
                { new IdKey( 1,  49 ), ドラム入力種別.LeftCrash },
                { new IdKey( 1,  59 ), ドラム入力種別.LeftCrash },
                { new IdKey( 0,  45 ), ドラム入力種別.Tom2 },
                { new IdKey( 0,  47 ), ドラム入力種別.Tom2 },
                { new IdKey( 0,  51 ), ドラム入力種別.Ride },
                { new IdKey( 0,  59 ), ドラム入力種別.Ride },
                { new IdKey( 0,  38 ), ドラム入力種別.Snare },
                { new IdKey( 0,  40 ), ドラム入力種別.Snare },
                { new IdKey( 0,  37 ), ドラム入力種別.Snare },
			};
        }

        /// <summary>
        ///		逆シリアル化後（復元後）に呼び出される。
        ///		DataMember を使って他の 非DataMember を初期化する、などの処理を行う。
        /// </summary>
        /// <param name="sc">未使用。</param>
        [OnDeserialized]
        private void OnDeserialized( StreamingContext sc )
        {
        }

        /// <summary>
        ///		シリアル化前に呼び出される。
        ///		非DataMember を使って保存用の DataMember を初期化する、などの処理を行う。
        /// </summary>
        /// <param name="sc">未使用。</param>
        [OnSerializing]
        private void OnSerializing( StreamingContext sc )
        {
        }

        /// <summary>
        ///		シリアル化後に呼び出される。
        ///		ログの出力などの処理を行う。
        /// </summary>
        /// <param name="sc">未使用。</param>
        [OnSerialized]
        private void OnSerialized( StreamingContext sc )
        {
        }

        #region " IExtensibleDataObject の実装 "
        //----------------
        private ExtensionDataObject _ExData;

        public virtual ExtensionDataObject ExtensionData
        {
            get
                => this._ExData;

            set
                => this._ExData = value;
        }
        //----------------
        #endregion
    }
}
