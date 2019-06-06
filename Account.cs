using System.Collections.Generic;

namespace PalimPalim
{

    public class Configuration
    {
        public bool Silent { get; set; }

        public bool PushBalanceNotification { get; set; }

        public List<Account> Accounts { get; set; }
    }

    public class Account
    {
        public int Id { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
