using System;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Lab_25
{
    public partial class AddClientWindow : Window
    {
        private const string ConnectionString = "Data Source=laundry_service.db;Version=3;";

        public AddClientWindow()
        {
            InitializeComponent();
        }

        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text;
            string phoneNumber = PhoneNumberTextBox.Text;
            string photoURL = PhotoURLTextBox.Text;

            if (string.IsNullOrWhiteSpace(fullName) || fullName == "Повне ім'я")
            {
                MessageBox.Show("Будь ласка, введіть повне ім'я клієнта.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsValidPhoneNumber(phoneNumber))
            {
                MessageBox.Show("Номер телефону може складатися тільки з цифр, знаків +, -, ( ).", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string cleanPhoneNumber = Regex.Replace(phoneNumber, @"[^\d]", "");

            if (IsPhoneNumberDuplicate(cleanPhoneNumber))
            {
                MessageBox.Show("Номер телефону вже існує.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (IsClientExists(fullName))
            {
                MessageBoxResult result = MessageBox.Show($"Клієнт з ім'ям '{fullName}' вже існує. Ви хочете додати клієнта?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Clients (FullName, PhoneNumber, PhotoURL) 
                                 VALUES (@FullName, @PhoneNumber, @PhotoURL)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FullName", fullName);
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    command.Parameters.AddWithValue("@PhotoURL", photoURL);
                    command.ExecuteNonQuery();
                }
            }

            this.DialogResult = true;
            this.Close();
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            string pattern = @"^[\d\+\-\(\)\s]+$";
            return Regex.IsMatch(phoneNumber, pattern);
        }

        private bool IsPhoneNumberDuplicate(string phoneNumber)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Clients WHERE REPLACE(REPLACE(REPLACE(REPLACE(PhoneNumber, '+', ''), '-', ''), '(', ''), ')', '') = @PhoneNumber";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    long count = (long)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private bool IsClientExists(string fullName)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Clients WHERE FullName = @FullName";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FullName", fullName);
                    long count = (long)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private void RemoveText(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text == "Повне ім'я" || textBox.Text == "Номер телефону" || textBox.Text == "Посилання на фото")
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
                if (textBox.Name == "FullNameTextBox")
                {
                    textBox.Text = "Повне ім'я";
                }
                else if (textBox.Name == "PhoneNumberTextBox")
                {
                    textBox.Text = "Номер телефону";
                }
                else if (textBox.Name == "PhotoURLTextBox")
                {
                    textBox.Text = "Посилання на фото";
                }
                textBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void ChoosePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Файли зображень (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                PhotoURLTextBox.Text = openFileDialog.FileName;
                PhotoURLTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }
    }
}