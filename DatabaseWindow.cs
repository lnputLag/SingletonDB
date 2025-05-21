using System;
using System.Windows;

namespace SingletonDB
{
    public class DatabaseWindow : Window
    {
        public DatabaseWindow()
        {
            // Автоматически регистрируем окно в Singleton
            DataBaseConnection.Instance.NotifyWindowOpened();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Автоматически уведомляем Singleton о закрытии
            DataBaseConnection.Instance.NotifyWindowClosed();
            base.OnClosed(e);
        }
    }
}