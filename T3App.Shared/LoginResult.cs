namespace T3App.Shared
{
    public class LoginResult
    {
        public string Email { get; set; }
        public string Role { get; set; }
        public string OriginalEMail { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public bool Successful { get; set; }
        public string Error { get; set; }
    }
}
