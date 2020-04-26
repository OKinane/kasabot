namespace KasaBot.Router
{
    public sealed class RouterCredentials
    {
        public string Host { get; }
        public string User { get; }
        public string Password { get; }

        public RouterCredentials(string host, string user, string password)
        {
            Host = host;
            User = user;
            Password = password;
        }
    }
}