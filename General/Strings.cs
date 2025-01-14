﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable SYSLIB1045 // GeneratedRegexAttribute
#pragma warning disable IDE0056 // Indexoperator

namespace Shared.StringsNS
{
    /// <summary>
    /// Extension class for string operations.
    /// </summary>
    public static partial class Strings
    {
        #region Regex

        /// <summary>
        /// Regular expression for matching an email address.<br/>
        /// General Email Regex (RFC 5322 Official Standard) from https://emailregex.com.
        /// </summary>
        public const string EmailRegex = "(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])";

        /// <summary>
        /// Regular expression of HTML tags to remove.
        /// </summary>
        public const string RemoveHtmlTagsRegex = @"(?></?\w+)(?>(?:[^>'""]+|'[^']*'|""[^""]*"")*)>";

        /// <summary>
        /// Regular expression for removing comments from HTML.
        /// </summary>
        public const string RemoveHtmlCommentsRegex = "<!--.*?-->";

        /// <summary>
        /// Regular expression for removing scripts from HTML.
        /// </summary>
        public const string RemoveHtmlScriptsRegex = @"(?s)<script.*?(/>|</script>)";

        /// <summary>
        /// Regular expression for removing styles from HTML.
        /// </summary>
        public const string RemoveHtmlStylesRegex = @"(?s)<style.*?(/>|</style>)";

        /// <summary>
        /// Capture groups are<br/>
        /// dir: directory full name<br/>
        /// vol: volume (C:/ or /)<br/>
        /// folder: all folder name captures (Value has last entry only)<br/>
        /// file: filename including extension<br/>
        /// name: filename excluding extension<br/>
        /// ext: extension including period<br/>
        /// </summary>
#if NET7_0_OR_GREATER
        [GeneratedRegex(@"\d+")]
        public static partial Regex Rx_Integer();
#else
        /// <summary> Splits path into segments. </summary>
        public static Regex Rx_Integer() => _Rx_Integer ??= new(@"\d+", RegexOptions.Compiled);
        private static Regex? _Rx_Integer;
#endif

        /// <summary>
        /// Extension to check if match is success.
        /// </summary>
        public static bool TryMatch(this Regex rx, string input, out Match match)
        {
            match = rx.Match(input);
            return match.Success;
        }

        #endregion

        #region Args

        /// <summary>Search for characters that need to be escaped in shell commands (Unix/Windows). Excludes \n+</summary>
        public static Regex Rx_ShellEscape => _Rx_ShellEscape ??= new(@"[`~!#$&*()\t{}[\]|;'<>? ""]", RegexOptions.Compiled);
        private static Regex? _Rx_ShellEscape;

        /// <summary>
        /// Joins an array of arguments into a single string, which can be used for commands. Puts arguments in quotes, if necessary.
        /// </summary>
        public static string JoinArgs(this IEnumerable<string> args)
        {
            if (args is null)
                return "";

            var sb = GetSb();

            foreach (string arg in args)
            {
                if (arg is null)
                    continue;

                if (sb.Length > 0)
                    sb.Append(' ');

                if (Rx_ShellEscape.IsMatch(arg) || arg is "")
                {
                    sb.Append('\"');
                    foreach (char c in arg)
                    {
                        if (c is '\"')
                        {
                            sb.Append('\"');
                            sb.Append('\"');
                        }
                        else
                            sb.Append(c);
                    }
                    sb.Append('\"');
                }
                else
                {
                    sb.Append(arg);
                }
            }

            return FlushSb();
        }

