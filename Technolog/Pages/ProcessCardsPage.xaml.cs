using System;
using System.Windows;
using System.Windows.Controls;
using Technolog.Models;
using Technolog.Services;

namespace Technolog.Pages
{
    public partial class ProcessCardsPage : Page
    {
        private ApiService apiService = new ApiService();

        public ProcessCardsPage()
        {
            InitializeComponent();
        }

        private async void ProcessCardsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCards();
        }

        private async System.Threading.Tasks.Task LoadCards()
        {
            try
            {
                var cards = await apiService.GetProcessCards();
                dgCards.ItemsSource = cards;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            txtProductId.Text = "";
            txtRecipeId.Text = "";
            txtVersion.Text = "1";
            txtAddError.Text = "";
            gridAdd.Visibility = Visibility.Visible;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtProductId.Text) || string.IsNullOrEmpty(txtRecipeId.Text))
            {
                txtAddError.Text = "Заполните ID продукта и рецептуры";
                return;
            }

            try
            {
                var model = new ProcessCardModel
                {
                    product_id = int.Parse(txtProductId.Text),
                    recipe_id = int.Parse(txtRecipeId.Text),
                    version = int.Parse(txtVersion.Text)
                };

                await apiService.CreateProcessCard(model);
                gridAdd.Visibility = Visibility.Collapsed;
                await LoadCards();
            }
            catch (Exception ex)
            {
                txtAddError.Text = ex.Message;
            }
        }

        private async void btnApprove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int cardId)
            {
                try
                {
                    await apiService.ApproveProcessCard(cardId);
                    await LoadCards();
                    MessageBox.Show("Техкарта утверждена");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            gridAdd.Visibility = Visibility.Collapsed;
        }
    }
}