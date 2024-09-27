using Shared.StringsNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoCompress
{
    public partial class CommandTool
    {
        public delegate void OnKeyPressDelegate(object sender, ConsoleKeyInfo key, out bool consumed);

        public string FilePath;
        public string? Args;
        public IEnumerable<string>? ArgsList;
        public Action<string>? OnStandard;
        public Action<string>? OnError;
        public OnKeyPressDelegate? OnKeyPress;
        public readonly StringBuilder Sb_Output = new();
        public readonly StringBuilder Sb_Error = new();
        public int ExitCode;
        public Process? Process;
        public bool EatCancel;

        public CommandTool()
        {
            this.FilePath ??= "";
            _handler = new EventHandler(Handler);
        }

        public CommandTool(string filePath, string args, Action<string>? onStandard = null, Action<string>? onError = null, OnKeyPressDelegate? onKeyPress = null) : this()
        {
            this.FilePath = filePath;
            this.Args = Rx_CmdEOL().Replace(args, "");
            this.OnStandard = onStandard;
            this.OnError = onError;
            this.OnKeyPress = onKeyPress;
        }

        public CommandTool(string filePath, IEnumerable<string> argsList, Action<string>? onStandard = null, Action<string>? onError = null, OnKeyPressDelegate? onKeyPress = null) : this()
        {
            this.FilePath = filePath;
            this.ArgsList = argsList;
            this.OnStandard = onStandard;
            this.OnError = onError;
            this.OnKeyPress = onKeyPress;
        }

        ~CommandTool()
        {
            SetConsoleCtrlHandler(_handler, false);
        }

        /// <summary>
        /// Synchronous process execution. Redirects input. Returns exit code and Standard/Error strings.
        /// </summary>
        public int Execute(out string output, out string error)
        {
            Execute();
            output = Sb_Output.ToString();
            error = Sb_Error.ToString();
            return ExitCode;
        }

        /// <summary>
        /// Synchronous process execution. Redirects input. Returns exit code.
        /// </summary>
        public int Execute()
        {
            if (this.ArgsList is null)
                Trace.WriteLine($"run-command {this.FilePath} {this.Args}");
            else
                Trace.WriteLine($"run-command {this.FilePath} {this.ArgsList.JoinArgs()}");

            SetConsoleCtrlHandler(_handler, true);

            ExitCode = -1;
            try
            {
                Sb_Output.Clear();
                Sb_Error.Clear();

                Process = new Process();
                Process.StartInfo = new()
                {
                    FileName = this.FilePath,
                    Arguments = this.Args,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    //StandardOutputEncoding = Encoding.UTF8,
                    //StandardErrorEncoding = Encoding.UTF8,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                foreach (var arg in this.ArgsList ?? [])
                    Process.StartInfo.ArgumentList.Add(arg);

                Process.EnableRaisingEvents = true;
                Process.OutputDataReceived += (sender, args) =>
                {
                    if (args?.Data is null)
                        return;
                    lock (this)
                    {
                        OnStandard?.Invoke(args.Data);
                        Debug.WriteLine(args.Data);
                        Sb_Output.AppendLine(args.Data);
                    }
                };
                Process.ErrorDataReceived += (sender, args) =>
                {
                    if (args?.Data is null)
                        return;
                    lock (this)
                    {
                        OnError?.Invoke(args.Data);
                        Debug.WriteLine(args.Data);
                        Sb_Error.AppendLine(args.Data);
                    }
                };

                Process.Start();
                Process.BeginOutputReadLine();
                Process.BeginErrorReadLine();
                Process.PriorityClass = ProcessPriorityClass.BelowNormal;

                while (true)
                {
                    if (OnKeyPress != null && Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        Trace.WriteLine($"key-press {key.Modifiers} {key.Key}");
                        OnKeyPress(this, key, out bool consumed);
                        if (!consumed)
                            Process?.StandardInput.Write(key.KeyChar);
                        continue;
                    }

                    //Thread.Sleep(200);
                    //if (_process.HasExited)
                    //    break;

                    if (Process?.WaitForExit(200) != false)
                        break;
                }

                Process?.WaitForExit(); // do not remove this!
                ExitCode = Process?.ExitCode ?? -1;
                Process = null;
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }

            SetConsoleCtrlHandler(_handler, false);

            return ExitCode;
        }

        /// <summary>
        /// Asynchronous process execution. Run <see cref="WaitForExit"/>.
        /// </summary>
        public void Start()
        {
            if (this.ArgsList is null)
                Trace.WriteLine($"run-command {this.FilePath} {this.Args}");
            else
                Trace.WriteLine($"run-command {this.FilePath} {this.ArgsList.JoinArgs()}");

            SetConsoleCtrlHandler(_handler, true);

            ExitCode = -1;
            try
            {
                var sb_standard = new StringBuilder();
                var sb_error = new StringBuilder();

                Process = new Process();
                Process.StartInfo = new()
                {
                    FileName = this.FilePath,
                    Arguments = this.Args,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    //StandardOutputEncoding = Encoding.UTF8,
                    //StandardErrorEncoding = Encoding.UTF8,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                Process.EnableRaisingEvents = true;
                Process.OutputDataReceived += (sender, args) =>
                {
                    if (args?.Data is null)
                        return;
                    lock (this)
                    {
                        OnStandard?.Invoke(args.Data);
                        Debug.WriteLine(args.Data);
                        sb_standard.AppendLine(args.Data);
                    }
                };
                Process.ErrorDataReceived += (sender, args) =>
                {
                    if (args?.Data is null)
                        return;
                    lock (this)
                    {
                        OnError?.Invoke(args.Data);
                        Debug.WriteLine(args.Data);
                        sb_error.AppendLine(args.Data);
                    }
                };

                Process.Start();
                Process.BeginOutputReadLine();
                Process.BeginErrorReadLine();
                Process.PriorityClass = ProcessPriorityClass.BelowNormal;
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }
        }

        /// <summary>
        /// Asynchronous check if process is finished. If true calls <see cref="WaitForExit"/>.
        /// </summary>
        public bool HasExited()
        {
            bool hasExited = Process?.HasExited ?? true;

            if (hasExited)
                WaitForExit(); // this makes sure the streams are flushed

            return hasExited;
        }

        /// <summary>
        /// Waits for asynchronous to finish.
        /// </summary>
        public void WaitForExit()
        {
            Process?.WaitForExit();
            ExitCode = Process?.ExitCode ?? -1;
            Process = null;

            SetConsoleCtrlHandler(_handler, false);
        }

        #region Kernel32
        [LibraryImport("Kernel32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleCtrlHandler(EventHandler handler, [MarshalAs(UnmanagedType.Bool)] bool add);

        private delegate bool EventHandler(CtrlType sig);
        private EventHandler _handler;

        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private bool Handler(CtrlType sig)
        {
            try
            {
                Trace.WriteLine("trigger command exit handle");
                Process?.Kill();
            }
            catch (Exception) { }
            return EatCancel;
        }
        #endregion

        [GeneratedRegex(@"[\\^]\n")]
        private static partial Regex Rx_CmdEOL();
    }
}
