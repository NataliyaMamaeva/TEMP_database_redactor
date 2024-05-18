using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;

namespace AdoLibraryApp
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        MainWindow mainWindow;
        public Window1(MainWindow w)
        {
            InitializeComponent();
            mainWindow = w;
        }

        private void ButtonStringUpload_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.connection = conString.Text;
            DialogResult = true;
        }


    }
}