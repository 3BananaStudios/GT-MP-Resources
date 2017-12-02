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

	public Boolean PlayerExists(string username)
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
			if (username == reader.GetString("Username"))
			{
				//If a user exists with this name then close the connection and return true
				connection.Close();
				return true;
			}
		}
		connection.Close();
		return false;
	}

	public Boolean CheckPassword(string username, string password)
	{
		var hashedPassword = API.getHashSHA256(password);

		connection = new MySqlConnection(myConnectionString);
		//Create a new command
		command = connection.CreateCommand();
		//Set the command - Create a query
		command.CommandText = "SELECT Password FROM user WHERE Username=@user";
		command.Parameters.AddWithValue("user", username);

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

	public void RegisterNewUser(string username, string password)
	{
		var hashedPassword = API.getHashSHA256(password);
		var skinID = -680474188;

		connection = new MySqlConnection(myConnectionString);
		connection.Open();
		command = connection.CreateCommand();
		//command.CommandText = "INSERT INTO user(social_club_name, password) VALUES('" + username + "','" + hashedPassword + "');";
		command.CommandText = "INSERT INTO user(Username, Password, SkinID) VALUES(@username, @hashedPassword, @skinID)";
		command.Parameters.AddWithValue("username", username);
		command.Parameters.AddWithValue("hashedPassword", hashedPassword);
		command.Parameters.AddWithValue("skinID", skinID);

		
		command.ExecuteNonQuery();

		connection.Close();
	}

	public PedHash GetSkinID(string username)
	{
		connection = new MySqlConnection(myConnectionString);
		//Create a new command
		command = connection.CreateCommand();
		//Set the command - Create a query
		command.CommandText = "SELECT SkinID FROM user WHERE Username=@user";
		command.Parameters.AddWithValue("user", username);

		connection.Open();
		PedHash skinID = (PedHash)command.ExecuteScalar();
		connection.Close();

		return skinID;
	}

	


}