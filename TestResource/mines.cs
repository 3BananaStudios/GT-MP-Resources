using System;
using GrandTheftMultiplayer.Server;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;

public class MinesTest : Script
{
	public MinesTest()
	{
		API.onResourceStart += myResourceStart;
	}

	private void myResourceStart()
	{
		API.consoleOutput("Starting my Resource!");
	}

	[Command("mine")]
	public void PlaceMine(Client sender, float mineRange = 10f)
	{
		var pos = API.getEntityPosition(sender);
		var playerDimension = API.getEntityDimension(sender);

		var prop = API.createObject(API.getHashKey("prop_bomb_01"), pos - new Vector3(0, 0, 1f), new Vector3(), playerDimension);
		var shape = API.createSphereColShape(pos, mineRange);
		shape.dimension = playerDimension;

		bool mineArmed = false;

		//Trigger when a player is in range of the mine
		shape.onEntityEnterColShape += (s, ent) =>
		{
			if (!mineArmed) return;
			API.createOwnedExplosion(sender, ExplosionType.HiOctane, pos, 1f, playerDimension);
			API.deleteEntity(prop);
			API.deleteColShape(shape);
		};

		//Trigger when a player leaves the mines range
		shape.onEntityEnterColShape += (s, ent) =>
		{
			if(ent == sender.handle && !mineArmed)
			{
				//The player has left the mines range so lets arm it
				mineArmed = true;
				API.sendNotificationToPlayer(sender, "Mine has been armed!", true);
			}
		};
	}
}
