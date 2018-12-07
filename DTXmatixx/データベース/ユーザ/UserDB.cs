using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXmatixx.データベース.ユーザ
{
    using User01 = old.User01;
    using User02 = old.User02;
    using User03 = old.User03;
    using User04 = old.User04;
    using User05 = old.User05;

    using User = User06;        // 最新バージョンを指定（その１）
    using Record = Record06;    //

    /// <summary>
    ///		ユーザデータベースに対応するエンティティクラス。
    /// </summary>
    class UserDB : SQLiteDBBase
    {
        public const long VERSION = 6;  // 最新バージョンを指定（その２）

        public Table<User> Users
            => base.DataContext.GetTable<User>();
        public Table<Record> Records
            => base.DataContext.GetTable<Record>();

        public UserDB()
            : base( @"$(AppData)UserDB.sqlite3", VERSION )
        {
        }


        protected override void テーブルがなければ作成する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                using( var transaction = this.Connection.BeginTransaction() )
                {
                    try
                    {
                        // 最新のバージョンのテーブルを作成する。
                        this.DataContext.ExecuteCommand( $"CREATE TABLE IF NOT EXISTS Users {User.ColumnsList};" );
                        this.DataContext.ExecuteCommand( $"CREATE TABLE IF NOT EXISTS Records {Record.ColumnList};" );
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
        }

        protected override void データベースのアップグレードマイグレーションを行う( long 移行元DBバージョン )
        {
            switch( 移行元DBバージョン )
            {
                case 1:
                    #region " 1 → 2 "
                    //----------------
                    // 変更点:
                    // ・Users テーブルから SongFolders カラムを削除。
                    this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = OFF" );
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // テータベースをアップデートしてデータを移行する。
                            this.DataContext.ExecuteCommand( $"CREATE TABLE new_Users {User02.ColumnsList}" );
                            this.DataContext.ExecuteCommand( "INSERT INTO new_Users SELECT Id,Name,ScrollSpeed,Fullscreen,AutoPlay_LeftCymbal,AutoPlay_HiHat,AutoPlay_LeftPedal,AutoPlay_Snare,AutoPlay_Bass,AutoPlay_HighTom,AutoPlay_LowTom,AutoPlay_FloorTom,AutoPlay_RightCymbal,MaxRange_Perfect,MaxRange_Great,MaxRange_Good,MaxRange_Ok,CymbalFree FROM Users" );
                            this.DataContext.ExecuteCommand( "DROP TABLE Users" );
                            this.DataContext.ExecuteCommand( "ALTER TABLE new_Users RENAME TO Users" );
                            this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = ON" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            Log.Info( "Users テーブルをアップデートしました。[1→2]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( "Users テーブルのアップデートに失敗しました。[1→2]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 2:
                    #region " 2 → 3 "
                    //----------------
                    // 変更点:
                    // ・Users テーブルに PlayMode カラムを追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // テータベースをアップデートしてデータを移行する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN PlayMode INTEGER NOT NULL DEFAULT 1" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            this.DataContext.ExecuteCommand( "VACUUM" );    // Vacuum はトランザクションの外で。
                            this.DataContext.SubmitChanges();
                            Log.Info( "Users テーブルをアップデートしました。[2→3]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( "Users テーブルのアップデートに失敗しました。[2→3]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 3:
                    #region " 3 → 4 "
                    //----------------
                    // 変更点:
                    // ・Users テーブルに RideLeft, ChinaLeft, SplashLeft カラムを追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // テータベースをアップデートしてデータを移行する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN RideLeft INTEGER NOT NULL DEFAULT 0" );  // 2018.2.11 現在、SQLite で複数カラムを一度に追加できる構文はない。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN ChinaLeft INTEGER NOT NULL DEFAULT 0" );
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN SplashLeft INTEGER NOT NULL DEFAULT 1" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            this.DataContext.ExecuteCommand( "VACUUM" );    // Vacuum はトランザクションの外で。
                            this.DataContext.SubmitChanges();
                            Log.Info( "Users テーブルをアップデートしました。[3→4]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( "Users テーブルのアップデートに失敗しました。[3→4]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 4:
                    #region " 4 → 5 "
                    //----------------
                    // 変更点:
                    // ・Users テーブルに DrumSound カラムを追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // テータベースをアップデートしてデータを移行する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN DrumSound INTEGER NOT NULL DEFAULT 1" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            this.DataContext.ExecuteCommand( "VACUUM" );    // Vacuum はトランザクションの外で。
                            this.DataContext.SubmitChanges();
                            Log.Info( $"Users テーブルをアップデートしました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( $"Users テーブルのアップデートに失敗しました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case 5:
                    #region " 5 → 6 "
                    //----------------
                    // 変更点:
                    // ・Users テーブルに LaneType カラムを追加。
                    this.DataContext.SubmitChanges();
                    using( var transaction = this.Connection.BeginTransaction() )
                    {
                        try
                        {
                            // テータベースをアップデートしてデータを移行する。
                            this.DataContext.ExecuteCommand( "ALTER TABLE Users ADD COLUMN LaneType NVARCHAR NOT NULL DEFAULT 'TypeA'" );
                            this.DataContext.SubmitChanges();

                            // 成功。
                            transaction.Commit();
                            this.DataContext.ExecuteCommand( "VACUUM" );    // Vacuum はトランザクションの外で。
                            this.DataContext.SubmitChanges();
                            Log.Info( $"Users テーブルをアップデートしました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                        catch
                        {
                            // 失敗。
                            transaction.Rollback();
                            throw new Exception( $"Users テーブルのアップデートに失敗しました。[{移行元DBバージョン}→{移行元DBバージョン + 1}]" );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                default:
                    throw new Exception( $"移行元DBのバージョン({移行元DBバージョン})がマイグレーションに未対応です。" );
            }
        }
    }
}
