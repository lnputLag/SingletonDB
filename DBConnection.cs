using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SingletonDB
{
    public sealed class DataBaseConnection : IDisposable
    {
        /// <summary>
        ///  Статическое поле, которое хранит единственный экземпляр класса "DataBaseConnection"
        ///  Благодаря static, поле принадлежит не конкретному объекту, а всему классу.
        ///  Модификатор доступа "private" для запрета прямого доступа извне класса (инкапсуляция).
        ///  Экземпляр можно получить только через метод GetInstance(). 
        ///  Пример: единственный ключ от сейфа, который хранится в определённом месте.
        /// </summary>
        private static DataBaseConnection _instance;

        /// <summary>
        /// Это объект-заглушка (lock-object), используемый для синхронизации потоков в многопоточной среде.
        /// "readonly" Чтобы гарантировать, что объект блокировки нельзя случайно заменить после создания (это важно для корректной работы lock).
        /// "lock" Без него два потока могут одновременно проверить if (_instance == null) и создать два экземпляра, что нарушит принцип Singleton.
        /// "lock" гарантирует, что проверка и создание пройдут атомарно.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Поле для хранения подключения к MySQL. Оно нестатическое, потому что относится к конкретному экземпляру DataBaseConnection.
        /// "private" Чтобы внешний код не мог напрямую управлять соединением (например, случайно закрыть его). 
        /// Доступ — только через методы класса(GetConnection(), CloseConnection()).
        /// </summary>
        private MySqlConnection _connection;

        /// <summary>
        /// Хранит строку подключения к БД (логин, пароль, адрес сервера и т. д.). Инициализируется один раз в конструкторе.
        /// "private" - Защитить данные (например, пароль от БД).Запретить изменение строки после создания (если она вдруг изменится, подключение может сломаться).
        /// </summary>
        private string _connectionString;

        private int _activeWindowsCount = 0; // Счётчик активных окон

        // Приватный конструктор
        private DataBaseConnection()
        {
            // Получаем строку подключения из app.config
            _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            // Проверка, что строка подключения не пустая
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Ошибка: строка подключения не найдена в app.config.");
            }

            _connection = new MySqlConnection(_connectionString);
        }

        public static DataBaseConnection Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DataBaseConnection();
                    }
                    return _instance;
                }
            }
        }

        // Метод для открытия соединения (вызывается при создании окна)
        public void NotifyWindowOpened()
        {
            lock (_lock)
            {
                _activeWindowsCount++;
                if (_activeWindowsCount == 1) // Первое окно — открываем соединение
                {
                    try
                    {
                        if (_connection.State != ConnectionState.Open)
                            _connection.Open();
                        MessageBox.Show("Подключение к базе данных MySQL успешно установлено.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка подключения: {ex.Message}");
                    }
                }
            }
        }

        // Метод для закрытия соединения (вызывается при закрытии окна)
        public void NotifyWindowClosed()
        {
            lock (_lock)
            {
                _activeWindowsCount--;
                if (_activeWindowsCount <= 0) // Все окна закрыты
                {
                    CloseConnection();
                    _activeWindowsCount = 0; // Сброс счётчика
                }
            }
        }

        public MySqlConnection GetConnection()
        {
            lock (_lock)
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();
                return _connection;
            }
        }

        private void CloseConnection()
        {
            if (_connection?.State == ConnectionState.Open)
                _connection.Close();
        }

        public void Dispose()
        {
            CloseConnection();
            _connection?.Dispose();
        }


    }
}
