namespace DLS.Graphics {
    public interface IDialogWrapper {
        public void ShowDialog(string title, string description, string cancelButton);
        public void ShowDialog(string title, string description, string cancelButton, string okButton);
    }
}