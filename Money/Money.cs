using System;
using GrandTheftMultiplayer.Shared.Gta;
using GrandTheftMultiplayer.Server;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using System.Collections.Generic;

public class Money : Script
{
	HashSet<Client> SaveList = new HashSet<Client>();
	DateTime LastAutosave = DateTime.Now;

	//DO NOT CHANGE
	public int StartingMoney = 250;
	public int AutosaveInterval = 2;
	public bool SaveEverytime = false;
	//Do NOT CHANGE

	[Command("InitMoney")]
	public void PlayerInit(Client player)
	{
		API.consoleOutput("--> Init player");
		var money = API.exported.mySQLDatabase.GetMoney(player);
		player.setData("money", money);
	}

	public void ChangeMoney(Client player, int amount)
	{
		player.setData("money", player.getData("money") + amount);
		API.consoleOutput("--> Changed " + player.name + "'s money");

		SaveList.Add(player);
	}

	public void SetMoney(Client player, int amount)
	{
		player.setData("money", amount);
		API.consoleOutput("--> Changed " + player.name + "'s money");

		SaveList.Add(player);
	}

	public bool SaveMoney(Client player, bool is_saving_loop_running = false)
	{
		if (!player.hasData("money")) return false;

		API.exported.mySQLDatabase.SaveMoney(player, player.getData("money"));
		API.consoleOutput("--> Saving " + player.name + "'s money");

		if (is_saving_loop_running) SaveList.Remove(player);
		return true;
	}

	public Money()
	{
		API.onResourceStart += Money_START;
		API.onResourceStop += Money_STOP;
		API.onUpdate += Money_UPDATE;
		//API.onPlayerFinishedDownload += Money_CONNECTED;
		API.onPlayerDisconnected += Money_Disconnected;
	}

	private void Money_Disconnected(Client player, string reason)
	{
		SaveMoney(player, false);
	}

	private void Money_CONNECTED(Client player)
	{
		PlayerInit(player);
	}

	private void Money_UPDATE()
	{
		if (AutosaveInterval == 0) return;

		if (DateTime.Now.Subtract(LastAutosave).Minutes >= AutosaveInterval)
		{
			int savedCount = 0;
			foreach (Client player in SaveList) if (SaveMoney(player, true)) savedCount++;
			if (savedCount > 0) API.consoleOutput("-> Autosaved {0} wallets.", savedCount);

			SaveList.Clear();
			SaveList.TrimExcess();

			LastAutosave = DateTime.Now;
		}
	}

	private void Money_STOP()
	{
		foreach (Client player in API.getAllPlayers())
		{
			SaveMoney(player, false);
		}

		SaveList.Clear();
	}

	private void Money_START()
	{
		if (API.hasSetting("moneyDefault")) StartingMoney = API.getSetting<int>("moneyDefault");
		if (API.hasSetting("moneyInterval")) AutosaveInterval = API.getSetting<int>("moneyInterval");
		if (API.hasSetting("moneySave")) SaveEverytime = API.getSetting<bool>("moneySave");

		API.consoleOutput("--> Starting money set to: {0}", StartingMoney);
		API.consoleOutput("--> Auto-save interval set to: {0}", AutosaveInterval);
		API.consoleOutput("--> Save after every action set to: {0}", SaveEverytime);

		foreach (Client player in API.getAllPlayers())
		{
			PlayerInit(player);
		}
	}
}

