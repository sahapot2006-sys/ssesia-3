using System;
using System.Windows;
using System.Windows.Controls;
using Technolog.Models;
using Technolog.Services;

namespace Technolog.Pages
{
    public partial class ExtruderPage : Page
    {
        private ApiService apiService = new ApiService();

        public ExtruderPage()
        {
            InitializeComponent();
        }

        private async void ExtruderPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPrograms();
        }

        private async System.Threading.Tasks.Task LoadPrograms()
        {
            try
            {
                var programs = await apiService.GetExtruderPrograms();
                dgPrograms.ItemsSource = programs;
            }
            catch (Exception ex)
            {
                dgPrograms.ItemsSource = null;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            txtProgramName.Text = "";
            txtProductId.Text = "";
            txtZones.Text = "80;85;90";
            txtScrewSpeed.Text = "300";
            txtFeederRate.Text = "50";
            txtAddError.Text = "";
            gridAdd.Visibility = Visibility.Visible;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtProgramName.Text) || string.IsNullOrEmpty(txtProductId.Text))
            {
                txtAddError.Text = "Заполните название и ID продукта";
                return;
            }

            try
            {
                var model = new ExtruderProgramModel
                {
                    program_name = txtProgramName.Text,
                    product_id = int.Parse(txtProductId.Text),
                    temperature_zones = txtZones.Text,
                    screw_speed = int.Parse(txtScrewSpeed.Text),
                    feeder_rate = decimal.Parse(txtFeederRate.Text)
                };

                await apiService.CreateExtruderProgram(model);
                gridAdd.Visibility = Visibility.Collapsed;
                await LoadPrograms();
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