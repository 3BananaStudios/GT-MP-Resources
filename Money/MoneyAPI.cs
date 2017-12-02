using System;
using GrandTheftMultiplayer.Shared.Gta;
using GrandTheftMultiplayer.Server;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

public class MoneyAPI : Script
{
	public static string myConnectionString = "SERVER=localhost;" + "PORT=3307;" + "DATABASE=gt_mp_data;" + "UID=root;" + "PASSWORD=root;";
	public static MySqlConnection connection;
	public static MySqlCommand command;
	public static MySqlDataReader reader;

	HashSet<Client> SaveList = new HashSet<Client>();
	DateTime LastAutosave = DateTime.Now;

	// DO NOT CHANGE THESE!!!!!!!
	long StartingMoney = 250;
	long MoneyCap = 9000000000000000;
	bool SaveEverytime = false;
	int AutosaveInterval = 5;

	public long Clamp(long value, long min, long max)
	{
		return (value <= min) ? min : (value >= max) ? max : value;
	}

	public void InitPlayer(Client player)
	{
		API.consoleOutput("-> Init Player");
		connection = new MySqlConnection(myConnectionString);
		command = connection.CreateCommand();
		command.CommandText = "SELECT money FROM user WHERE social_club_name=@user";
		command.Parameters.AddWithValue("user", player.name.ToString());
		connection.Open();
		using (reader = command.ExecuteReader())
		{
			API.consoleOutput("FOO");
			if(reader.HasRows)
			{
				reader.Read();
				long money = reader.GetInt64(0);

				player.setData("money_amount", money);
				API.triggerClientEvent(player, "UpdateMoneyHUD", Convert.ToString(money));
			}
			else
			{
				CreateWallet(player);
			}
		}
		connection.Close();
	}

	public void CreateWallet(Client player)
	{
		connection = new MySqlConnection(myConnectionString);
		command = connection.CreateCommand();
		command.CommandText = "INSERT INTO user('money') VALUES(@money)";
		command.Parameters.AddWithValue("money", StartingMoney);
		connection.Open();
		command.ExecuteNonQuery();
		connection.Close();

		player.setData("money_amount", StartingMoney);
		API.triggerClientEvent(player, "UpdateMoneyHUD", Convert.ToString(StartingMoney));
	}

	public bool SaveMoney(Client player, bool inside_loop = false)
	{
		if (!player.hasData("money_amount")) return false;

		connection = new MySqlConnection(myConnectionString);
		command = connection.CreateCommand();
		command.CommandText = "UPDATE user SET money=@money WHERE social_club_name=@user";
		command.Parameters.AddWithValue("money", player.getData("money_amount"));
		command.Parameters.AddWithValue("user", player.name.ToString());
		connection.Open();
		command.ExecuteNonQuery();
		connection.Close();

		if (!inside_loop) SaveList.Remove(player);
		return true;
	}

	public long GetMoney(Client player)
	{
		return (player.hasData("money_amount")) ? player.getData("money_amount") : 0;
	}

	public void ChangeMoney(Client player, long amount)
	{
		if (!player.hasData("money_amount")) return;

		player.setData("money_amount", Clamp(player.getData("money_amount") + amount, MoneyCap * -1, MoneyCap));
		API.triggerClientEvent(player, "UpdateMoneyHUD", Convert.ToString(player.getData("money_amount")), Convert.ToString(amount)); // disgusting hack for long support
		SaveList.Add(player);

		if (SaveEverytime) SaveMoney(player);
	}

	public void SetMoney(Client player, long amount)
	{

		if (!player.hasData("money_amount")) return;

		player.setData("money_amount", Clamp(amount, MoneyCap * -1, MoneyCap));
		API.triggerClientEvent(player, "UpdateMoneyHUD", Convert.ToString(player.getData("money_amount"))); // disgusting hack for long support
		SaveList.Add(player);

		if (SaveEverytime) SaveMoney(player);
	}

	public MoneyAPI()
	{
		API.onResourceStart += MoneyAPI_INIT;
		API.onResourceStop += MoneyAPI_STOP;
		API.onUpdate += MoneyAPI_UPDATE;
		API.onPlayerFinishedDownload += MoneyAPI_PLAYER_JOIN;
		API.onPlayerDisconnected += MoneyAPI_PLAYER_LEAVE;
	}

	public void MoneyAPI_INIT()
	{
		if (API.hasSetting("walletDefault")) StartingMoney = API.getSetting<long>("walletDefault");
		if (API.hasSetting("walletCap")) MoneyCap = API.getSetting<long>("walletCap");
		if (API.hasSetting("walletInterval")) AutosaveInterval = API.getSetting<int>("walletInterval");
		if (API.hasSetting("walletSave")) SaveEverytime = API.getSetting<bool>("walletSave");

		API.consoleOutput("MoneyAPI Loaded");
		API.consoleOutput("-> Starting Money: ${0:n0}", StartingMoney);
		API.consoleOutput("-> Money Cap: ${0:n0}", MoneyCap);
		API.consoleOutput("-> Autosave: {0}", AutosaveInterval == 0 ? "Disabled" : "every " + AutosaveInterval + " minutes");
		API.consoleOutput("-> Save After Operation: {0}", SaveEverytime ? "Enabled" : "Disabled");

		foreach (Client player in API.getAllPlayers()) InitPlayer(player);
	}

	public void MoneyAPI_STOP()
	{
		foreach (Client player in SaveList) SaveMoney(player, true);

		SaveList.Clear();
		SaveList.TrimExcess();
	}

	public void MoneyAPI_UPDATE()
	{
		if (AutosaveInterval == 0) return;

		if(DateTime.Now.Subtract(LastAutosave).Minutes >= AutosaveInterval)
		{
			int savedCount = 0;
			foreach (Client player in SaveList) if (SaveMoney(player, true)) savedCount++;
			if(savedCount > 0) API.consoleOutput("-> Autosaved {0} wallets.", savedCount);

			SaveList.Clear();
			SaveList.TrimExcess();

			LastAutosave = DateTime.Now;
		}
	}

	public void MoneyAPI_PLAYER_JOIN(Client player)
	{
		InitPlayer(player);
	}

	public void MoneyAPI_PLAYER_LEAVE(Client player, string reason)
	{
		SaveMoney(player);
	}
}
