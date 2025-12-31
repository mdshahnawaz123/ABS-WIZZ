using System;
using System.Threading.Tasks;

namespace Asset.Services
{
    public static class LicenseManager
    {
        private static readonly string URL =
            "https://raw.githubusercontent.com/mdshahnawaz123/plugin-access-control/main/users.json";

        public static async Task<LoginResult> TryAutoLoginAsync()
        {
            var token = TokenService.LoadToken();
            if (token == null)
                return LoginResult.Fail("Login required.");

            if (token.MachineId != MachineHelper.GetMachineId())
                return LoginResult.Fail("Different machine.");

            if (token.ExpiresUtc < DateTime.UtcNow)
            {
                TokenService.DeleteToken();
                return LoginResult.Fail("License expired.");
            }

            // Online validation
            try
            {
                var auth = new AuthService(URL);
                var loaded = await auth.LoadUsersAsync();

                if (loaded)
                {
                    var user = auth.GetUser(token.Username);
                    if (user == null || !user.Active)
                    {
                        TokenService.DeleteToken();
                        return LoginResult.Fail("License revoked.");
                    }
                }
            }
            catch
            {
                // allow offline
            }

            return LoginResult.Ok();
        }


        public static async Task<bool> LoginAsync(string u, string p, Action<string> err)
        {
            var auth = new AuthService(URL);

            var ok = await auth.ValidateCredentialsAsync(u, p, err);
            if (!ok) return false;

            var token = new LocalAuthToken
            {
                Username = auth.CurrentUser.Username,
                MachineId = MachineHelper.GetMachineId(),
                Plan = auth.CurrentUser.Plan,
                ExpiresUtc = auth.CurrentUser.Expires.ToUniversalTime()
            };

            TokenService.SaveToken(token);
            return true;
        }
    }
}
