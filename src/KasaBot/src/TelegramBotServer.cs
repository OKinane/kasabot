using System;
using System.Threading.Tasks;
using KasaBot.Router;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace KasaBot
{
    internal sealed class TelegramBotServer
    {
        public TelegramBotServer(ITelegramBotClient botClient, IAuthenticator authenticator, IRouterManager routerManager)
        {
            this.bot = new Lazy<Task<(ITelegramBotClient Client, string Name)>>(async () =>
            {
                var botUser = await botClient.GetMeAsync().ConfigureAwait(false);
                return (botClient, botUser.FirstName);
            });
            this.authenticator = authenticator;
            this.routerManager = routerManager;
        }

        public async Task RunAsync()
        {
            var (client, name) = await bot.Value.ConfigureAwait(false);
            System.Console.WriteLine($"Bot '{name}' started");
            client.OnMessage += OnMessageHandler;
            client.StartReceiving();
            await Task.Delay(-1).ConfigureAwait(false);
        }

        private readonly Lazy<Task<(ITelegramBotClient Client, string Name)>> bot;
        private readonly IAuthenticator authenticator;
        private readonly IRouterManager routerManager;

        private async void OnMessageHandler(object sender, MessageEventArgs messageEventArgs)
        {
            System.Console.WriteLine($"[ChatId={messageEventArgs.Message.Chat.Id}] New message");
            Func<object, Task> sendTextAsync = null;
            try
            {
                var message = messageEventArgs.Message;
                if (message == null || message.Type != MessageType.Text) return;

                var botInfo = await bot.Value.ConfigureAwait(false);
                sendTextAsync = obj =>
                {
                    string text = obj.ToString();
                    System.Console.WriteLine($"[ChatId={message.Chat.Id}] Sending: {text}");
                    return botInfo.Client.SendTextMessageAsync(message.Chat.Id, text);
                };

                if (message.From == null)
                {
                    await sendTextAsync("ERROR: message.From == null").ConfigureAwait(false);
                    return;
                }
                System.Console.WriteLine($"[ChatId={message.Chat.Id}] From: {message.From.Id};{message.From.FirstName}");
                if (!authenticator.TryAuthenticate(message.From, out var userFriendlyName))
                {
                    var userId = message.From.Id;
                    await sendTextAsync("You are not allowed to use this bot. Your user ID: " + userId).ConfigureAwait(false);
                    return;
                }
                System.Console.WriteLine($"[ChatId={message.Chat.Id}] Handling message: {message.Text}");
                await HandleCommand(sendTextAsync, message.Text, botInfo.Name, userFriendlyName).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                try
                {
                    if (sendTextAsync != null)
                        await sendTextAsync("ERROR: " + e.Message).ConfigureAwait(false);
                }
                catch (Exception sendException)
                {
                    Console.Error.WriteLine(sendException);
                }
            }
        }

        private async Task HandleCommand(Func<object, Task> sendTextAsync, string command, string botName, string userFriendlyName)
        {
            switch (command.ToLowerInvariant())
            {
                case "wifi off":
                case "wifioff":
                    await sendTextAsync("Shutting down wifi").ConfigureAwait(false);
                    var offResult = await routerManager.ToggleWifi(userFriendlyName, false, botName).ConfigureAwait(false);
                    await sendTextAsync(offResult).ConfigureAwait(false);
                    break;
                case "wifi on":
                case "wifion":
                    await sendTextAsync("Bringing wifi up").ConfigureAwait(false);
                    var onResult = await routerManager.ToggleWifi(userFriendlyName, true, botName).ConfigureAwait(false);
                    await sendTextAsync(onResult).ConfigureAwait(false);
                    break;
                default:
                    await sendTextAsync("Unknown command: " + command).ConfigureAwait(false);
                    break;
            }
        }
    }
}