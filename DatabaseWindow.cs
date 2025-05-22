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

//DatabaseWindow решает две задачи:
// 1. Регистрирует окно при создании (NotifyWindowOpened()).
// 2. Уведомляет о закрытии (NotifyWindowClosed()).

/// <summary>
/// "Конструктор (DatabaseWindow())" - При создании любого окна (наследника DatabaseWindow):
/// - Обращается к Singleton-подключению (DataBaseConnection.Instance).
/// - Вызывает NotifyWindowOpened().
/// "Метод NotifyWindowOpened() (в DataBaseConnection):"
/// - Увеличивает счётчик окон (_activeWindowsCount++).
/// - Если это первое окно — открывает соединение с БД.
/// </summary>


//  Метод OnClosed()
/// <summary>
/// "При закрытии окна:"
/// - Уведомляет DataBaseConnection через NotifyWindowClosed().
/// - Вызывает базовый метод OnClosed() (важно для корректной работы WPF).
/// "Метод NotifyWindowClosed() (в DataBaseConnection):"
/// - Уменьшает счётчик окон (_activeWindowsCount--).
/// - Если окон не осталось (_activeWindowsCount == 0) — закрывает соединение с БД.
/// </summary>
