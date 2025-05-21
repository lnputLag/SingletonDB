using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SingletonDB
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                var db = DataBaseConnection.Instance;
                var connection = db.GetConnection();
                MessageBox.Show("Подключение к базе данных MySQL успешно установлено.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        //Закрытие соединения
        protected override void OnClosed(EventArgs e)
        {
            DataBaseConnection.Instance.CloseConnection(); 
            base.OnClosed(e);
        }
    }
}
