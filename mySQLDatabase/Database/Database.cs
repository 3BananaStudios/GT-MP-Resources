using System;
using MySql.Data.MySqlClient;
using GrandTheftMultiplayer;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;

public class Database : Script
{
	public static string myConnectionString = "SERVER=localhost;" + "PORT=3307;" + "DATABASE=gt_mp_data;" + "UID=root;" + "PASSWORD=root;";
	public static MySqlConnection connection;
	public static MySqlCommand command;
	public static MySqlDataReader reader;

	public Boolean PlayerExists(Client player)
	{
		//Create new connection
		connection = new MySqlConnection(myConnectionString);
		//Create a new command
		command = connection.CreateCommand();
		//Set the command - Create a query
		command.CommandText = "SELECT * FROM user";

		//Open connection
		connection.Open();
		//Tell the reader what to read
		reader = command.ExecuteReader();
		//While the reader is still reading
		while (reader.Read())
		{
			//Select every 'socialClubName' from the table
			//string name = reader.GetString("social_club_name");
			//Check if the player with that name exists
			//API.consoleOutput(name);
			if (player.socialClubName == reader.GetString("social_club_name"))
			{
				//If a user exists with this name then close the connection and return true
				connection.Close();
				return true;
			}
		}
		connection.Close();
		return false;
	}

	public Boolean CheckPassword(Client player, string password)
	{
		var hashedPassword = API.getHashSHA256(password);

		connection = new MySqlConnection(myConnectionString);
		//Create a new command
		command = connection.CreateCommand();
		//Set the command - Create a query
		command.CommandText = "SELECT password FROM user WHERE social_club_name=@user";
		command.Parameters.AddWithValue("user", player.socialClubName);

		//Open connection
		connection.Open();
		//Tell the reader what to read
		var serverSavedPassword = command.ExecuteScalar();
		connection.Close();
		if (hashedPassword == serverSavedPassword.ToString())
			return true;
		else
			return false;

	}

	public void RegisterNewUser(Client player, string username, string password)
	{
		var hashedPassword = API.getHashSHA256(password);
		var skinID = -680474188;

		connection = new MySqlConnection(myConnectionString);
		connection.Open();
		command = connection.CreateCommand();
		//command.CommandText = "INSERT INTO user(social_club_name, password) VALUES('" + username + "','" + hashedPassword + "');";
		command.CommandText = "INSERT INTO user(social_club_name, username, password, skinID) VALUES(@social_club_name, @username, @hashedPassword, @skinID)";
		command.Parameters.AddWithValue("social_club_name", player.socialClubName);
		command.Parameters.AddWithValue("username", username);
		command.Parameters.AddWithValue("hashedPassword", hashedPassword);
		command.Parameters.AddWithValue("skinID", skinID);

		
		command.ExecuteNonQuery();

		connection.Close();
	}

	public PedHash GetSkinID(Client player)
	{
		connection = new MySqlConnection(myConnectionString);
		//Create a new command
		command = connection.CreateCommand();
		//Set the command - Create a query
		command.CommandText = "SELECT skinID FROM user WHERE social_club_name=@user";
		command.Parameters.AddWithValue("user", player.socialClubName);

		connection.Open();
		PedHash skinID = (PedHash)command.ExecuteScalar();
		connection.Close();

		return skinID;
	}

	


}