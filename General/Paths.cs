using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable SYSLIB1045 // GeneratedRegexAttribute

namespace Shared.PathsNS
{
    public enum FileType
    {
        Undefined,
        Directory,
        File,
    }

    /// <summary>
    /// WIP. Tool to handle path operations.
    /// </summary>
    public class PathInfo
    {
        public static char PathSeparator => System.IO.Path.DirectorySeparatorChar;

        //private bool isDirty;

        public PathInfo(string? path) //WIP this could use Regex function instead
        {
            if (string.IsNullOrEmpty(path))
                path = ".";
            path = path.Replace(PathSeparator == '/' ? '\\' : '/', PathSeparator);

            int indexSlash = path.LastIndexOf(PathSeparator);
            int indexDot = path.LastIndexOf('.');
            bool hasNoExtension = indexDot <= indexSlash + 1;

            this.FullName = path;
            this.Root = System.IO.Path.GetPathRoot(path) ?? "";
            this.IsAbsolute = this.Root.Length > 0;
            this.Directory = path.Substring(0, indexSlash + 1);
            if (hasNoExtension)
            {
                this.FileNameNoExtension = path.Substring(indexSlash + 1);
                if (this.FileNameNoExtension.EndsWith("."))
                    this.FileNameNoExtension = this.FileNameNoExtension.Substring(1);
                this.Extension = "";
                this.FileName = this.FileNameNoExtension;
            }
            else
            {
                this.FileNameNoExtension = path.Substring(indexSlash + 1, indexDot - indexSlash - 1);
                this.Extension = path.Substring(indexDot + 1);
                this.FileName = $"{this.FileNameNoExtension}.{this.Extension}";
            }
        }

        public bool IsValid { get; } //WIP

        public string FullName { get; }

        public string Root { get; }

        public string Directory { get; }

        public string FileNameNoExtension { get; }

        public string Extension { get; }

        public string FileName { get; }

        public FileType Type { get; } //WIP

        public bool IsAbsolute { get; } //WIP

        public string PathAbsolute(string? workingDirectory = null)
        {
            if (this.IsAbsolute && !string.IsNullOrEmpty(workingDirectory))
                Trace.WriteLine($"[Warning] {typeof(PathInfo).FullName}.PathAbsolute trying to set working directory on an absolute path '{this.FullName}'");

            if (this.IsAbsolute)
                return this.FullName;

            workingDirectory ??= System.IO.Directory.GetCurrentDirectory(); //System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
            return workingDirectory + this.FullName;

            throw new NotImplementedException();
        }

        internal static void Sandbox()
        {
            //var files = Directory.GetFiles(TargetPath.Groups["dir"].Value)
            //    .OrderBy(f => Strings.Rx_Number().Replace(f, match => match.Value.PadLeft(4, '0')));

            System.IO.Directory.Delete("");
            System.IO.Directory.Exists("");
            System.IO.Directory.CreateDirectory("");

            System.IO.File.Delete("");
            System.IO.File.Exists("");
            System.IO.File.CreateText("");

            System.IO.Path.ChangeExtension("", "");
            System.IO.Path.Combine("", "");
            System.IO.Path.GetDirectoryName(""); // warning, this is inconsistent
            System.IO.Path.GetExtension("");
            System.IO.Path.GetFileName("");
            System.IO.Path.GetFileNameWithoutExtension("");
            System.IO.Path.GetFullPath("");
            System.IO.Path.GetInvalidFileNameChars();
            System.IO.Path.GetPathRoot("");
            System.IO.Path.GetRandomFileName();
            System.IO.Path.GetTempFileName();
            System.IO.Path.GetTempPath();
            System.IO.Path.HasExtension("");
            System.IO.Path.IsPathRooted("");

            var di = new System.IO.DirectoryInfo("");
            _ = di.Parent;
            _ = di.Exists;
            _ = di.FullName;
            di.Create();
            di.Delete();
            di.EnumerateDirectories();
            di.EnumerateFiles();
            di.GetDirectories();
            di.GetFiles();
            di.MoveTo("");
            di.CreateSubdirectory("");

            var fi = new System.IO.FileInfo("");
            _ = fi.Exists;
            fi.CreateText();
            fi.Delete();

        }
    }

