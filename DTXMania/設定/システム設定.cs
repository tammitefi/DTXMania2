using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FDK;

namespace DTXMania.設定
{
    /// <summary>
    ///		システム設定。
    ///		全ユーザで共有される項目。
    /// </summary>
    /// <remarks>
    ///		ユーザ別の項目は<see cref="ユーザ設定"/>で管理すること。
    /// </remarks>
    [DataContract( Name = "Configuration", Namespace = "" )]
    [KnownType( typeof( キーバインディング ) )]
    class システム設定 : IExtensibleDataObject
    {
        public static readonly VariablePath システム設定ファイルパス = @"$(AppData)Configuration.json";

        /// <summary>
        ///     このクラスのバージョン。
        /// </summary>
        [DataMember( Order = 1 )]
        public static int Version = 1;

        /// <remarks>
        ///		キーバインディングは全ユーザで共通。
        /// </remarks>
        [DataMember( Name = "KeyBindings", Order = 100 )]
        public キーバインディング キーバインディング { get; set; } = null;

        /// <summary>
        ///		曲ファイルを検索するフォルダのリスト。
        /// </summary>
        /// <remarks>
        ///		シリアライゼーションでは、これを直接使わずに、<see cref="_曲検索フォルダProxy"/> を仲介する。
        /// </remarks>
        public List<VariablePath> 曲検索フォルダ { get; protected set; } = null;

        /// <summary>
        ///     1～10?
        /// </summary>
        [DataMember( Name = "WorkThreadSleep", Order = 10 )]
        public int 入力発声スレッドのスリープ量ms { get; set; }

        [DataMember( Name = "WindowPositionOfViewerMode", Order = 10 )]
        public Point ウィンドウ表示位置Viewerモード用 { get; set; }

        [DataMember( Name = "ClientSizeOfViewerMode", Order = 10 )]
        public Size ウィンドウサイズViewerモード用 { get; set; }


        public システム設定()
        {
            this.OnDeserializing( new StreamingContext() );

            // パスの指定がなければ、とりあえず exe のあるフォルダを検索対象にする。
            if( 0 == this.曲検索フォルダ.Count )
                this.曲検索フォルダ.Add( @"$(Exe)" );
        }

        public static システム設定 復元する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 設定 = ( システム設定 ) null;

				try
				{
					// コンフィグのバージョン番号を取得。
					var jobj = JObject.Parse( File.ReadAllText( システム設定ファイルパス.変数なしパス ) ); // いったん型なしで読み込む
					int configVersion = ( (int?) jobj[ "Version" ] ) ?? 0;

					// バージョン別に読み込み。
					switch( configVersion )
					{
						case 1:
							// Release 015 以降
							設定 = JsonConvert.DeserializeObject<システム設定>( jobj.ToString() );
							Log.Info( $"システム設定をファイルから復元しました。[{システム設定ファイルパス.変数付きパス}]" );
							break;

						case 0:
							#region " Release 014 まで "
							//----------------
							try
							{
								設定 = FDKUtilities.復元する<システム設定>( システム設定ファイルパス, UseSimpleDictionaryFormat: false );
								Log.Info( $"システム設定をファイルから復元しました。[{システム設定ファイルパス.変数付きパス}]" );
							}
							catch
							{
								Log.WARNING( $"復元に失敗したので、新規に作成して保存します。[{システム設定ファイルパス.変数付きパス}]" );
								設定 = new システム設定();
								FDKUtilities.保存する( 設定, システム設定ファイルパス, UseSimpleDictionaryFormat: false );
							}
							return 設定;
						//----------------
						#endregion

						default:
							throw new NotSupportedException( $"未知のバージョン（{configVersion}）が指定されました。" );
					}

					return 設定;
				}
				catch( FileNotFoundException )
				{
					Log.Info( $"ファイルが存在しないため、新規に作成して保存します。[{システム設定ファイルパス.変数付きパス}]" );
					設定 = new システム設定();
					FDKUtilities.保存する( 設定, システム設定ファイルパス, UseSimpleDictionaryFormat: false );
					return 設定;
				}
				catch
				{
					Log.ERROR( $"ファイルの内容に誤りがあります。新規に作成して保存します。[{システム設定ファイルパス.変数付きパス}]" );
					設定 = new システム設定();
					FDKUtilities.保存する( 設定, システム設定ファイルパス, UseSimpleDictionaryFormat: false );
					return 設定;
				}
			}
        }

        public void 保存する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                switch( Version )
                {
                    case 0:
                        //FDKUtilities.保存する( this, _ファイルパス, UseSimpleDictionaryFormat: false );
                        //break;
                    case 1:
                        File.WriteAllText( システム設定ファイルパス.変数なしパス, JsonConvert.SerializeObject( this, Formatting.Indented ) );
                        Log.Info( $"システム設定 を保存しました。[{システム設定ファイルパス.変数付きパス}]" );
                        break;
                }
            }
        }


        /// <summary>
        ///		<see cref="曲検索フォルダ"/> のシリアライゼーションのための仲介役。
        /// </summary>
        [DataMember( Name = "SongPaths", Order = 10 )]
        private List<string> _曲検索フォルダProxy = null;


        // シリアライズ関連

        /// <summary>
        ///		コンストラクタまたは逆シリアル化前（復元前）に呼び出される。
        ///		ここでは主に、メンバを規定値で初期化する。
        /// </summary>
        /// <param name="sc">未使用。</param>
        [OnDeserializing]
        private void OnDeserializing( StreamingContext sc )
        {
            this.キーバインディング = new キーバインディング();
            this.曲検索フォルダ = new List<VariablePath>();
            this._曲検索フォルダProxy = new List<string>();
            this.入力発声スレッドのスリープ量ms = 2;
            this.ウィンドウ表示位置Viewerモード用 = new Point( 100, 100 );
            this.ウィンドウサイズViewerモード用 = new Size( 640, 360 );
        }

        /// <summary>
        ///		逆シリアル化後（復元後）に呼び出される。
        ///		DataMember を使って他の 非DataMember を初期化する、などの処理を行う。
        /// </summary>
        /// <param name="sc">未使用。</param>
        [OnDeserialized]
        private void OnDeserialized( StreamingContext sc )
        {
            // Proxy から曲検索フォルダを復元。
            foreach( var path in this._曲検索フォルダProxy )
                this.曲検索フォルダ.Add( path );

            // パスの指定がなければ、とりあえず exe のあるフォルダを検索対象にする。
            if( 0 == this.曲検索フォルダ.Count )
                this.曲検索フォルダ.Add( @"$(Exe)" );

            // 値の範囲チェック。
            if( ( this.入力発声スレッドのスリープ量ms < 1 ) || ( this.入力発声スレッドのスリープ量ms > 10 ) )
            {
                Log.WARNING( $"入力発声スレッドのスリープ量msの値（{this.入力発声スレッドのスリープ量ms}）が無効です。1～10の範囲の整数で指定してください。" );
                this.入力発声スレッドのスリープ量ms = Math.Max( 1, Math.Min( 10, this.入力発声スレッドのスリープ量ms ) );
            }
        }

        /// <summary>
        ///		シリアル化前に呼び出される。
        ///		非DataMember を使って保存用の DataMember を初期化する、などの処理を行う。
        /// </summary>
        /// <param name="sc">未使用。</param>
        [OnSerializing]
        private void OnSerializing( StreamingContext sc )
        {
            // 曲検索フォルダの内容をProxyへ転載。
            this._曲検索フォルダProxy = new List<string>();
            foreach( var varpath in this.曲検索フォルダ )
                this._曲検索フォルダProxy.Add( varpath.変数付きパス );
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
