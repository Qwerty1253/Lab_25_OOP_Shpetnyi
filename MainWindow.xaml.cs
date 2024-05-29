using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Windows;

namespace Lab_25
{
    public partial class MainWindow : Window
    {
        private const string ConnectionString = "Data Source=laundry_service.db;Version=3;";

        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
            LoadClients();
            LoadOrders();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string createClientsTable = @"CREATE TABLE IF NOT EXISTS Clients (
                                                ClientID INTEGER PRIMARY KEY AUTOINCREMENT,
                                                FullName TEXT,
                                                PhoneNumber TEXT,
                                                PhotoURL TEXT)";
                string createOrdersTable = @"CREATE TABLE IF NOT EXISTS Orders (
                                                OrderID INTEGER PRIMARY KEY AUTOINCREMENT,
                                                ClientID INTEGER,
                                                BookingTime TEXT,
                                                ServiceType TEXT,
                                                OrderStatus TEXT,
                                                FOREIGN KEY (ClientID) REFERENCES Clients(ClientID))";
                using (var command = new SQLiteCommand(createClientsTable, connection))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(createOrdersTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void LoadClients()
        {
            var clients = new List<Client>();
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Clients";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            ClientID = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            PhoneNumber = reader.GetString(2),
                            PhotoURL = reader.GetString(3)
                        });
                    }
                }
            }
            ClientsListView.ItemsSource = clients;
        }

        private void LoadOrders()
        {
            var orders = new List<Order>();
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = @"SELECT o.OrderID, o.BookingTime, o.ServiceType, o.OrderStatus, c.FullName 
                                 FROM Orders o 
                                 JOIN Clients c ON o.ClientID = c.ClientID";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new Order
                        {
                            OrderID = reader.GetInt32(0),
                            BookingTime = reader.GetString(1),
                            ServiceType = reader.GetString(2),
                            OrderStatus = reader.GetString(3),
                            ClientName = reader.GetString(4)
                        });
                    }
                }
            }
            OrdersListView.ItemsSource = orders;
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            var addClientWindow = new AddClientWindow();
            if (addClientWindow.ShowDialog() == true)
            {
                LoadClients();
            }
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            var addOrderWindow = new AddOrderWindow();
            if (addOrderWindow.ShowDialog() == true)
            {
                LoadOrders();
            }
        }

        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsListView.SelectedItem is Client selectedClient)
            {
                MessageBoxResult result = MessageBox.Show($"Ви впевнені, що хочете видалити клієнта '{selectedClient.FullName}'?", "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM Clients WHERE ClientID = @ClientID";
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ClientID", selectedClient.ClientID);
                            command.ExecuteNonQuery();
                        }
                    }
                    LoadClients();
                    LoadOrders(); // Обновить заказы, так как могут быть заказы с удаленным клиентом
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть клієнта для видалення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersListView.SelectedItem is Order selectedOrder)
            {
                MessageBoxResult result = MessageBox.Show($"Ви впевнені, що хочете видалити замовлення від '{selectedOrder.ClientName}'?", "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var connection = new SQLiteConnection(ConnectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM Orders WHERE OrderID = @OrderID";
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@OrderID", selectedOrder.OrderID);
                            command.ExecuteNonQuery();
                        }
                    }
                    LoadOrders();
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть замовлення для видалення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class Client
        {
            public int ClientID { get; set; }
            public string FullName { get; set; }
            public string PhoneNumber { get; set; }
            public string PhotoURL { get; set; }
        }

        public class Order
        {
            public int OrderID { get; set; }
            public int ClientID { get; set; }
            public string BookingTime { get; set; }
            public string ServiceType { get; set; }
            public string OrderStatus { get; set; }
            public string ClientName { get; set; }
        }
    }
}