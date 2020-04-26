using System;
using System.Threading.Tasks;

namespace KasaBot.Ssh
{
    public interface ISshSession : IDisposable
    {
        string CurrentUser { get; }
        Task<SshCommandResult> Run(string commandText);
    }
}