    /// <summary>
    /// Collection of special folders.
    /// </summary>
    public static partial class Paths
    {
        public static readonly char[] InvalidFileNameChars =
        [
            '"', '<', '>', '|', '\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005',
            '\u0006', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\u000e', '\u000f',
            '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019',
            '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f', ':', '*', '?', '\\',
            '/'
        ];

        public static string? FilterFilename(this string filename)
        {
            if (filename == null)
                return null;

            var sb = new StringBuilder();
            foreach (char c in filename.Trim())
            {
                if (!InvalidFileNameChars.Contains(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }

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
        [GeneratedRegex(@"^(?<dir>(?<vol>(?:[A-Za-z]:)?[\\/])?(?:(?<folder>[^\\/]+)[\\/])*)(?<file>(?<name>[^\\/]*?)?(?<ext>\.[^\\/\.]+)?)$")]
        public static partial Regex Rx_Path();
#else
        /// <summary> Splits path into segments. </summary>
        public static Regex Rx_Path() => _Rx_Path ??= new(@"^(?<dir>(?<vol>(?:[A-Za-z]:)?[\\/])?(?:(?<folder>[^\\/]+)[\\/])*)(?<file>(?<name>[^\\/]*?)?(?<ext>\.[^\\/\.]+)?)$");
        private static Regex? _Rx_Path;
#endif

        /// <summary>True if paths are equal. Resolves relative paths. Ignores closing path separator.</summary>
        public static bool AreEqual(this FileInfo? path1, FileInfo? path2)
        {
            if (path1 is null || path2 is null)
                return false;
            if (ReferenceEquals(path1, path2))
                return true;
            return AreEqual(path1.FullName, path2.FullName);
        }

        /// <summary>
        /// True if path is valid.
        /// </summary>
        public static bool IsValidPath(string path)
        {
            if (path is null)
                return false;

            int i = 0;

            if (OperatingSystem.IsWindows())
            {
                if (path.StartsWith("\\\\?\\")) // extended-length path
                {
                    i = 4;
                    if (path.Contains('/') || path.Contains(".\\") || path.Contains("..\\"))
                        return false;
                }

                if (path.Length > i + 1 && path[i + 1] == ':' && path[i] is (> 'A' and < 'Z' or > 'a' and < 'z'))
                    i += 2;
            }

            if (path.Length <= i) // must have at least one character
                return false;

            for (; i < path.Length; i++)
            {
                char c = path[i];
                if (c is '/' or '\\')
                { }
                else if (Path.GetInvalidFileNameChars().Contains(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks for files or directories.
        /// </summary>
        public static bool Exists(string path)
        {
            if (File.Exists(path))
                return true;
            if (Directory.Exists(path))
                return true;
            return false;
        }

        /// <summary>True if paths are equal. Resolves relative paths. Ignores closing path separator.</summary>
        public static bool AreEqual(string path1, string path2)
        {
            if (path1 is null or "")
                return path1 == path2;

            path1 = Path.GetFullPath(path1);
            path2 = Path.GetFullPath(path2);

            int length1 = path1.Length;
            int length2 = path2.Length;

            int length;
            if (length1 == length2)
                length = length1;
            else if (length1 - 1 == length2 && path1[length2] is '/' or '\\')
                length = length2;
            else if (length1 == length2 - 1 && path2[length1] is '/' or '\\')
                length = length1;
            else
                return false;

            return string.Compare(
                path1, 0,
                path2, 0,
                length,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal
                ) == 0;
        }

        public static List<string> Split(string? paths)
        {
            var list = new List<string>();
            if (paths is null or "")
                return list;

            bool isQuote = false;
            int len = paths.Length;
            int lastSemi = -1;
            for (int i = 0; ; i++)
            {
                if (i == len)
                {
                    if (lastSemi + 1 < len && paths[lastSemi + 1] is '\"')
                        list.Add(paths[(lastSemi + 2)..(i - 1)]);
                    else
                        list.Add(paths[(lastSemi + 1)..(i)]);
                    break;
                }

                if (paths[i] is '\"')
                    isQuote = !isQuote;
                else if (!isQuote && paths[i] is ';')
                {
                    if (paths[lastSemi + 1] is '\"')
                        list.Add(paths[(lastSemi + 2)..(i - 1)]);
                    else
                        list.Add(paths[(lastSemi + 1)..(i)]);
                    lastSemi = i;
                }
            }
            return list;
        }

#if NET7_0_OR_GREATER
        [GeneratedRegex(@"^(.*?)([^\\\/]+?)(?:\((\d+)\))?(\.\w+)?$")]
        private static partial Regex Rx_UniqueFilename();
#else
        public static Regex Rx_UniqueFilename() => _Rx_UniqueFilename ??= new(@"^(.*?)([^\\\/]+?)(?:\((\d+)\))?(\.\w+)?$");
        private static Regex? _Rx_UniqueFilename;
#endif
        /// <summary>
        /// Returns file path that does not exist. Appends (1) or increases existing numberation, if file already exists.
        /// </summary>
        public static string GetUniqueFilename(string path)
        {
            if (!Exists(path))
                return path;

            var m1 = Rx_UniqueFilename().Match(path);
            if (!m1.Success)
                throw new Exception($"Path malformed '{path}'");

            string parent = m1.Groups[1].Value;
            if (parent.Length == 0)
                parent = ".";

            int number = 0;
            foreach (var p in Directory.GetFileSystemEntries(parent, $"{m1.Groups[2].Value}*{m1.Groups[3].Value}", SearchOption.TopDirectoryOnly))
            {
                var m2 = Rx_UniqueFilename().Match(p);
                if (m2.Success && m2.Groups[3].Success)
                    number = Math.Max(number, int.Parse(m2.Groups[3].Value));
            }
            return $"{m1.Groups[1].Value}{m1.Groups[2].Value}({number + 1}){m1.Groups[4].Value}";
        }

        /// <summary>Folder path of the currently running executable. Same as <seealso cref="AppContext.BaseDirectory"/>.</summary>
        public static string AssemblyDirectory => AppContext.BaseDirectory;

        /// <summary>Folder path of the working directory. Same as <seealso cref="Environment.CurrentDirectory"/>.</summary>
        public static string WorkingDirectory => Environment.CurrentDirectory;

        /// <summary>%username%</summary>
        public static string Username => Environment.UserName;

        /// <summary>C:\Users\%username%</summary>
        public static string UserProfile => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\Desktop</summary>
        public static string DesktopDirectory => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Program Files</summary>
        public static string ProgramFiles => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Program Files (x86)</summary>
        public static string ProgramFilesX86 => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\OneDrive - Bruker Physik GmbH\Documents</summary>
        public static string MyDocuments => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\Pictures</summary>
        public static string MyPictures => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\Music</summary>
        public static string MyMusic => Environment.GetFolderPath(Environment.SpecialFolder.MyMusic, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\Videos</summary>
        public static string MyVideos => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\AppData\Roaming</summary>
        public static string ApplicationData => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\AppData\Local</summary>
        public static string LocalApplicationData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\ProgramData</summary>
        public static string CommonApplicationData => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\WINDOWS\system32</summary>
        public static string System => Environment.GetFolderPath(Environment.SpecialFolder.System, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\WINDOWS\SysWOW64</summary>
        public static string SystemX86 => Environment.GetFolderPath(Environment.SpecialFolder.SystemX86, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup</summary>
        public static string Startup => Environment.GetFolderPath(Environment.SpecialFolder.Startup, Environment.SpecialFolderOption.DoNotVerify);

        /// <summary>C:\Users\%username%\Desktop<br/>Prefer DesktopDirectory instead.</summary>
        public static string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.Desktop, Environment.SpecialFolderOption.DoNotVerify);
    }
}
