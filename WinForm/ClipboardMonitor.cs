using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

//source: https://stackoverflow.com/a/1394225

namespace Shared
{
    [DefaultEvent("ClipboardChanged")]
    public partial class ClipboardMonitor : Control
    {
        private IntPtr _NextClipboardViewer;
        private DateTime _LastChange;

        /// <summary>Clipboard contents changed.</summary>
        public event EventHandler<ClipboardChangedEventArgs>? ClipboardChanged;

        public ClipboardMonitor()
        {
            this.BackColor = Color.Red;
            this.Visible = false;

            _NextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.Handle);
        }

        protected override void Dispose(bool disposing)
        {
            ChangeClipboardChain(this.Handle, _NextClipboardViewer);
            base.Dispose(disposing);
        }

        [LibraryImport("user32.dll")]
        private static partial int SetClipboardViewer(int hWndNewViewer);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [LibraryImport("user32.dll", EntryPoint = "SendMessageA")]
        private static partial int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [LibraryImport("user32.dll")]
        private static partial IntPtr GetClipboardOwner();

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // defined in winuser.h
            const int wm_drawclipboard = 0x308;
            const int wm_changecbchain = 0x30D;

            switch (m.Msg)
            {
                case wm_drawclipboard:
                    OnClipboardChanged();
                    _ = SendMessage(_NextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                case wm_changecbchain:
                    if (m.WParam == _NextClipboardViewer)
                        _NextClipboardViewer = m.LParam;
                    else
                        _ = SendMessage(_NextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        void OnClipboardChanged()
        {
            try
            {
                var now = DateTime.Now;
                if (now < _LastChange.AddMilliseconds(200))
                    return;
                _LastChange = now;
                var iData = Clipboard.GetDataObject();
                if (iData is not null)
                    ClipboardChanged?.Invoke(this, new ClipboardChangedEventArgs(iData, GetClipboardOwner()));
            } catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                //MessageBox.Show(e.ToString());
            }
        }
    }

    public class ClipboardChangedEventArgs : EventArgs
    {
        public readonly IDataObject DataObject;
        public readonly IntPtr Owner;

        public ClipboardChangedEventArgs(IDataObject dataObject, IntPtr owner)
        {
            this.DataObject = dataObject;
            this.Owner = owner;
        }
    }
}
