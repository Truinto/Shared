using Shared.StringsNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Shared
{
    public partial class CommandTool : IDisposable
    {
        public delegate void OnKeyPressDelegate(object sender, ConsoleKeyInfo key, out bool consumed);

        public object? Parent;
        public string FilePath;
        public string? Args;
        public IEnumerable<string>? ArgsList;
        public Action<string>? OnStandard;
        public Action<string>? OnError;
        public OnKeyPressDelegate? OnKeyPress;
        public readonly StringBuilder Sb_Output = new();
        public readonly StringBuilder Sb_Error = new();
        public int ExitCode = -1;
        public Process? Process = new();
        public bool EatCancel;
        public ProcessPriorityClass Priority = ProcessPriorityClass.Normal;
        private volatile ProcessState _state;

        public CommandTool()
        {
            _state = ProcessState.Uninitialized;
            this.FilePath ??= "";
#if WINFORMS
            if (!Application.MessageLoop) _handler = new EventHandler(Handler);
#else
            _handler = new EventHandler(Handler);
#endif
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
            Disposing();
        }

        public void Dispose()
        {
            Disposing();
            GC.SuppressFinalize(this);
        }

        private void Disposing()
        {
            try
            {
                Debug.WriteLine($"disposing {this.FilePath}");
                SetConsoleCtrlHandlerCall(_handler, false);
                if (State is ProcessState.Running)
                    Process?.Kill(true);
                Process = null;
                _state = ProcessState.Exited;
            } catch (Exception) { }
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
            if (Process == null)
                throw new InvalidOperationException("Process already finished");

            if (this.ArgsList is null)
                Debug.WriteLine($"run-command {this.FilePath} {this.Args}");
            else
                Debug.WriteLine($"run-command {this.FilePath} {this.ArgsList.JoinArgs()}");

            SetConsoleCtrlHandlerCall(_handler, true);

            Process.StartInfo = new()
            {
                FileName = this.FilePath,
                Arguments = this.Args,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            if (ArgsList != null)
                foreach (var arg in this.ArgsList)
                    Process.StartInfo.ArgumentList.Add(arg);

            Process.EnableRaisingEvents = true;
            Process.OutputDataReceived += (sender, args) =>
            {
                if (args?.Data is null)
                    return;
                lock (Sb_Output)
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
                lock (Sb_Output)
                {
                    OnError?.Invoke(args.Data);
                    Debug.WriteLine(args.Data);
                    Sb_Error.AppendLine(args.Data);
                }
            };

            Process.Start();
            _state = ProcessState.Running;

            Process.PriorityClass = Priority;
            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();

            if (OnKeyPress != null)
            {
                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        Debug.WriteLine($"key-press {key.Modifiers} {key.Key}");
                        OnKeyPress(this, key, out bool consumed);
                        if (!consumed)
                            Process?.StandardInput.Write(key.KeyChar);
                        continue;
                    }
                    if (Process?.WaitForExit(200) != false)
                        break;
                }
            }

            WaitForExit();

            return ExitCode;
        }

        /// <summary>
        /// Asynchronous process execution. Run <see cref="WaitForExit"/> or read until <see cref="State"/> is <see cref="ProcessState.Exited"/>.
        /// </summary>
        public void Start()
        {
            if (Process == null)
                throw new InvalidOperationException("Process already finished");

            if (this.ArgsList is null)
                Debug.WriteLine($"run-command {this.FilePath} {this.Args}");
            else
                Debug.WriteLine($"run-command {this.FilePath} {this.ArgsList.JoinArgs()}");

            SetConsoleCtrlHandlerCall(_handler, true);

            Process.StartInfo = new()
            {
                FileName = this.FilePath,
                Arguments = this.Args,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            if (ArgsList != null)
                foreach (var arg in this.ArgsList)
                    Process.StartInfo.ArgumentList.Add(arg);

            Process.EnableRaisingEvents = true;
            Process.OutputDataReceived += (sender, args) =>
            {
                if (args?.Data is null)
                    return;
                lock (Sb_Output)
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
                lock (Sb_Output)
                {
                    OnError?.Invoke(args.Data);
                    Debug.WriteLine(args.Data);
                    Sb_Error.AppendLine(args.Data);
                }
            };
            Process.Exited += (sender, args) => { _state = ProcessState.Exited; };

            Process.Start();
            _state = ProcessState.Running;

            Process.PriorityClass = Priority;
            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();
        }

        /// <summary>
        /// Check process state. Flushes pipes once state is <see cref="ProcessState.Exited"/>.
        /// </summary>
        public ProcessState State
        {
            get
            {
                if (_state == ProcessState.Exited && Process != null)
                    WaitForExit();
                return _state;
            }
        }

        /// <summary>
        /// Waits for asynchronous to finish.
        /// </summary>
        public void WaitForExit()
        {
            Process?.WaitForExit();
            ExitCode = Process?.ExitCode ?? -1;
            Process = null;
            _state = ProcessState.Exited;

            SetConsoleCtrlHandlerCall(_handler, false);
        }

        #region Kernel32

        private static bool SetConsoleCtrlHandlerCall(EventHandler? handler, bool add)
        {
            if (handler != null && OperatingSystem.IsWindows())
                return SetConsoleCtrlHandler(handler, add);
            return false;
        }

        [LibraryImport("Kernel32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleCtrlHandler(EventHandler handler, [MarshalAs(UnmanagedType.Bool)] bool add);

        private delegate bool EventHandler(CtrlType sig);
        private EventHandler? _handler;

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
                Debug.WriteLine("trigger command exit handle");
                Disposing();
            } catch (Exception) { }
            return EatCancel;
        }
        #endregion

        [GeneratedRegex(@"[\\^]\n")]
        private static partial Regex Rx_CmdEOL();
    }

    public enum ProcessState
    {
        Invalid,
        Uninitialized,
        Running,
        Exited,
    }
}
