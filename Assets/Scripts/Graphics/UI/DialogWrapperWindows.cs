using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Ookii.Dialogs.WinForms;

namespace DLS.Graphics {
    public class DialogWrapperWindows : IDialogWrapper
    {
        public class WindowWrapper : IWin32Window {
            private IntPtr _hwnd;
            public WindowWrapper(IntPtr handle) { _hwnd = handle; }
            public IntPtr Handle { get { return _hwnd; } }
        }
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        public void ShowDialog(string title, string description, string cancelButton)
        {
            new InputDialog {
                WindowTitle = title,
                Content = description,
                Input = cancelButton
            }.ShowDialog(new WindowWrapper(GetActiveWindow()));
        }

        public void ShowDialog(string title, string description, string cancelButton, string okButton)
        {
            throw new NotImplementedException();
        }
    }
}
