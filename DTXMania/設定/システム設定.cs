﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
    class システム設定
    {
        public static readonly VariablePath システム設定ファイルパス = @"$(AppData)Configuration.yaml";

        /// <summary>
        ///     このクラスのバージョン。
        /// </summary>
        public int Version = 1;

        /// <remarks>
        ///		キーバインディングは全ユーザで共通。
        /// </remarks>
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
        public int 入力発声スレッドのスリープ量ms { get; set; }

        public Point ウィンドウ表示位置Viewerモード用 { get; set; }

        public Size ウィンドウサイズViewerモード用 { get; set; }

        /// <summary>
        ///     チップヒットの判定位置を、判定バーからさらに上下に調整する。
        ///     -99～+99[ms] 。
        /// </summary>
        /// <remarks>
        ///     判定バーの見かけの位置は変わらず、判定位置のみ移動する。
        ///     入力機器の遅延対策であるが、入力機器とのヒモづけは行わないので、
        ///     入力機器が変われば再調整が必要になる場合がある。
        /// </remarks>
        public int 判定位置調整ms { get; set; }


        public システム設定()
        {
            this.キーバインディング = new キーバインディング();
            this.曲検索フォルダ = new List<VariablePath>() { @"$(Exe)" };
            this.入力発声スレッドのスリープ量ms = 2;
            this.ウィンドウ表示位置Viewerモード用 = new Point( 100, 100 );
            this.ウィンドウサイズViewerモード用 = new Size( 640, 360 );
            this.判定位置調整ms = 0;
        }

        public static システム設定 復元する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // (1) 読み込み or 新規作成

                var config = (システム設定) null;

                try
                {
                    var yaml = File.ReadAllText( システム設定ファイルパス.変数なしパス );
                    var deserializer = new YamlDotNet.Serialization.Deserializer();
                    config = deserializer.Deserialize<システム設定>( yaml );

                    if( 1 != config.Version )
                    {
                        Log.ERROR( $"未対応のバージョンです。新規に作成して保存します。[{システム設定ファイルパス.変数付きパス}]" );
                        config = new システム設定();
                        config.保存する();
                    }
                }
                catch( FileNotFoundException )
                {
                    Log.Info( $"ファイルが存在しないため、新規に作成します。[{システム設定ファイルパス.変数付きパス}]" );
                    config = new システム設定();
                    config.保存する();
                }
                catch
                {
                    Log.ERROR( $"ファイルの内容に誤りがあります。新規に作成して保存します。[{システム設定ファイルパス.変数付きパス}]" );
                    config = new システム設定();
                    config.保存する();
                }


                // (2) 読み込み後の処理

                // パスの指定がなければ、とりあえず exe のあるフォルダを検索対象にする。
                if( 0 == config.曲検索フォルダ.Count )
                    config.曲検索フォルダ.Add( @"$(Exe)" );

                // 値の範囲チェック。
                config.入力発声スレッドのスリープ量ms = Math.Max( 1, Math.Min( 10, config.入力発声スレッドのスリープ量ms ) );

                return config;
            }
        }

        public void 保存する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var serializer = new YamlDotNet.Serialization.SerializerBuilder()
                    .WithTypeInspector( inner => new FDK.シリアライズ.YAML.CommentGatheringTypeInspector( inner ) )
                    .WithEmissionPhaseObjectGraphVisitor( args => new FDK.シリアライズ.YAML.CommentsObjectGraphVisitor( args.InnerVisitor ) )
                    .Build();

                // ※ 値が既定値であるプロパティは出力されないので注意。
                var yaml = serializer.Serialize( this );
                File.WriteAllText( システム設定ファイルパス.変数なしパス, yaml );

                Log.Info( $"システム設定 を保存しました。[{システム設定ファイルパス.変数付きパス}]" );
            }
        }
    }
}
