using System;
using GrandTheftMultiplayer.Shared.Gta;
using GrandTheftMultiplayer.Server;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;

public class UserJoin : Script
{
	public UserJoin()
	{
		API.onClientEventTrigger += OnCleintEvent;
	}

	private void OnCleintEvent(Client sender, string eventName, object[] arguments)
	{
		if (eventName == "Login")
			Login(sender, arguments[0].ToString(), arguments[1].ToString());
		if (eventName == "Register")
			Register(sender, arguments[0].ToString(), arguments[1].ToString());
	}
	
	public void Login(Client player, string username, string password)
	{
		var doesPlayerExist = API.exported.mySQLDatabase.PlayerExists(player);

		if (doesPlayerExist)
		{
			var isPasswordCorrect = API.exported.mySQLDatabase.CheckPassword(player, password);
			if (isPasswordCorrect)
			{
				API.triggerClientEvent(player, "successfulLogin");
				API.exported.MoneyAPI.InitPlayer(player);
				SetPlayerStats(player, username);
			}
			else
				API.triggerClientEvent(player, "failedLogin");
		}
		else
			API.triggerClientEvent(player, "failedLogin");


	}

	public void Register(Client player, string username, string password)
	{
		var doesPlayerExist = API.exported.mySQLDatabase.PlayerExists(player);
		if (doesPlayerExist)
		{
			API.triggerClientEvent(player, "failedRegisterUserExists");
			return;
		}

		API.exported.mySQLDatabase.RegisterNewUser(player, username, password);
		API.triggerClientEvent(player, "successfulRegister");
		API.exported.MoneyAPI.InitPlayer(player);
		SetPlayerStats(player, username);
	}

	public void SetPlayerStats(Client player, string username)
	{
		API.setPlayerSkin(player, API.exported.mySQLDatabase.GetSkinID(player)); //Sets the players skin which is saved in the DB
	}

}