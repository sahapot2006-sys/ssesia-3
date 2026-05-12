using System;
using System.Windows;
using Technolog.Services;

namespace Technolog
{
    public partial class LoginWindow : Window
    {
        private ApiService apiService = new ApiService();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                txtError.Text = "Введите логин и пароль";
                return;
            }

            btnLogin.IsEnabled = false;
            txtError.Text = "";

            try
            {
                var user = await apiService.Login(login, password);

                if (user.role != "Technologist")
                {
                    txtError.Text = "Доступ только для технологов";
                    return;
                }

                MainWindow main = new MainWindow(user);
                main.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                txtError.Text = ex.Message;
            }
            finally
            {
                btnLogin.IsEnabled = true;
            }
        }

        // ЭТОТ МЕТОД НУЖНО ДОБАВИТЬ!
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }
    }
}