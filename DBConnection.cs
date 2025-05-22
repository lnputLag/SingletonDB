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
       
        private static DataBaseConnection _instance;
        private static readonly object _lock = new object();
        private MySqlConnection _connection;
        private string _connectionString;
        private int _activeWindowsCount = 0;
        private DataBaseConnection()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

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

        public void NotifyWindowOpened()
        {
            lock (_lock) 
            {
                _activeWindowsCount++; 
                if (_activeWindowsCount == 1) 
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

        public void NotifyWindowClosed()
        {
            lock (_lock)
            {
                _activeWindowsCount--;
                if (_activeWindowsCount <= 0) 
                {
                    CloseConnection();
                    _activeWindowsCount = 0; 
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
            _connection?.Dispose();
        }


    }
}
