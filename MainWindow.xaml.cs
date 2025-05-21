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
    public partial class MainWindow : DatabaseWindow // Наследуемся от DatabaseWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            // Теперь здесь не нужно прописывать логику БД — она уже в базовом классе!
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NewWindow newWindow = new NewWindow();
            newWindow.Show();
        }
    }
}
