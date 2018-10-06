using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FDK
{
    /// <summary>
    ///		任意の１つの要素を選択できる機能を持つ List。
    /// </summary>
    /// <typeparam name="T">要素の型。</typeparam>
    public class SelectableList<T> : List<T>
    {
        /// <summary>
        ///		現在選択されている要素のインデックス番号（0～Count-1）。
        ///		未選択またはリストが空なら、負数。
        /// </summary>
        public int SelectedIndex
        {
            get;
            protected set;
        } = -1;

        /// <summary>
        ///		現在選択されている要素。
        ///		未選択またはリストが空なら、default。
        /// </summary>
        public T SelectedItem
            => ( 0 <= this.SelectedIndex ) ? this[ this.SelectedIndex ] : default;

        /// <summary>
        ///		コンストラクタ。
        /// </summary>
        public SelectableList()
        {
        }

        /// <summary>
        ///		要素を選択する。
        /// </summary>
        /// <param name="インデックス番号">選択する要素のインデックス番号（0～Count-1）。負数なら未選択状態にする。</param>
        /// <returns>選択または未選択状態にできたら true、できなかったら false。</returns>
        public bool SelectItem( int インデックス番号 )
        {
            if( ( 0 == this.Count ) ||              // リストが空だったり、
                ( this.Count <= インデックス番号 ) )    // 指定されたインデックスが大きすぎた場合には
            {
                return false;                       // false を返す。
            }

            this.SelectedIndex = インデックス番号;      // 0 または 負数は OK。

            return true;
        }
        /// <summary>
        ///		要素を選択する。
        /// </summary>
        /// <remarks>
        ///		要素が存在しない場合には、未選択状態になる。
        /// </remarks>
        /// <param name="要素">選択する要素。</param>
        /// <returns>選択または未選択状態にできたら true、できなかったら false。</returns>
        public bool SelectItem( T 要素 )
        {
            if( 0 == this.Count )
                return false;

            this.SelectedIndex = this.IndexOf( 要素 );      // 見つからなければ負数が返される --> 未選択状態になる。

            return true;
        }
        /// <summary>
        ///		要素を選択する。
        /// </summary>
        /// <remarks>
        ///		要素が存在しない場合には、未選択状態になる。
        /// </remarks>
        /// <param name="selector">選択すべき要素に対してtrueを返す。複数返した場合は最初にtrueが返された要素が選択される。</param>
        /// <returns>選択または未選択状態にできたら true、できなかったら false。</returns>
        public bool SelectItem( Func<T, bool> selector )
        {
            for( int i = 0; i < this.Count; i++ )
            {
                if( selector( this[ i ] ) )
                {
                    this.SelectedIndex = i;
                    return true;
                }
            }

            this.SelectedIndex = -1;    // 未選択状態
            return false;
        }

        /// <summary>
        ///		リストの先頭の要素を選択する。
        /// </summary>
        /// <returns>選択できたら true、できなかったら false。</returns>
        public bool SelectFirst()
        {
            if( 0 == this.Count )   // リストが空なら
                return false;       // false を返す。

            this.SelectedIndex = 0;

            return true;
        }

        /// <summary>
        ///		リストの末尾の要素を選択する。
        /// </summary>
        /// <returns>選択できたら true、できなかったら false。</returns>
        public bool SelectLast()
        {
            if( 0 == this.Count )   // リストが空なら
                return false;       // false を返す。

            this.SelectedIndex = this.Count - 1;

            return true;
        }

        /// <summary>
        ///		現在選択されている要素のひとつ後ろの要素を選択する。
        /// </summary>
        /// <param name="Loop">true にすると、すでに末尾だったら先頭を選択する。</param>
        /// <returns>選択できたら true、できなかったら false。</returns>
        public bool SelectNext( bool Loop = false )
        {
            if( ( 0 > this.SelectedIndex ) ||               // 未選択だったり
                ( 0 == this.Count ) )                       // リストが空だったりする場合は
            {
                return false;                               // false を返す。
            }

            if( this.Count - 1 <= this.SelectedIndex )      // すでに末尾に位置してたりする場合は
            {
                if( Loop )
                    this.SelectedIndex = 0;                 // Loop が true なら先頭へ。
                else
                    return false;                           // それ以外は false を返す。
            }
            else
            {
                this.SelectedIndex++;
            }

            return true;
        }

        /// <summary>
        ///		現在選択されている要素のひとつ前の要素を選択する。
        /// </summary>
        /// <param name="Loop">true にすると、すでに先頭だったら末尾を選択する。</param>
        /// <returns>選択できたら true、できなかったら false。</returns>
        public bool SelectPrev( bool Loop = false )
        {
            if( ( 0 > this.SelectedIndex ) ||               // 未選択だったり
                ( 0 == this.Count ) )                       // リストが空だったりする場合は
            {
                return false;                               // false を返す。
            }

            if( 0 == this.SelectedIndex )                   // すでに先頭に位置してたりする場合は
            {
                if( Loop )
                    this.SelectedIndex = this.Count - 1;    // Loop が true なら末尾へ。
                else
                    return false;                           // それ以外は false を返す。
            }
            else
            {
                this.SelectedIndex--;
            }

            return true;
        }
    }
}
