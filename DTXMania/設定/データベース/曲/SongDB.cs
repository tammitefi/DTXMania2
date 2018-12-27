using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FDK;
using SSTFormatCurrent = SSTFormat.v4;

namespace DTXMania.データベース.曲
{
    using Song01 = old.Song01;
    using Song02 = old.Song02;
    using Song03 = old.Song03;
    using Song = Song04;    // 最新バージョンを指定(1/2)。

    /// <summary>
    ///		曲データベースに対応するエンティティクラス。
    /// </summary>
    class SongDB : SQLiteDBBase
    {
        public const long VERSION = 4;  // 最新バージョンを指定(2/2)。

        public static readonly VariablePath 曲DBファイルパス = @"$(AppData)SongDB.sqlite3";

        public Table<Song> Songs
            => base.DataContext.GetTable<Song>();


        public SongDB()
            : base( 曲DBファイルパス, VERSION )
        {
        }


        protected override void テーブルがなければ作成する()
        {
            using( var transaction = this.Connection.BeginTransaction() )
            {
                try
                {
                    // 最新のバージョンのテーブルを作成する。
                    this.DataContext.ExecuteCommand( $"CREATE TABLE IF NOT EXISTS Songs {Song.ColumnsList};" );
                    this.DataContext.SubmitChanges();

                    // 成功。
                    transaction.Commit();
                }
                catch
                {
                    // 失敗。
                    transaction.Rollback();
                }
            }
        }

        protected override void データベースのアップグレードマイグレーションを行う( long 移行元DBバージョン )
        {
            switch( 移行元DBバージョン )
            {
                case 1:
                    #region " 1 → 2 "
                    //----------------
                    this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = OFF" );
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // テータベースをアップデートしてデータを移行する。
                            this.DataContext.ExecuteCommand( $"CREATE TABLE new_Songs {Song02.ColumnsList}" );
                            this.DataContext.ExecuteCommand( $"INSERT INTO new_Songs SELECT *,null FROM Songs" );   // 追加されたカラムは null
                            this.DataContext.ExecuteCommand( $"DROP TABLE Songs" );
                            this.DataContext.ExecuteCommand( $"ALTER TABLE new_Songs RENAME TO Songs" );
                            this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = ON" );
                            this.DataContext.SubmitChanges();

                            // すべてのレコードについて、追加されたカラムを更新する。
                            var song02s = base.DataContext.GetTable<Song02>();
                            foreach( var song02 in song02s )
                            {
                                var score = (SSTFormatCurrent.スコア) null;
                                var vpath = new VariablePath( song02.Path );

                                // スコアを読み込む
                                score = SSTFormatCurrent.スコア.ファイルから生成する( vpath.変数なしパス );

                                // PreImage カラムの更新。
                                if( score.プレビュー画像ファイル名.Nullでも空でもない() )
                                {
                                    // プレビュー画像は、曲ファイルからの相対パス。
                                    song02.PreImage = Path.Combine( Path.GetDirectoryName( vpath.変数なしパス ), score.プレビュー画像ファイル名 );
                                }
                                else
                                {
                                    song02.PreImage =
                                        ( from ファイル名 in Directory.GetFiles( Path.GetDirectoryName( vpath.変数なしパス ) )
                                          where _対応するサムネイル画像名.Any( thumbファイル名 => ( Path.GetFileName( ファイル名 ).ToLower() == thumbファイル名 ) )
                                          select ファイル名 ).FirstOrDefault();
                                }
                            }
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            Log.Info( "Songs テーブルをアップデートしました。[1→2]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            Log.ERROR( "Songs テーブルのアップデートに失敗しました。[1→2]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 2:
                    #region " 2 → 3 "
                    //----------------
                    this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = OFF" );
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // テータベースをアップデートしてデータを移行する。
                            this.DataContext.ExecuteCommand( $"CREATE TABLE new_Songs {Song03.ColumnsList}" );
                            this.DataContext.ExecuteCommand( $"INSERT INTO new_Songs SELECT *,null FROM Songs" );   // 追加されたカラムは null
                            this.DataContext.ExecuteCommand( $"DROP TABLE Songs" );
                            this.DataContext.ExecuteCommand( $"ALTER TABLE new_Songs RENAME TO Songs" );
                            this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = ON" );
                            this.DataContext.SubmitChanges();

                            // すべてのレコードについて、追加されたカラムを更新する。
                            var song03s = base.DataContext.GetTable<Song03>();
                            foreach( var song03 in song03s )
                            {
                                var score = (SSTFormatCurrent.スコア) null;
                                var vpath = new VariablePath( song03.Path );

                                // スコアを読み込む 
                                score = SSTFormatCurrent.スコア.ファイルから生成する( vpath.変数なしパス );

                                // Artist カラムの更新。
                                if( score.アーティスト名.Nullでも空でもない() )
                                {
                                    song03.Artist = score.アーティスト名;
                                }
                                else
                                {
                                    song03.Artist = ""; // null不可
                                }
                            }
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            Log.Info( "Songs テーブルをアップデートしました。[2→3]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            Log.ERROR( "Songs テーブルのアップデートに失敗しました。[2→3]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 3:
                    #region " 3 → 4 "
                    //----------------
                    // 変更点：
                    // ・Preimage フィールドの内容が、絶対パスから、曲譜面ファイルからの相対パスに変更。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // すべてのレコードについて、変更のあったカラムを更新する。
                            var song03s = this.DataContext.GetTable<Song03>();
                            foreach( var song03 in song03s )
                            {
                                // Preimage を絶対パスから相対パスに変換。
                                if( song03.PreImage.Nullでも空でもない() && Path.IsPathRooted( song03.PreImage ) )
                                    song03.PreImage = FDK.Folder.絶対パスを相対パスに変換する( Path.GetDirectoryName( song03.Path ), song03.PreImage );
                            }
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            Log.Info( $"Songs テーブルをアップデートしました。[{移行元DBバージョン}→{移行元DBバージョン+1}]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            Log.ERROR( $"Songs テーブルのアップデートに失敗しました。[{移行元DBバージョン}→{移行元DBバージョン+1}]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                default:
                    throw new Exception( $"移行元DBのバージョン({移行元DBバージョン})がマイグレーションに未対応です。" );
            }
        }


        private readonly string[] _対応するサムネイル画像名 = { "thumb.png", "thumb.bmp", "thumb.jpg", "thumb.jpeg" };
    }
}
