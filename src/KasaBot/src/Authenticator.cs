using System;
using System.Collections.Immutable;
using Telegram.Bot.Types;

namespace KasaBot
{
    public interface IAuthenticator
    {
        bool TryAuthenticate(User telegramUser, out string userFriendlyName);
    }

    internal class SimpleAuthenticator : IAuthenticator
    {
        public static SimpleAuthenticator Parse(string authorizedUsers)
        {
            var dict = ImmutableDictionary.CreateBuilder<int, string>();
            int idPosition = 0;
            do
            {
                var equalPosition = authorizedUsers.IndexOf('=', idPosition);
                if (equalPosition <= idPosition)
                    throw new ArgumentException($"{nameof(authorizedUsers)} is malformed");
                var separatorPosition = authorizedUsers.IndexOf(';', idPosition);
                if (separatorPosition == idPosition
                    || (separatorPosition > idPosition && separatorPosition < equalPosition))
                    throw new ArgumentException($"{nameof(authorizedUsers)} is malformed");
                var idSpan = authorizedUsers.AsSpan(idPosition, equalPosition - idPosition);
                if (!int.TryParse(idSpan, out var id))
                    throw new ArgumentException($"{nameof(authorizedUsers)} is malformed");
                ReadOnlySpan<char> nameSpan;
                if (separatorPosition < 0)
                {
                    nameSpan = authorizedUsers.AsSpan(equalPosition + 1);
                    idPosition = -1;
                }
                else
                {
                    nameSpan = authorizedUsers.AsSpan(equalPosition + 1, separatorPosition - equalPosition - 1);
                    idPosition = separatorPosition + 1;
                }
                dict.Add(id, new string(nameSpan));
            } while (idPosition > 0);
            return new SimpleAuthenticator(dict.ToImmutable());
        }

        public bool TryAuthenticate(User telegramUser, out string userFriendlyName)
        {
            return users.TryGetValue(telegramUser.Id, out userFriendlyName);
        }
        
        private readonly ImmutableDictionary<int, string> users;

        private SimpleAuthenticator(ImmutableDictionary<int, string> users)
        {
            this.users = users;
        }
    }
}