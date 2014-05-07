// <copyright file="Extensions.cs">
//    Copyright (C) 2014  PythEch
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either Version 3 of the License, or
//    (at your option) any later Version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>Cygnus - Cydia-like APT Client for Windows</summary>

namespace Cygnus.Extensions
{
    using System;
    using System.Collections.Generic;
    using XPTable.Models;

    /// <summary>
    /// Some String extensions that I find useful.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts string to a proper URL format.
        /// </summary>
        /// <example>
        /// HTtp://Apt.SaURik.com -> http://apt.saurik.com/
        /// </example>
        /// <param name="str">The string to normalize.</param>
        /// <returns>Normalized URL.</returns>
        public static string toValidURL(this String str)
        {
            return new Uri(str).ToString();
        }

        /// <summary>
        /// Makes it possible to check if a string contains something case-insensitively.
        /// </summary>
        /// <param name="str">The extension string.</param>
        /// <param name="toCheck">The string to check.</param>
        /// <returns>true if toCheck is in string, otherwise false.</returns>
        /// <remarks>
        /// Credits: http://stackoverflow.com/a/444818
        /// </remarks>
        public static bool ContainsIgnoreCase(this String str, string toCheck)
        {
            return str.IndexOf(toCheck, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        /// <summary>
        /// String.IsNullOrWhite"S"pace() shortcut.
        /// </summary>
        /// <param name="str">The string to test.</param>
        /// <returns>True if string contains whitespace, else false.</returns>
        public static bool IsNullOrWhitespace(this String str)
        {
            return String.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// int.TryParse() shortcut.
        /// </summary>
        /// <param name="str">The string to cast.</param>
        /// <param name="defaultValue">Default value to return in case conversion fails.</param>
        /// <returns>Int32 equivalent of the string. defaultValue if something goes wrong.</returns>
        public static int ToInt32(this String str, int defaultValue=0)
        {
            int result;
            if (Int32.TryParse(str, out result))
            {
                return result;
            }
            // else
            return defaultValue;
        }

        /// <summary>
        /// String.Format() shortcut that I can't live without.
        /// </summary>
        /// <param name="str">The string to format.</param>
        /// <param name="arg0">Argument #0.</param>
        /// <returns>Formatted string.</returns>
        public static string FormatWith(this String str, object arg0)
        {
            return String.Format(str, arg0);
        }

        /// <summary>
        /// String.Format() shortcut that I can't live without.
        /// </summary>
        /// <param name="str">The string to format.</param>
        /// <param name="arg0">Argument #0.</param>
        /// <param name="arg1">Argument #1.</param>
        /// <returns>Formatted string.</returns>
        public static string FormatWith(this String str, object arg0, object arg1)
        {
            return String.Format(str, arg0, arg1);
        }

        /// <summary>
        /// String.Format() shortcut that I can't live without.
        /// </summary>
        /// <param name="str">The string to format.</param>
        /// <param name="arg0">Argument #0.</param>
        /// <param name="arg1">Argument #1.</param>
        /// <param name="arg2">Argument #2.</param>
        /// <returns>Formatted string.</returns>
        public static string FormatWith(this String str, object arg0, object arg1, object arg2)
        {
            return String.Format(str, arg0, arg1, arg2);
        }

        /// <summary>
        /// String.Format() shortcut that I can't live without.
        /// </summary>
        /// <param name="str">The string to format.</param>
        /// <param name="args">The arguments to send.</param>
        /// <returns>Formatted string.</returns>
        public static string FormatWith(this String str, params object[] args)
        {
            return String.Format(str, args);
        }
    }

    public static class XPTableExtensions
    {
        /// <summary>
        /// Row equivalent of XPTable.CellAt() method.
        /// Will only work when XPTable.EnableWordWrap is false.
        /// </summary>
        /// <param name="tableModel">TableModel to extend.</param>
        /// <param name="yPosition">Y-Position of Row to check.</param>
        /// <returns>The row at given position. Null if it doesn't exist.</returns>
        public static Row RowAt(this TableModel tableModel, int yPosition)
        {
            int rowIndex = yPosition / tableModel.RowHeight;

            return tableModel.Rows[rowIndex];
        }
    }
}
