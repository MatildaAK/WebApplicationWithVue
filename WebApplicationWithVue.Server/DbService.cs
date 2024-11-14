using Npgsql;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using WebApplicationWithVue.Server.Controllers;

namespace WebApplicationWithVue.Server;

    public class DbService : IDbService
    {
        private readonly NpgsqlConnection _connection;
        private readonly List<string> _refreshTokens = [];

    public DbService(IConfiguration configuration)
        {
            var _connectionString = configuration["ConnectionStrings:PostgreSQL"];
            _connection = new NpgsqlConnection(_connectionString);
        }

    public async Task<string> RegisterAccountAsync(LoginInformation loginInformation)
     {
            int rows = 0;
            string query = "INSERT INTO users (username, password) VALUES (@username, @password)";

        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();

        using (NpgsqlCommand command = new NpgsqlCommand(query, _connection))
        {
            command.Parameters.AddWithValue("@username", loginInformation.UserName);
            command.Parameters.AddWithValue("@password", PasswordHandler.Hash(loginInformation.Password));

            rows = await command.ExecuteNonQueryAsync();
        }
        return rows.ToString();
     }

    public async Task<bool> LoginAsync(string userName, string password)
    {
        // todo: hitta användare
        string query = "SELECT * FROM users WHERE username = @username";
        string dbPassword = String.Empty;

        if(_connection.State != ConnectionState.Open)
            await _connection.OpenAsync();

        using (NpgsqlCommand command = new NpgsqlCommand(query, _connection))
        {
            command.Parameters.AddWithValue("@username", userName);

            // todo: jämföra hash
            using (NpgsqlDataReader reader = await command.ExecuteReaderAsync()) {
                reader.Read();
                dbPassword = reader.GetString(reader.GetOrdinal("password"));
            }
        }

        return PasswordHandler.Verify(password, dbPassword);
    }

    public void SaveRefreshToken(string refreshToken)
    {
        _refreshTokens.Add(refreshToken);
    }

    public Task<bool> RefreshToken(string refreshToken)
    {
        Console.WriteLine(_refreshTokens.Count);
        foreach (var token in _refreshTokens)
        {
            Console.WriteLine(token);
        }
        return Task.FromResult(_refreshTokens.Contains(refreshToken));
    }
}

public static class PasswordHandler
{
    public static string Hash(string password)
    {    
        using SHA256 sha256Hash = SHA256.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = sha256Hash.ComputeHash(inputBytes);
        return Convert.ToHexString(hash);
    }

    // todo: Lösenords kontroll
    public static bool Verify(string password, string hash)
    {
        var hashedPassword = Hash(password);
        return hashedPassword == hash;
    }
}

