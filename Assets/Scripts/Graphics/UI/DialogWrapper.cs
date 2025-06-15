namespace DLS.Graphics {
    class DialogWrapper {
        static IDialogWrapper dialogWrapper;
        static DialogWrapper() {
            #if UNITY_STANDALONE_WIN
                dialogWrapper = new DialogWrapperWindows();
            #endif
        }

        public static void ShowDialog(string title, string description, string cancelButton) => 
            dialogWrapper.ShowDialog(title, description, cancelButton);
        public static void ShowDialog(string title, string description, string cancelButton, string okButton) =>
            dialogWrapper.ShowDialog(title, description, cancelButton, okButton);
    }
}