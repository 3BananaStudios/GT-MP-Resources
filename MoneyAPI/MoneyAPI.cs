using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.IO;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;

namespace MoneyAPI
{
    public class MoneyAPI : Script
    {
		public static string myConnectionString = "SERVER=localhost;" + "PORT=3307;" + "DATABASE=gt_mp_data;" + "UID=root;" + "PASSWORD=root;";
		public static MySqlConnection connection;
		public static MySqlCommand command;
		public static MySqlDataReader reader;


		/* Config - don't touch these, modify meta.xml instead */
		long StartingMoney = 250;
        long MoneyCap = 9000000000000000;
        bool SaveEverytime = false;
        int AutosaveInterval = 5;
        bool Logging = false;

        HashSet<Client> SaveList = new HashSet<Client>();
        DateTime LastAutosave = DateTime.Now;

        /* spaghetti begins here */
        public long Clamp(long value, long min, long max) // http://stackoverflow.com/a/3176617
        {
            return (value <= min) ? min : (value >= max) ? max : value;
        }

        public void InitPlayer(Client player)
        {

			connection = new MySqlConnection(myConnectionString);
			command = connection.CreateCommand();

			connection.Open();

			command.CommandText = "SELECT Money FROM wallets WHERE SocialClub=@sc";
			command.Parameters.AddWithValue("@sc", player.socialClubName);

			reader = command.ExecuteReader();
			if (reader.HasRows)
			{
				reader.Read();
				long money = reader.GetInt64(0);

				player.setData("wallet_Amount", money);
				API.triggerClientEvent(player, "UpdateMoneyHUD", Convert.ToString(money)); // disgusting hack for long support
			}
			else
			{
				CreateWallet(player);
			}

			connection.Close();
					
        }

        public void CreateWallet(Client player)
        {
			connection = new MySqlConnection(myConnectionString);
			command = connection.CreateCommand();
			connection.Open();

			command.CommandText = "INSERT INTO wallets (SocialClub, Money) VALUES (@sc, @money)";
			command.Parameters.AddWithValue("@sc", player.socialClubName);
			command.Parameters.AddWithValue("@money", StartingMoney);
			command.ExecuteNonQuery();

			connection.Close();

			player.setData("wallet_Amount", StartingMoney);
			API.triggerClientEvent(player, "UpdateMoneyHUD", Convert.ToString(StartingMoney)); // disgusting hack for long support
                    
        }

        public bool SaveWallet(Client player, bool inside_loop = false)
        {
			connection = new MySqlConnection(myConnectionString);
			command = connection.CreateCommand();
			connection.Open();

			command.CommandText = "UPDATE wallets SET Money=@money WHERE SocialClub=@sc";
			command.Parameters.AddWithValue("@money", player.getData("wallet_Amount"));
			command.Parameters.AddWithValue("@sc", player.socialClubName);
			command.ExecuteNonQuery();

			connection.Close();
            if (!inside_loop) SaveList.Remove(player);
			return true;
        }

        public void WalletLog(Client player, long amount, string function)
        {
            using (MySqlConnection sqlCon = new MySqlConnection(myConnectionString))
            {
                using (MySqlCommand sqlCmd = new MySqlCommand())
                {
                    try {
                        sqlCon.Open();

                        sqlCmd.CommandText = "INSERT INTO wallet_logs (SocialClub, Amount, Function, Date) VALUES (@sc, @amount, @func, DATETIME('NOW', 'LOCALTIME'))";
                        sqlCmd.Parameters.AddWithValue("@sc", player.socialClubName);
                        sqlCmd.Parameters.AddWithValue("@amount", amount);
                        sqlCmd.Parameters.AddWithValue("@func", function);
                        sqlCmd.ExecuteNonQuery();

                        sqlCon.Close();
                    } catch (MySqlException e) {
                        API.consoleOutput("{0} Logging Error: #{1} - {2}", function, e.ErrorCode, e.Message);
                    }
                }
            }
        }

        public long GetMoney(Client player)
        {
            return (player.hasData("wallet_Amount")) ? player.getData("wallet_Amount") : 0;
        }

