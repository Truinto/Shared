﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shared
{
    /// <summary>
    /// Class to register global hotkey. Works only if thread messages are processed (e.g. WindowsForms).
    /// </summary>
    public class GlobalHotKeyUI : IMessageFilter, IDisposable
    {
        /// <summary>
        /// Message identifier 0x312 means that the mesage is a WM_HOTKEY message.
        /// </summary>
        const int WM_HOTKEY = 786;

        private bool disposed = false;
        /// <summary>
        /// A normal application can use any value between 0x0000 and 0xBFFF as the ID
        /// but if you are writing a DLL, then you must use GlobalAddAtom to get a
        /// unique identifier for your hot key.
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// A handle to the window that will receive WM_HOTKEY messages generated by the hot key.
        /// </summary>
        public readonly IntPtr Handle;
        public readonly Keys Key;
        public readonly KeyModifiers Modifiers;

        /// <summary>
        /// Raise an event when the hotkey is pressed.
        /// </summary>
        public event EventHandler HotKeyPressed;

        public bool IsRegistered { get; private set; }

        public GlobalHotKeyUI(IntPtr handle, int id, KeyModifiers modifiers, Keys key)
        {
            if (key == Keys.None)
                throw new ArgumentException("The key can not be None.");

            this.Handle = handle;
            this.Id = id & 0x3FFF;
            this.Modifiers = modifiers;
            this.Key = key;

            this.IsRegistered = RegisterHotKey(Handle, Id, Modifiers, Key);

            // Adds a message filter to monitor Windows messages as they are routed to their destinations.
            Application.AddMessageFilter(this);
        }

        /// <summary>
        /// Filters out a message before it is dispatched.
        /// </summary>
        public bool PreFilterMessage(ref Message m)
        {
            // The property WParam of Message is typically used to store small pieces
            // of information. In this scenario, it stores the ID.
            if (m.Msg == WM_HOTKEY
                && m.HWnd == this.Handle
                && m.WParam == (IntPtr)this.Id
                && HotKeyPressed != null)
            {
                // Raise the HotKeyPressed event if it is an WM_HOTKEY message.
                HotKeyPressed(this, EventArgs.Empty);

                // True to filter the message and stop it from being dispatched.
                return true;
            }

            // Return false to allow the message to continue to the next filter or control.
            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Application.RemoveMessageFilter(this);
                UnregisterHotKey(Handle, Id);
                IsRegistered = false;
            }

            disposed = true;
        }

        /// <summary>
        /// Define a system-wide hot key.
        /// </summary>
        /// <param name="hWnd">
        /// A handle to the window that will receive WM_HOTKEY messages generated by the
        /// hot key. If this parameter is NULL, WM_HOTKEY messages are posted to the
        /// message queue of the calling thread and must be processed in the message loop.
        /// </param>
        /// <param name="id">
        /// The identifier of the hot key. If the hWnd parameter is NULL, then the hot
        /// key is associated with the current thread rather than with a particular
        /// window.
        /// </param>
        /// <param name="fsModifiers">
        /// The keys that must be pressed in combination with the key specified by the
        /// uVirtKey parameter in order to generate the WM_HOTKEY message. The fsModifiers
        /// parameter can be a combination of the following values.
        /// MOD_ALT     0x0001
        /// MOD_CONTROL 0x0002
        /// MOD_SHIFT   0x0004
        /// MOD_WIN     0x0008
        /// </param>
        /// <param name="vk">The virtual-key code of the hot key.</param>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private extern static bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk);

        /// <summary>
        /// Frees a hot key previously registered by the calling thread.
        /// </summary>
        /// <param name="hWnd">
        /// A handle to the window associated with the hot key to be freed. This parameter
        /// should be NULL if the hot key is not associated with a window.
        /// </param>
        /// <param name="id">
        /// The identifier of the hot key to be freed.
        /// </param>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private extern static bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
