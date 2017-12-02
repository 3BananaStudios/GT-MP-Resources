using System;
using GrandTheftMultiplayer.Shared.Gta;
using GrandTheftMultiplayer.Server;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;


class UserUI : Script
{
	[Command("cash")]
	public void cash_CMD(Client player)
	{
		API.triggerClientEvent(player, "showMoney");
	}
}