        /// <summary>
        /// Splits arguments into an array. Handles quotes.
        /// </summary>
        public static List<string> SplitArgs(this string args)
        {
            var result = new List<string>();
            var sb = new StringBuilder();

            bool isEnd = true;
            bool inQuote = false;
            int quoteCount = 0;
            foreach (char c in args)
            {
                if (c is not ' ')
                    isEnd = false;
                if (c is '\"')
                {
                    inQuote = !inQuote;
                    if (inQuote && quoteCount > 0)
                    {
                        sb.Append('\"');
                        quoteCount = 0;
                    }
                    quoteCount = 1;
                }
                else
                {
                    quoteCount = 0;
                    if (!inQuote && c is ' ')
                    {
                        if (sb.Length > 0 || !isEnd)
                        {
                            result.Add(sb.ToString());
                            sb.Clear();
                            isEnd = true;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            if (!isEnd)
                result.Add(sb.ToString());

            return result;
        }

        #endregion

        #region Conversions

        #endregion

        #region Enum

        #endregion

        #region Path

        /// <summary>
        /// Converts string into valid absolute path.<br/>
        /// Returns null, if invalid path (e.g. illegal characters).
        /// </summary>
        public static string? FilterPath(this string path)
        {
            try
            {
                path = path.Trim();
                var dir = new DirectoryInfo(path);
                //Console.WriteLine($"{dir.FullName}, {dir.Name}, {dir.Parent}");
                return dir.FullName;
            } catch (Exception) { return default; }
        }

        public static void GetDirCompletion(this string path, List<string> dirs)
        {
            try
            {
                dirs.Clear();
                var info = new DirectoryInfo(path);
                var parent = info.Parent;
                string search = $"{info.Name}*";

                if (parent == null || info.Exists)
                {
                    search = "*";
                    parent = info;
                }

                var subs = parent.GetDirectories(search, SearchOption.TopDirectoryOnly);
                dirs.AddRange(subs.Where(w => !w.Attributes.HasFlag(FileAttributes.Hidden)).Select(s => s.FullName));
                //Debug.WriteLine($"list={dirs.Join()}");
            } catch (Exception) { }
        }

        /// <summary>
        /// Returns to windows style volume expanded string "C:\".
        /// Otherwise returns input string.
        /// </summary>
        public static string? ExpandWindowsVolume(this string letter)
        {
            if (letter == null || letter.Length < 1 || letter.Length > 2)
                return letter;
            if (!letter[0].IsLetter())
                return letter;
            if (letter.Length == 2 && letter[1] != ':')
                return letter;
            return $"{letter[0]}:\\";
        }

        public static string MergePaths(IEnumerable<string> paths)
        {
            if (paths == null)
                return "";

            var sb = GetSb();
            bool first = true;
            foreach (string path in paths)
            {
                string? full = FilterPath(path);
                if (full.IsEmpty())
                    continue;

                if (!first)
                    sb.Append(System.IO.Path.PathSeparator);
                else
                    first = false;
                sb.Append(full);
            }
            return FlushSb();
        }

        private static readonly char[] InvalidFileNameChars =
        [
            '"', '<', '>', '|', '\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005',
            '\u0006', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\u000e', '\u000f',
            '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019',
            '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f', ':', '*', '?', '\\',
            '/'
        ];

        /// <summary>
        /// Checks for files or directories.
        /// </summary>
        public static bool PathExists(string path)
        {
            if (File.Exists(path))
                return true;
            if (Directory.Exists(path))
                return true;
            return false;
        }

        private static Regex? _rxFileBrackets;
        private static Regex? _rxFileExtension;
        /// <summary>
        /// Returns file path that does not exist. Appends (1) or increases existing numberation, if file already exists.
        /// </summary>
        public static string GetUniqueFilename(string path)
        {
            _rxFileBrackets ??= new Regex(@"(.*)\((\d+)\)(?!.*[\/\\])(.*)", RegexOptions.Compiled | RegexOptions.RightToLeft);
            _rxFileExtension ??= new Regex(@"(.*)(?!.*[\/\\])(\..*)", RegexOptions.Compiled | RegexOptions.RightToLeft);

            while (PathExists(path))
            {
                Match match;
                if ((match = _rxFileBrackets.Match(path)).Success && int.TryParse(match.Groups[2].Value, out int number))
                    path = match.Result($"$1({number + 1})$3");
                else if ((match = _rxFileExtension.Match(path)).Success)
                    path = match.Result("$1(1)$2");
                else
                    path += "(1)";
            }

            return path;
        }

        public static string? FilterFilename(this string filename)
        {
            if (filename == null)
                return null;

            var sb = GetSb();
            foreach (char c in filename.Trim())
            {
                if (!InvalidFileNameChars.Contains(c))
                    sb.Append(c);
            }
            return FlushSb();
        }

        #endregion

        /// <summary>
        /// Escapes Unicode and ASCII non printable characters
        /// </summary>
        /// <param name="input">The string to convert</param>
        /// <returns>An escaped string literal</returns>
        public static string ToLiteral(this string input)
        {
            var sb = new StringBuilder(input.Length + 2);
            foreach (char c in input)
            {
                switch (c)
                {
                    case '\"': sb.Append(@"\"""); break;
                    case '\\': sb.Append(@"\\"); break;
                    case '\b': sb.Append(@"\b"); break;
                    case '\f': sb.Append(@"\f"); break;
                    case '\n': sb.Append(@"\n"); break;
                    case '\r': sb.Append(@"\r"); break;
                    case '\t': sb.Append(@"\t"); break;
                    case '\'': sb.Append(@"\u0027"); break;
                    case '\0': sb.Append(@"\u0000"); break;
                    case '\a': sb.Append(@"\u0007"); break;
                    case '\v': sb.Append(@"\u000B"); break;
                    case '´': sb.Append(@"\u00B4"); break;
                    case '`': sb.Append(@"\u0060"); break;
                    case >= (char)0x20 and <= (char)0x7e:
                        sb.Append(c);
                        break;
                    default:
                        if (c > 0xFFFF)
                            throw new InvalidDataException();
                        sb.Append(@"\u");
                        sb.Append(((int)c).ToString("X4"));
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>Joins an enumeration with a value converter and a delimiter to a string</summary>
        /// <typeparam name="T">The inner type of the enumeration</typeparam>
        /// <param name="enumeration">The enumeration</param>
        /// <param name="converter">An optional value converter (from T to string)</param>
        /// <param name="delimiter">An optional delimiter</param>
        /// <returns>The values joined into a string</returns>
        ///
        public static string Join<T>(this IEnumerable<T> enumeration, Func<T, string>? converter = null, string delimiter = ", ")
        {
            converter ??= t => t?.ToString() ?? "";
            var sb = GetSb();
            foreach (var obj in enumeration)
            {
                if (sb.Length > 0)
                    sb.Append(delimiter);

                sb.Append(converter(obj));
            }
            return FlushSb();
        }

        /// <summary>Returns substring. Always excludes char 'c'. Returns null, if index is out of range or char not found.</summary>
        /// <param name="str">source string</param>
        /// <param name="c">char to search for</param>
        /// <param name="start">start index; negative number search last index instead</param>
        /// <param name="tail">get tail instead of head</param>
        public static string? TrySubstring(this string str, char c, int start = 0, bool tail = false)
        {
            try
            {
                if (tail)
                {
                    if (start < 0)
                        return str.Substring(str.LastIndexOf(c) + 1);
                    return str.Substring(str.IndexOf(c, start) + 1);
                }

                if (start < 0)
                    return str.Substring(0, str.LastIndexOf(c));
                return str.Substring(start, str.IndexOf(c, start));
            } catch (Exception)
            {
                return null;
            }
        }

        #region Search and Compare

        public static bool IsNumber(this char c)
        {
            return c is >= (char)48 and <= (char)57;
        }

        public static bool IsUppercase(this char c)
        {
            return c is >= (char)65 and <= (char)90;
        }

        public static bool IsLowercase(this char c)
        {
            return c is >= (char)97 and <= (char)122;
        }

        public static bool IsLetter(this char c)
        {
            return c is >= (char)65 and <= (char)90 or >= (char)97 and <= (char)122;
        }

        public static bool IsAlphanumeric(this char c)
        {
            return c is >= (char)48 and <= (char)57 or >= (char)65 and <= (char)90 or >= (char)97 and <= (char)122;
        }

        public static bool IsNotSpaced(this StringBuilder sb)
        {
            if (sb.Length == 0)
                return false;

            return sb[sb.Length - 1] != ' ';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithO(this string source, string value) => source.StartsWith(value, StringComparison.Ordinal);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithOI(this string source, string value) => source.StartsWith(value, StringComparison.OrdinalIgnoreCase);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWithO(this string source, string value) => source.EndsWith(value, StringComparison.Ordinal);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsOI(this string source, string value) => source.Equals(value, StringComparison.OrdinalIgnoreCase);

        public static bool ContainsOI(this IEnumerable<string> collection, string value)
        {
            foreach (var item in collection)
                if (value.Equals(item, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        public static bool IsEmpty(this string? str)
        {
            return str is null or "";
        }

        #endregion

        /// <summary>
        /// Evaluator for Regex.Replace extension.
        /// </summary>
        public delegate string RegexEvaluator(Match match, int index, int count);

        /// <summary>
        /// Regex.Replace, but with additional index and count values.
        /// </summary>
        public static string Replace(this Regex rx, string input, RegexEvaluator evaluator)
        {
            var matches = rx.Matches(input);
            if (matches.Count <= 0)
                return input;

            var sb = GetSb();
            int index = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                sb.Append(input, index, match.Index - index); // append non-matches
                index = match.Index + match.Length;
                sb.Append(evaluator(match, i, matches.Count)); // append match replacement
            }

            if (index < input.Length)
            {
                sb.Append(input, index, input.Length - index);
            }
            return FlushSb();
        }

        #region Buffer

        private static readonly StringBuilder _sb = new();
        private static StringBuilder GetSb()
        {
            System.Threading.Monitor.Enter(_sb);
            _sb.Clear();
            return _sb;
        }
        private static string FlushSb()
        {
            string text = _sb.ToString();
            _sb.Clear();
            System.Threading.Monitor.Exit(_sb);
            return text;
        }

        #endregion
    }
}
