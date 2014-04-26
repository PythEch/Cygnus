namespace Cygnus.Extensions
{
    using System;
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
