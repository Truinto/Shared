using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        public static Regex Rx_Integer() => _Rx_Integer ??= new(@"\d+");
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

        /// <summary>
        /// Turns a string into <see cref="DateTime"/>. Much more forgiving than DateTime.Parse.
        /// </summary>
        public static DateTime ToDateTime(this string? input)
        {
            if (input is null)
                return DateTime.MinValue;
            int tlen = input.Length;
            if (tlen < 4)
                return DateTime.MinValue;

            // parse year
            int i = 4;
            if (!int.TryParse(input.AsSpan(0, 4), out int year))
            {
                DateTime.TryParse(input, out var result);
                return result;
            }

            try
            {
                // try parse all other two digit segments
                if (!parseOneOrTwo(out int month) || month is < 1 or > 12)
                    return new DateTime(year, 1, 1);
                if (!parseOneOrTwo(out int day) || day is < 1 or > 31)
                    return new DateTime(year, month, 1);
                if (!parseOneOrTwo(out int hour) || hour is < 0 or > 23)
                    return new DateTime(year, month, day);
                if (!parseOneOrTwo(out int minute) || minute is < 0 or > 59)
                    return new DateTime(year, month, day, hour, 0, 0);
                if (!parseOneOrTwo(out int second) || second is < 0 or > 59)
                    return new DateTime(year, month, day, hour, minute, 0);
                
                // handle potential time offsets
                if (i >= tlen)
                { }
                else if (input[i] is 'Z')
                    return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
                else if (input[i] is '+')
                {
                    if (parseOneOrTwo(out int offset_hours) && parseOneOrTwo(out int offset_minutes))
                        return new DateTimeOffset(year, month, day, hour, minute, second, new TimeSpan(offset_hours, offset_minutes, 0)).LocalDateTime;
                }
                else if (input[i] is '-')
                {
                    if (parseOneOrTwo(out int offset_hours) && parseOneOrTwo(out int offset_minutes))
                        return new DateTimeOffset(year, month, day, hour, minute, second, new TimeSpan(-offset_hours, -offset_minutes, 0)).LocalDateTime;
                }

                return new DateTime(year, month, day, hour, minute, second);

            } catch (Exception)
            {
                Trace.WriteLine($"error while parsing datetime '{input}'");
                DateTime.TryParse(input, out var result);
                return result;
            }

            bool parseOneOrTwo(out int number)
            {
                number = 0;
                for (; ; i++) // foward next number
                {
                    if (i >= tlen)
                        return false;
                    if (input[i] is >= '0' and <= '9')
                        break;
                }

                // read two or one digit
                if (i + 1 < tlen && input[i + 1] is >= '0' and <= '9')
                {
                    number = int.Parse(input.AsSpan(i, 2));
                    i += 2;
                    return true;
                }
                else
                {
                    number = input[i++] - 0x30; // turn char into digit
                    return true;
                }
            }
        }

        /// <summary>Joins an enumeration with a value converter and a delimiter to a string</summary>
        /// <param name="enumeration">The enumeration</param>
        /// <param name="converter">An optional value converter (from T to string)</param>
        /// <param name="delimiter">An optional delimiter</param>
        /// <returns>The values joined into a string</returns>
        public static string Join(this IEnumerable enumeration, Func<object, string>? converter = null, string delimiter = ", ")
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

        /// <summary>Joins an enumeration with a value converter and a delimiter to a string</summary>
        /// <typeparam name="T">The inner type of the enumeration</typeparam>
        /// <param name="enumeration">The enumeration</param>
        /// <param name="converter">An optional value converter (from T to string)</param>
        /// <param name="delimiter">An optional delimiter</param>
        /// <returns>The values joined into a string</returns>
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

        #endregion

        #region Enum

        #endregion

        #region Search and Compare

        /// <summary>
        /// A <see cref="string.Compare(string?, string?)"/> variant that uses padding for each number pair to get a natural sorting of numbers.
        /// </summary>
        public static int CompareNatural(string? str1, string? str2)
        {
            if (str1 == null)
                return str2 == null ? 0 : -1;
            if (str2 == null)
                return 1;

            for (int i = 0, j = 0; i < str1.Length && j < str2.Length; i++, j++)
            {
                char c1 = str1[i];
                char c2 = str2[j];
                if (c1.IsNumber() && c2.IsNumber())
                {
                    int start1 = i, start2 = j;
                    while (++i < str1.Length && str1[i].IsNumber()) ;
                    while (++j < str2.Length && str2[j].IsNumber()) ;
                    int max = Math.Max(i - start1, j - start2);
                    string pad1 = str1[start1..i].PadLeft(max, '0');
                    string pad2 = str2[start2..j].PadLeft(max, '0');
                    int compare = string.Compare(pad1, pad2, StringComparison.Ordinal);
                    if (compare != 0)
                        return compare;
                    if (i >= str1.Length)
                        return j >= str2.Length ? 0 : -1;
                    if (j >= str2.Length)
                        return 1;
                    c1 = str1[i];
                    c2 = str2[j];
                }
                if (c1 == c2)
                    continue;
                return c1 < c2 ? -1 : 1;
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNumber(this char c)
        {
            return c is >= '0' and <= '9';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUppercase(this char c)
        {
            return c is >= 'A' and <= 'Z';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLowercase(this char c)
        {
            return c is >= 'a' and <= 'z';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLetter(this char c)
        {
            return c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlphanumeric(this char c)
        {
            return c is >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsI(this char c1, char c2)
        {
            if (c1 == c2)
                return true;
            if (c1 is >= 'a' and <= 'z')
                return (c1 - 0x20) == c2;
            if (c2 is >= 'a' and <= 'z')
                return c1 == (c2 - 0x20);
            return false;
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

        #region Substring

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

        /// <inheritdoc cref="TrySubstring(string, char, int, bool)"/>
        public static bool TrySubstring(this string str, char c, int start, bool tail, [NotNullWhen(true)] out string? outstring)
        {
            outstring = TrySubstring(str, c, start, tail);
            return str != null;
        }

#if NET7_0_OR_GREATER
        /// <summary>
        /// Returns substring. Always excludes char 'c'. Returns false, if index out of bounds.<br/>
        /// Example with c='.'<br/>
        /// M.Fru.Bar<br/>
        /// 0 ^ 1 ^ 2
        /// </summary>
        public static bool TrySubstringByOccurrence(this string str, char c, Index occurrence, out string outstring)
        {
            int len = str.Length;
            int count = 0;
            int start = 0;
            int end = len;

            if (!occurrence.IsFromEnd)
            {
                for (int i = 0; i < len; i++)
                {
                    if (str[i] == c)
                    {
                        if (count++ >= occurrence.Value)
                        {
                            outstring = str[start..i];
                            return true;
                        }
                        start = i + 1;
                    }
                }
            }
            else
            {
                for (int i = len - 1; i >= 0; i--)
                {
                    if (str[i] == c)
                    {
                        if (count++ >= occurrence.Value)
                        {
                            outstring = str[(i + 1)..end];
                            return true;
                        }
                        end = i;
                    }
                }
            }

            if (count >= occurrence.Value)
            {
                outstring = str[start..end];
                return true;
            }

            outstring = str;
            return false;
        }
#endif

        #endregion

        #region Replacement

        /// <summary>
        /// Evaluator for Regex.Replace extension.
        /// </summary>
        public delegate string RegexEvaluator(Match match, int index, int count);

        /// <summary>
        /// Regex.Replace, but with additional index and count values.<br/>
        /// Index is zero based.
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

        #endregion

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
