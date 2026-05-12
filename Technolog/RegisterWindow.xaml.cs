using System;
using System.Windows;
using Technolog.Models;
using Technolog.Services;

namespace Technolog
{
    public partial class RegisterWindow : Window
    {
        private ApiService apiService = new ApiService();

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;
            string fullName = txtFullName.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullName))
            {
                txtError.Text = "Заполните обязательные поля";
                return;
            }

            btnRegister.IsEnabled = false;
            txtError.Text = "";

            try
            {
                var model = new RegisterModel
                {
                    login = login,
                    password = password,
                    full_name = fullName,
                    role = "Technologist",
                    email = txtEmail.Text.Trim(),
                    phone = txtPhone.Text.Trim(),
                    department = txtDepartment.Text.Trim()
                };

                await apiService.Register(model);

                MessageBox.Show("Регистрация прошла успешно!", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                txtError.Text = ex.Message;
            }
            finally
            {
                btnRegister.IsEnabled = true;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}