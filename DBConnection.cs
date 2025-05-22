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
        /// "private" - Защитить данные (например, пароль от БД).Запретить изменение строки после создания (если она вдруг изменится, 
        /// подключение может сломаться).
        /// </summary>
        private string _connectionString;

        private int _activeWindowsCount = 0; // Счётчик активных окон

        // Приватный конструктор
        private DataBaseConnection()
        {
            // Получаем строку подключения из app.config
            /// <summary>
            /// "ConfigurationManager" - Класс из пространства имён System.Configuration, 
            /// который предоставляет доступ к данным конфигурации приложения.
            /// "ConnectionStrings" - Свойство ConfigurationManager, возвращающее коллекцию строк подключения из конфигурации.
            /// "["MySqlConnection"]" - Обращение к конкретной строке подключения по её имени (name="MySqlConnection").
            /// ".ConnectionString" - Свойство, возвращающее саму строку подключения в формате(пример): 
            /// "Server=localhost;Database=TestDB;User=root;Password=12345;"
            /// "Сохранение в _connectionString" - Приватное поле класса, хранящее строку подключения для дальнейшего использования
            /// </summary>
            _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

            // Проверка, что строка подключения не пустая
            /// <summary>
            /// "string.IsNullOrEmpty()" - статический метод, который проверяет Является ли строка null (полностью отсутствует) ИЛИ пустой (равна "").
            /// "InvalidOperationException" -  исключение, которое сигнализирует о недопустимой операции.
            /// "new MySqlConnection()" — конструктор класса MySqlConnection (из библиотеки MySql.Data).
            /// Принимает строку подключения как параметр.Создаётся новый объект подключения в памяти.
            /// Само соединение с БД ещё не открывается (для этого нужно вызвать .Open()). Строка подключения сохраняется внутри объекта.
            /// </summary>
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Ошибка: строка подключения не найдена в app.config.");
            }
            _connection = new MySqlConnection(_connectionString);
        }

        /// <summary>
        /// "public static" - Свойство доступно без создания экземпляра класса и принадлежит самому классу, а не объекту.
        /// "DataBaseConnection" - Тип возвращаемого значения — экземпляр класса DataBaseConnection.
        /// "Instance" - Имя свойства, через которое получают доступ к Singleton.
        /// "lock (_lock)" - Потокобезопасность: блокирует доступ к коду для других потоков на время выполнения блока.
        /// "if (_instance == null)" - Проверяет, создан ли уже экземпляр. Если нет — создаёт новый.
        /// "return _instance;" - Возвращает существующий или только что созданный экземпляр.
        /// </summary>
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
        //Метода выполняет две ключевые задачи:
        // 1. Увеличивает счётчик активных окон (_activeWindowsCount++).
        // 2. Открывает соединение с БД, если это первое активное окно.
        /// <summary>
        /// "lock (_lock)" - Гарантирует, что только один поток может выполнять этот код одновременно.
        /// "if (_activeWindowsCount == 1)" - Условие выполняется только для первого окна. Последующие окна не будут пытаться открыть соединение.
        /// "_connection.State != ConnectionState.Open" - Проверяет, не открыто ли соединение (защита от повторного открытия).
        /// </summary>
        public void NotifyWindowOpened()
        {
            lock (_lock) // Блокировка для потокобезопасности
            {
                _activeWindowsCount++; // Увеличение счётчика окон
                if (_activeWindowsCount == 1) // Первое окно — открываем соединение
                {
                    try
                    {
                        if (_connection.State != ConnectionState.Open) // Проверка состояния
                            _connection.Open(); // Открытие соединения
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

        //Метод Dispose() реализует интерфейс IDisposable и выполняет:
        // 1. Закрытие соединения с БД (если оно открыто).
        // 2. Освобождение неуправляемых ресурсов, связанных с подключением.
        /// <summary>
        ///  "_connection?.Dispose();" - ? (null-conditional operator) — проверяет, что _connection не null.
        ///  Если _connection существует, вызывает его метод Dispose().
        /// </summary>
        public void Dispose()
        {
            _connection?.Dispose(); // Dispose() сам закроет соединение
        }


    }
}
