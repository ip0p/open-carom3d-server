using System;
using System.Collections.Generic;

namespace business.service
{
    public class AccountService
    {
        private static AccountService _instance;
        private Dictionary<string, Account> _accounts;
        private Dictionary<uint, Player> _players;

        private AccountService()
        {
            _accounts = new Dictionary<string, Account>();
            _players = new Dictionary<uint, Player>();
        }

        public static AccountService GetInstance()
        {
            if (_instance == null)
            {
                _instance = new AccountService();
            }
            return _instance;
        }

        public Account CreateAccount(string accountName, string accountPassword)
        {
            // TODO: check if account already exists
            uint accountId = (uint)(_accounts.Count + 1);
            Account newAccount = new Account(accountId, accountName, "");

            string accountKey = accountName.ToLower();
            _accounts[accountKey] = newAccount;
            return newAccount;
        }

        public Player CreatePlayerFromAccount(Account account)
        {
            Player newPlayer = new Player(account);
            _players[account.Id] = newPlayer;
            return newPlayer;
        }

        public Account LogAccount(string accountName, string accountPassword)
        {
            Account account = FindAccount(accountName);
            return account;
        }

        public Account FindAccount(string accountName)
        {
            string accountId = accountName.ToLower();
            _accounts.TryGetValue(accountId, out Account account);
            return account;
        }

        public Player GetPlayer(Account account)
        {
            _players.TryGetValue(account.Id, out Player player);
            return player;
        }
    }
}
