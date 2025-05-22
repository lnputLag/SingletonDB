using System;
using System.Windows;

namespace SingletonDB
{
    public class DatabaseWindow : Window
    {
        public DatabaseWindow()
        {
            DataBaseConnection.Instance.NotifyWindowOpened();
        }

        protected override void OnClosed(EventArgs e)
        {
            DataBaseConnection.Instance.NotifyWindowClosed();
            base.OnClosed(e);
        }
    }
}
