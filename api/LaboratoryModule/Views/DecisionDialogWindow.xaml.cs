using System.Windows;
using LaboratoryModule.Models;

namespace LaboratoryModule.Views
{
    public partial class DecisionDialogWindow : Window
    {
        public LabDecision SelectedDecision { get; private set; }
        public string BlockReason { get; private set; } = string.Empty;

        public DecisionDialogWindow()
        {
            InitializeComponent();

            BlockRadio.Checked += (s, e) =>
            {
                ReasonLabel.Visibility = Visibility.Visible;
                BlockReasonBox.Visibility = Visibility.Visible;
            };

            ApproveRadio.Checked += (s, e) =>
            {
                ReasonLabel.Visibility = Visibility.Collapsed;
                BlockReasonBox.Visibility = Visibility.Collapsed;
            };
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (BlockRadio.IsChecked == true && string.IsNullOrWhiteSpace(BlockReasonBox.Text))
            {
                MessageBox.Show("Укажите причину блокировки!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedDecision = ApproveRadio.IsChecked == true ? LabDecision.Approved : LabDecision.Blocked;
            BlockReason = BlockReasonBox.Text;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}