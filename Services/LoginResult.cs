namespace Asset.Services
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }

        public static LoginResult Ok()
            => new LoginResult { Success = true };

        public static LoginResult Fail(string msg)
            => new LoginResult { Success = false, Error = msg };
    }
}

