using Asset.Services;
using System.Windows;

namespace ABS_WIZZ.UI
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private async void SignIn_Click(object sender, RoutedEventArgs e)
        {
            TB_Error.Text = "";
            IsEnabled = false;

            var ok = await LicenseManager.LoginAsync(
                TB_Username.Text,
                PB_Password.Password,
                m => TB_Error.Text = m);

            IsEnabled = true;

            if (ok)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
