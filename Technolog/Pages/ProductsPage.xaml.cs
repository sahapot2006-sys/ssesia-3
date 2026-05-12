using System;
using System.Windows;
using System.Windows.Controls;
using Technolog.Models;
using Technolog.Services;

namespace Technolog.Pages
{
    public partial class ProductsPage : Page
    {
        private ApiService apiService = new ApiService();

        public ProductsPage()
        {
            InitializeComponent();
        }

        private async void ProductsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProducts();
        }

        private async System.Threading.Tasks.Task LoadProducts()
        {
            try
            {
                var products = await apiService.GetProducts();
                dgProducts.ItemsSource = products;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            txtCode.Text = "";
            txtName.Text = "";
            txtType.Text = "";
            txtForm.Text = "";
            txtAddError.Text = "";
            gridAdd.Visibility = Visibility.Visible;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtCode.Text) || string.IsNullOrEmpty(txtName.Text))
            {
                txtAddError.Text = "Заполните код и название";
                return;
            }

            btnSave.IsEnabled = false;
            txtAddError.Text = "";

            try
            {
                var product = new Product
                {
                    product_code = txtCode.Text.Trim(),
                    name = txtName.Text.Trim(),
                    type = txtType.Text.Trim(),
                    release_form = txtForm.Text.Trim(),
                    status = "Active"
                };

                var result = await apiService.CreateProduct(product);

                if (result != null)
                {
                    MessageBox.Show("Продукт успешно добавлен!", "Успех",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                    gridAdd.Visibility = Visibility.Collapsed;
                    await LoadProducts();
                }
                else
                {
                    txtAddError.Text = "Ошибка при добавлении продукта";
                }
            }
            catch (Exception ex)
            {
                txtAddError.Text = ex.Message;
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnSave.IsEnabled = true;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            gridAdd.Visibility = Visibility.Collapsed;
        }
    }
}