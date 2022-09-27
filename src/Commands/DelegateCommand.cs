using System.Windows.Input;

namespace ArtemisBlog.Commands
{
    class DelegateCommand : ICommand
    {
        private readonly Action action;
        private bool executeEnabled;

        internal DelegateCommand(Action action)
        {
            this.action = action;
            executeEnabled = true;
        }

        internal bool ExecuteEnabled
        {
            get { return executeEnabled; }
            set
            {
                if (executeEnabled != value)
                {
                    executeEnabled = value;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        #region · ICommand ·
        public bool CanExecute(object parameter)
        {
            return executeEnabled;
        }

        public void Execute(object parameter)
        {
            action();
        }

        public event EventHandler CanExecuteChanged;
        #endregion
    }
}
