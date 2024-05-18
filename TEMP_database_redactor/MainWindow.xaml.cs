using System;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Data.Common;
using System.Data;
using System.Windows.Forms.VisualStyles;

namespace AdoLibraryApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string? connection;

        public string rbSelected = "";
        public partial class Data
        {
            public List<object> data;
            public Data()
            {
                data = new List<object>();
            }
            public override string ToString()
            {
                string result = "";
                foreach (object s in data)
                {
                    result += s.ToString();
                    int size = 0;
                    if (s is String)
                        size = 35;
                    if (s is Int32)
                        size = 0;
                    if (s is Double)
                        size = 10;
                    if (s is DateTime)
                        size = 20;

                    for (int i = 0; i < ((size - s.ToString().Length) / 2) * 2; i++)
                        result += " .";
                    result += "\t";
                }
                return result;
            }
        }
        ObservableCollection<Data> listSourse;
        public MainWindow()
        {
            InitializeComponent();
            listSourse = new ObservableCollection<Data>();
        }
        private void uploadBDbutton_Click(object sender, RoutedEventArgs e)
        {
            Window1 window1 = new Window1(this);
            if (window1.ShowDialog() != null)
                stackRadioButtons.Children.Clear();

            if (connection != null)
            {
                SqlConnection sqLconnection;
                try { sqLconnection = new SqlConnection(connection); }
                catch (Exception ex) { MessageBox.Show(ex.Message); window1.Close(); return; }

                try { sqLconnection.Open(); }
                catch (Exception ex) { MessageBox.Show(ex.Message); }

                SqlCommand command = sqLconnection.CreateCommand();
                command.CommandText = @"SELECT name FROM sys.tables";

                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            RadioButton rb = new RadioButton();
                            rb.Name = reader[i].ToString();
                            rb.Content = reader[i].ToString();
                            rb.GroupName = "tables";
                            rb.Click += Rb_Click;
                            stackRadioButtons.Children.Add(rb);
                        }
                    }
                }
                reader.Close();

                Char[] symbols = { '=', ';' };
                List<string> temp = new();
                temp = connection.Split(symbols).ToList();
                nameDB.Text = temp[5];
                sqLconnection.Close();

                addData.IsEnabled = false;
                upgradeData.IsEnabled = false;
                deleteData.IsEnabled = false;
                listSourse.Clear();
                redactStack.Children.Clear();

            }
        }
        private void Rb_Click(object sender, RoutedEventArgs e)
        {
            listSourse.Clear();
            redactStack.Children.Clear();

            if (sender is RadioButton rb)
            {
                using (SqlConnection sqLconnection = new SqlConnection(connection))
                {
                    sqLconnection.Open();
                    SqlCommand command = sqLconnection.CreateCommand();
                    command.CommandText = $@"SELECT * FROM {rb.Name}";

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        redactStack.Children.Clear();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            TextBlock textBlock = new TextBlock();
                            textBlock.Text = reader.GetName(i);

                            TextBox textBox = new TextBox();
                            textBox.Name = $"t{i}";
                            textBox.Text = reader.GetName(i);

                            redactStack.Children.Add(textBlock);
                            redactStack.Children.Add(textBox);
                        }
                        string temp = " ";
                        while (reader.Read())
                        {
                            Data d = new Data();
                            for (int i = 0; i < reader.FieldCount; i++)
                            { d.data.Add(reader.GetValue(i)); temp += d.data[i].GetType().ToString(); }
                            listSourse.Add(d);
                        }
                        //MessageBox.Show(temp);
                    }
                    reader.Close();
                }
                rbSelected = rb.Name;
                addData.IsEnabled = true;
                upgradeData.IsEnabled = false;
                deleteData.IsEnabled = false;

            }
            ListOut.ItemsSource = listSourse;
        }
        public void upgradeData_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection sqLconnection = new SqlConnection(connection))
            {
                sqLconnection.Open();
                SqlCommand command = sqLconnection.CreateCommand();

                string n = "";

                if (redactStack.Children[0] is TextBlock t)
                    n = t.Text;
                command.CommandText = @"";
                if (ListOut.SelectedItem is Data data)
                {
                    var m = data.data[0];

                    for (int i = 0; i < data.data.Count; i++)
                    {
                        if (redactStack.Children[i * 2] is TextBlock textBlock)
                        {
                            if (textBlock.Text == "id")
                                continue;

                            if (redactStack.Children[i * 2 + 1] is TextBox textBox)
                                if (data.data[i].ToString() == textBox.Text)
                                    continue;
                                else
                                    command.CommandText += $"UPDATE {rbSelected} SET" +
                                    $" {textBlock.Text} = '{textBox.Text}' WHERE {n} = {m} ";
                        }
                    }
                }
                //MessageBox.Show(command.CommandText);
                try { command.ExecuteNonQuery(); }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
        public void addData_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection sqLconnection = new SqlConnection(connection))
            {
                sqLconnection.Open();

                SqlCommand command = sqLconnection.CreateCommand();

                command.CommandText = @$"INSERT INTO {rbSelected} VALUES (";
                if (ListOut.SelectedItem is Data data)
                {
                    for (int i = 0; i < data.data.Count; i++)
                    {
                        if (redactStack.Children[i * 2] is TextBlock textBlock)
                        {
                            if (textBlock.Text == "id")
                                continue;
                            if (redactStack.Children[i * 2 + 1] is TextBox textBox)
                                command.CommandText += $"'{textBox.Text}',";
                        }
                    }
                }
                command.CommandText = command.CommandText.Remove(command.CommandText.Length - 1);
                command.CommandText += ")";
                // MessageBox.Show(command.CommandText);
                try { command.ExecuteNonQuery(); }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
        public void deleteData_Click(object sender, RoutedEventArgs e)
        {

            using (SqlConnection sqLconnection = new SqlConnection(connection))
            {
                sqLconnection.Open();
                SqlCommand command = sqLconnection.CreateCommand();

                string n = "";

                if (redactStack.Children[0] is TextBlock t)
                    n = t.Text;
                command.CommandText = @"";
                if (ListOut.SelectedItem is Data data)
                {
                    var m = data.data[0];
                    command.CommandText += $"DELETE FROM {rbSelected} WHERE {n} = {m} ";
                }

                //MessageBox.Show(command.CommandText);
                try { command.ExecuteNonQuery(); }
                catch (Exception ex)
                {
                    MessageBox.Show("this data is foreign key for something. " +
                        "Delete that reference firs, then come again >:(\n" + ex.Message);
                }
            }
        }
        private void ListOut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListOut.SelectedItems != null)
            {
                upgradeData.IsEnabled = true;
                deleteData.IsEnabled = true;
            }
            if (ListOut.SelectedItem is Data d)
                for (int i = 0; i < d.data.Count; i++)
                {
                    if (redactStack.Children[i * 2 + 1] is TextBox t)
                        t.Text = d.data[i].ToString();
                }
        }
    }
}

