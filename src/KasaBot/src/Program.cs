using System;
using System.IO;
using System.Threading.Tasks;
using dotenv.net;
using KasaBot.Router;
using KasaBot.Ssh;
using Telegram.Bot;

namespace KasaBot
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                    LoadConfigIntoEnvironment(args[0]);
                var BOT_ACCESS_TOKEN = ReadEnvVarOnce("BOT_ACCESS_TOKEN");
                var BOT_AUTHORIZED_USERS = ReadEnvVarOnce("BOT_AUTHORIZED_USERS");
                var BOT_ROUTER_HOSTNAME = ReadEnvVarOnce("BOT_ROUTER_HOSTNAME");
                var BOT_ROUTER_SSH_USERNAME = ReadEnvVarOnce("BOT_ROUTER_SSH_USERNAME");
                var BOT_ROUTER_SSH_PASSWORD = ReadEnvVarOnce("BOT_ROUTER_SSH_PASSWORD");
                
                var botClient = new TelegramBotClient(BOT_ACCESS_TOKEN);
                var authenticator = SimpleAuthenticator.Parse(BOT_AUTHORIZED_USERS);
                var routerCredentials = new RouterCredentials(BOT_ROUTER_HOSTNAME, BOT_ROUTER_SSH_USERNAME, BOT_ROUTER_SSH_PASSWORD);
                var sshSessionFactory = new RenciSshNetSessionFactory();
                var routerManager = new RouterManager(sshSessionFactory, routerCredentials);
                var server = new TelegramBotServer(botClient, authenticator, routerManager);
                await server.RunAsync().ConfigureAwait(false);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return -1;
            }
        }

        private static void LoadConfigIntoEnvironment(string dotEnvPath)
        {
            var dotEnvFilePath = new FileInfo(dotEnvPath);
            DotEnv.Config(true, dotEnvFilePath.FullName);
        }
        private static string ReadEnvVarOnce(string envVarName)
        {
            var value = Environment.GetEnvironmentVariable(envVarName);
            if (value == null)
                throw new Exception($"Environment variable '{envVarName}' is not set");
            Environment.SetEnvironmentVariable(envVarName, null);
            return value;
        }
    }
}
