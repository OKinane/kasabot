using System.Threading.Tasks;
using KasaBot.Router;

namespace KasaBot.Ssh
{
    public interface ISshSessionFactory
    {
        ISshSession Create(RouterCredentials routerCredentials);
    }

    internal sealed class RenciSshNetSessionFactory : ISshSessionFactory
    {
        public ISshSession Create(RouterCredentials credentials)
        {
            var sshClient = new Renci.SshNet.SshClient(credentials.Host, credentials.User, credentials.Password);
            sshClient.Connect();
            return new SshSession(sshClient, credentials.User);
        }

        private sealed class SshSession : ISshSession
        {
            private Renci.SshNet.SshClient sshClient;

            public string CurrentUser { get; }

            public SshSession(Renci.SshNet.SshClient sshClient, string currentUser)
            {
                this.sshClient = sshClient;
                CurrentUser = currentUser;
            }

            public async Task<SshCommandResult> Run(string commandText)
            {
                System.Console.WriteLine($"[SSH] Executing: {commandText}");
                var command = sshClient.CreateCommand(commandText);
                await Task.Factory.FromAsync(command.BeginExecute, command.EndExecute, null).ConfigureAwait(false);
                return new SshCommandResult(command.ExitStatus, command.Result, command.Error);
            }
            public void Dispose()
            {
                sshClient.Disconnect();
                sshClient.Dispose();
            }
        }
    }
}