        public void ChangeMoney(Client player, long amount)
        {
            if (!player.hasData("wallet_Amount")) return;
            player.setData("wallet_Amount", Clamp(player.getData("wallet_Amount") + amount, MoneyCap * -1, MoneyCap));
            API.triggerClientEvent(player, "UpdateMoneyHUD", Convert.ToString(player.getData("wallet_Amount")), Convert.ToString(amount)); // disgusting hack for long support
            SaveList.Add(player);

            if (SaveEverytime) SaveWallet(player);
            if (Logging) WalletLog(player, amount, System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void SetMoney(Client player, long amount)
        {
            if (!player.hasData("wallet_Amount")) return;
            player.setData("wallet_Amount", Clamp(amount, MoneyCap * -1, MoneyCap));
            API.triggerClientEvent(player, "UpdateMoneyHUD", Convert.ToString(player.getData("wallet_Amount"))); // disgusting hack for long support
            SaveList.Add(player);

            if (SaveEverytime) SaveWallet(player);
            if (Logging) WalletLog(player, amount, System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public MoneyAPI()
        {
            API.onResourceStart += MoneyAPI_Init;
            API.onResourceStop += MoneyAPI_Exit;
            API.onUpdate += MoneyAPI_Update;
            //API.onPlayerFinishedDownload += MoneyAPI_PlayerJoin;
            API.onPlayerDisconnected += MoneyAPI_PlayerLeave;
        }

        public void MoneyAPI_Init()
        {
            if (API.hasSetting("walletDefault")) StartingMoney = API.getSetting<long>("walletDefault");
            if (API.hasSetting("walletCap")) MoneyCap = API.getSetting<long>("walletCap");
            if (API.hasSetting("walletInterval")) AutosaveInterval = API.getSetting<int>("walletInterval");
            if (API.hasSetting("walletSave")) SaveEverytime = API.getSetting<bool>("walletSave");
            if (API.hasSetting("walletLog")) Logging = API.getSetting<bool>("walletLog");

            API.consoleOutput("MoneyAPI Loaded");
            API.consoleOutput("-> Starting Money: ${0:n0}", StartingMoney);
            API.consoleOutput("-> Money Cap: ${0:n0}", MoneyCap);
            API.consoleOutput("-> Autosave: {0}", AutosaveInterval == 0 ? "Disabled" : "every " + AutosaveInterval + " minutes");
            API.consoleOutput("-> Save After Operation: {0}", SaveEverytime ? "Enabled" : "Disabled");
            API.consoleOutput("-> Logging: {0}", Logging ? "Enabled" : "Disabled");

            // create tables if they don't exist
            /*
			connection = new MySqlConnection(myConnectionString);
			command = new MySqlCommand();

			connection.Open();

			command.CommandText = "CREATE TABLE IF NOT EXISTS wallets (SocialClub NVARCHAR(20) PRIMARY KEY, Money BIGINT)";
			command.ExecuteNonQuery();

			command.CommandText = "CREATE TABLE IF NOT EXISTS wallet_logs (ID INTEGER PRIMARY KEY AUTOINCREMENT, SocialClub NVARCHAR(20), Amount BIGINT, Function VARCHAR(20), Date DATETIME)";
			command.ExecuteNonQuery();

			connection.Close();*/
                  

            // load wallets of connected players
            foreach (Client player in API.getAllPlayers()) InitPlayer(player);
        }

        public void MoneyAPI_Exit()
        {
            foreach (Client player in SaveList) SaveWallet(player, true);

            SaveList.Clear();
            SaveList.TrimExcess();
        }

        public void MoneyAPI_Update()
        {
            if (AutosaveInterval == 0) return;
            if (DateTime.Now.Subtract(LastAutosave).Minutes >= AutosaveInterval)
            {
                int savedCount = 0;
                foreach (Client player in SaveList) if (SaveWallet(player, true)) savedCount++;
                if (savedCount > 0) API.consoleOutput("-> Autosaved {0} wallets.", savedCount);

                SaveList.Clear();
                SaveList.TrimExcess();

                LastAutosave = DateTime.Now;
            }

			
        }

        public void MoneyAPI_PlayerJoin(Client player)
        {
            InitPlayer(player);
        }

        public void MoneyAPI_PlayerLeave(Client player, string reason)
        {
            SaveWallet(player);
        }
    }
}