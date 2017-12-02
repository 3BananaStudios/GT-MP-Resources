using System;
using GrandTheftMultiplayer.Shared.Gta;
using GrandTheftMultiplayer.Server;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;

public class MyScript : Script
{
	public MyScript()
	{
		API.onResourceStart += customResourceStart;
	}

	private void customResourceStart()
	{
		API.consoleOutput("My script started...");
		
	}

	[Command("hi", Alias = "hello")]
	public void SayHi(Client sender)
	{
		API.sendChatMessageToAll(string.Format("{0} is saying hi!!!!", sender.name));
	}

	[Command("givePistol")]
	public void GivePistol(Client sender)
	{
		API.givePlayerWeapon(sender, WeaponHash.Pistol, 100, false, true);
		API.sendChatMessageToAll(String.Format("Given a pistol to {0}", sender.name));
		API.exported.MoneyAPI.ChangeMoney(sender, -100);
	}

	[Command("me", GreedyArg = true)]
	public void MeCommand(Client sender, string action)
	{
		var message = String.Format("* {0} {1} *", sender.name, action);
		var playersInRange = API.getPlayersInRadiusOfPlayer(30f, sender);

		foreach (Client c in playersInRange)
		{
			API.sendChatMessageToPlayer(c, message);
		}
	}

	[Command("shout", Alias = "s", GreedyArg = true)]
	public void ShoutCommand(Client sender, string text)
	{
		var message = String.Format("{0}: *Shouts* {1}!", sender.name, text);
		var playersInRange = API.getPlayersInRadiusOfPlayer(50f, sender);

		foreach (Client c in playersInRange)
		{
			API.sendChatMessageToPlayer(c, message);
		}
	}

	[Command("SpawnCar")]
	public void SpawnCarCommand(Client Sender, int colour1, int colour2)
	{
		if (API.isPlayerInAnyVehicle(Sender))
			API.deleteEntity(API.getPlayerVehicle(Sender));

		var pos = API.getEntityPosition(Sender);

		var veh = API.createVehicle(VehicleHash.Elegy2, pos, new Vector3(), colour1, colour2);

		API.setPlayerIntoVehicle(Sender, veh, -1);
		API.sendChatMessageToPlayer(Sender, "Spawned a vehicle");
	}

	int ARMOR_PRICE = 250;
	[Command("armor")]
	public void CMD_BuyArmor(Client player)
	{
		if (API.exported.MoneyAPI.GetMoney(player) < ARMOR_PRICE)
		{
			API.sendChatMessageToPlayer(player, "You don't have enough money.");
			return;
		}
		API.exported.MoneyAPI.ChangeMoney(player, -ARMOR_PRICE);
		API.setPlayerArmor(player, 100);
		API.sendChatMessageToPlayer(player, "Bought body armor.");
		return;
	}
}

