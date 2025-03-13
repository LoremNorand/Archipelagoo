using UnityEngine;
using Mono.Data.Sqlite;
using System.IO;
using System;

public class DbController : MonoBehaviour
{
    private const string fileName = "UsersDB.bytes";
    private string DBPath;

    void Start()
    {
        DBPath = GetDatabasePath();
    }
    public int SetPlayerId(string name)
    {
        using (var connection = new SqliteConnection("Data Source=" + DBPath))
        using (var command = connection.CreateCommand())
        {
            connection.Open();
            command.CommandText = "SELECT id FROM Players WHERE Name = @name LIMIT 1";
            command.Parameters.AddWithValue("@name", name);
            SqliteDataReader reader = command.ExecuteReader();
            return int.Parse(reader.GetValue(0).ToString());
            
        }
    }
    public bool ExaminationUser(string name)
    {
        try
        {
            using (var connection = new SqliteConnection("Data Source=" + DBPath))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "SELECT 1 FROM Players WHERE Name = @name LIMIT 1";
                command.Parameters.AddWithValue("@name", name);
                SqliteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    return false;
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка проверки пользователя: {ex}");
            return false;
        }
    }

    public bool AddUser(string name, string password)
    {
        try
        {
            using (var connection = new SqliteConnection("Data Source=" + DBPath))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "INSERT INTO Players (Name, Password) VALUES (@name, @password)";
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@password", password);

                return command.ExecuteNonQuery() > 0;
            }
        }
        catch (SqliteException ex)
        {
            Debug.LogError($"Ошибка добавления пользователя: {ex.Message}");
            return false;
        }
    }
    public bool PasswordChek(string name, string password)
    {
        try
        {
            using (var connection = new SqliteConnection("Data Source=" + DBPath))
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = "SELECT name, password FROM Players WHERE Name = @name LIMIT 1";
                command.Parameters.AddWithValue("@name", name);
                SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (password == reader.GetValue(1).ToString())
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка проверки пользователя: {ex}");
            return false;
        }
    }
    private string GetDatabasePath()
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);

        #if UNITY_EDITOR
        if (!File.Exists(path))
        {
            File.Copy(Path.Combine(Application.streamingAssetsPath, fileName), path);
        }
        #endif
        //Debug.Log(path);
        return path;
    }
}