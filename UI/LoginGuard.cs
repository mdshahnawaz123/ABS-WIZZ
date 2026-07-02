using ABS_WIZZ.Services;
using System.Threading.Tasks;
using System.Windows;

namespace ABS_WIZZ.UI
{
    public static class LoginGuard
    {
        private static bool _isAuthenticated = false;

        /// <summary>
        /// Central check to ensure the user is authorized.
        /// It will attempt auto-login first, then show the LoginWindow if needed.
        /// </summary>
        public static bool IsAuthorized()
        {
            // 1. Session check — if already logged in this Revit session, skip everything
            if (_isAuthenticated) return true;

            // 2. Auto-login check — attempts to load from encrypted local token
            var autoLoginResult = Task.Run(async () => await LicenseManager.TryAutoLoginAsync()).Result;

            if (autoLoginResult.Success)
            {
                _isAuthenticated = true;
                return true;
            }

            // 3. Manual login — show modal LoginWindow if auto-login fails
            var loginWindow = new LoginWindow();
            loginWindow.HideIcon();

            bool? result = loginWindow.ShowDialog();
            if (result == true)
            {
                _isAuthenticated = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resets the authentication state.
        /// </summary>
        public static void Reset() => _isAuthenticated = false;
    }
}
