using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;

namespace Lab_25
{
    public partial class AddOrderWindow : Window
    {
        private const string ConnectionString = "Data Source=laundry_service.db;Version=3;";

        public AddOrderWindow()
        {
            InitializeComponent();
            LoadClients();
        }

        private void LoadClients()
        {
            var clients = new List<MainWindow.Client>();
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Clients";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new MainWindow.Client
                        {
                            ClientID = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            PhoneNumber = reader.GetString(2),
                            PhotoURL = reader.GetString(3)
                        });
                    }
                }
            }
            ClientComboBox.ItemsSource = clients;
            ClientComboBox.DisplayMemberPath = "FullName";
            ClientComboBox.SelectedValuePath = "ClientID";
        }

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientComboBox.SelectedValue == null)
            {
                MessageBox.Show("Будь ласка, виберіть клієнта.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int clientId = (int)ClientComboBox.SelectedValue;
            string bookingTime = BookingTimeTextBox.Text;
            string serviceType = ServiceTypeTextBox.Text;
            string orderStatus = OrderStatusTextBox.Text;

            if (string.IsNullOrWhiteSpace(bookingTime) || bookingTime == "Час бронювання")
            {
                MessageBox.Show("Будь ласка, введіть час бронювання.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(serviceType) || serviceType == "Тип послуги")
            {
                MessageBox.Show("Будь ласка, введіть тип послуги.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(orderStatus) || orderStatus == "Статус замовлення")
            {
                MessageBox.Show("Будь ласка, введіть статус замовлення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Orders (ClientID, BookingTime, ServiceType, OrderStatus) 
                                 VALUES (@ClientID, @BookingTime, @ServiceType, @OrderStatus)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClientID", clientId);
                    command.Parameters.AddWithValue("@BookingTime", bookingTime);
                    command.Parameters.AddWithValue("@ServiceType", serviceType);
                    command.Parameters.AddWithValue("@OrderStatus", orderStatus);
                    command.ExecuteNonQuery();
                }
            }

            this.DialogResult = true;
            this.Close();
        }

        private void RemoveText(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text == "Час бронювання" || textBox.Text == "Тип послуги" || textBox.Text == "Статус замовлення")
            {
                textBox.Text = "";
                textBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void AddText(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                if (textBox.Name == "BookingTimeTextBox")
                {
                    textBox.Text = "Час бронювання";
                }
                else if (textBox.Name == "ServiceTypeTextBox")
                {
                    textBox.Text = "Тип послуги";
                }
                else if (textBox.Name == "OrderStatusTextBox")
                {
                    textBox.Text = "Статус замовлення";
                }
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }
}