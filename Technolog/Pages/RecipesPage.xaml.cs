using System;
using System.Windows;
using System.Windows.Controls;
using Technolog.Models;
using Technolog.Services;

namespace Technolog.Pages
{
    public partial class RecipesPage : Page
    {
        private ApiService apiService = new ApiService();
        private int selectedRecipeId = 0;

        public RecipesPage()
        {
            InitializeComponent();
        }

        private async void RecipesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRecipes();
        }

        private async System.Threading.Tasks.Task LoadRecipes()
        {
            try
            {
                var recipes = await apiService.GetRecipes();
                dgRecipes.ItemsSource = recipes;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void dgRecipes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRecipes.SelectedItem is Recipe recipe)
            {
                selectedRecipeId = recipe.recipe_id;
                try
                {
                    var fullRecipe = await apiService.GetRecipe(recipe.recipe_id);
                    dgComposition.ItemsSource = fullRecipe.composition;
                }
                catch (Exception ex)
                {
                    dgComposition.ItemsSource = null;
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            txtName.Text = "";
            txtVersion.Text = "1";
            txtComponentId.Text = "";
            txtPercentage.Text = "";
            txtLoadOrder.Text = "";
            txtAddError.Text = "";
            gridAdd.Visibility = Visibility.Visible;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                txtAddError.Text = "Введите название";
                return;
            }

            try
            {
                var model = new RecipeCreateModel
                {
                    name = txtName.Text,
                    version = int.Parse(txtVersion.Text)
                };

                var recipe = await apiService.CreateRecipe(model);

                if (!string.IsNullOrEmpty(txtComponentId.Text))
                {
                    var compModel = new CompositionModel
                    {
                        component_id = int.Parse(txtComponentId.Text),
                        percentage = decimal.Parse(txtPercentage.Text),
                        load_order = int.Parse(txtLoadOrder.Text)
                    };
                    await apiService.AddComposition(recipe.recipe_id, compModel);
                }

                gridAdd.Visibility = Visibility.Collapsed;
                await LoadRecipes();
            }
            catch (Exception ex)
            {
                txtAddError.Text = ex.Message;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            gridAdd.Visibility = Visibility.Collapsed;
        }
    }
}