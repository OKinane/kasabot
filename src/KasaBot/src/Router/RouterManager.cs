using System.Threading.Tasks;
using KasaBot.Ssh;

namespace KasaBot.Router
{
    public interface IRouterManager
    {
        Task<SshCommandResult> ToggleWifi(string user, bool activate, string logTag);
    }

    public sealed class RouterManager : IRouterManager
    {
        public RouterManager(ISshSessionFactory sshSessionFactory, RouterCredentials routerCredentials)
        {
            this.sshSessionFactory = sshSessionFactory;
            this.routerCredentials = routerCredentials;
        }

        public async Task<SshCommandResult> ToggleWifi(string userName, bool activate, string logTag)
        {
            using var session = sshSessionFactory.Create(routerCredentials);
            var radioCommand = "radio " + (activate ? "on" : "off");
            await SysLog(session, logTag, $"'{userName}' executed: {radioCommand}").ConfigureAwait(false);
            return await session.Run(radioCommand).ConfigureAwait(false);
        }
        
        private readonly ISshSessionFactory sshSessionFactory;
        private readonly RouterCredentials routerCredentials;

        private static async Task<SshCommandResult> SysLog(ISshSession session, string logTag, string message)
        {
            string Escape(string text) => text == null ? null : $"\"{text.Replace("\"", "\\\"")}\"";
            var tag = $"{logTag}[ssh user:{session.CurrentUser}]";
            return await session.Run($"logger -t {Escape(tag)} {Escape(message)}").ConfigureAwait(false);
        }
    }
}