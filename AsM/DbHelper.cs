using System.Security.Cryptography;
using AsM.Components.Pages.Account;
using AsM.Models;
using Cassandra;

namespace AsM;

public class DatabaseService
{
    public Cluster Cluster { get; set; } = Cluster.Builder().AddContactPoint(Environment.GetEnvironmentVariable("IS_DOCKER") == "true" ? "cassandra" : "localhost").Build();
}


public static class DbHelper
{

    // TODO: Error Handling
    public static async Task<bool> InsertPassword(Guid id, string password)
    {
        var cluster = Cluster.Builder().AddContactPoint(Environment.GetEnvironmentVariable("IS_DOCKER") == "true" ? "cassandra" : "localhost").Build();
        var session = await cluster.ConnectAsync("accounts");

        byte[] salt;
        RandomNumberGenerator.Create().GetBytes(salt = new byte[16]);

        // Create the Rfc2898DeriveBytes instance
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);
        
        // Get the hash
        byte[] hash = pbkdf2.GetBytes(20);

        // Combine salt and hash
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);


        
        try
        {
            var statement = await session.PrepareAsync("INSERT INTO accounts.credentials (id, hash) VALUES (?, ?)");
            await session.ExecuteAsync(statement.Bind(id, Convert.ToBase64String(hashBytes)));
        }
        finally
        {
            await session.ShutdownAsync();
            await cluster.ShutdownAsync();
        }

        return true;
    }
    
    
    public static async Task<bool> CreateUser(Signup.SignupForm form)
    {
        var cluster = Cluster.Builder().AddContactPoint(Environment.GetEnvironmentVariable("IS_DOCKER") == "true" ? "cassandra" : "localhost").Build();
        var session = await cluster.ConnectAsync("accounts");

        try
        {
            var statement = await session.PrepareAsync("INSERT INTO accounts.users (id, dob, email, username, displayname) VALUES (?, ?, ?, ?, ?)");
            await session.ExecuteAsync(statement.Bind(form.Id, null, form.Email, form.Username, form.Displayname));
        }
        finally
        {
            await session.ShutdownAsync();
            await cluster.ShutdownAsync();
        }
        return true;
    }

    public static async Task<User?> GetUser(Guid id)
    {
        // If running in Docker, use cassandra as hostname, otherwise localhost
        var cluster = Cluster.Builder()
            .AddContactPoint(Environment.GetEnvironmentVariable("IS_DOCKER") == "true" ? "cassandra" : "localhost")
            .Build();

        var session = await cluster.ConnectAsync("accounts");
        try
        {
            var statement = await session.PrepareAsync("SELECT * FROM users WHERE id = ?");
            var res = (await session.ExecuteAsync(statement.Bind(id))).FirstOrDefault();

            if (res == null)
                return null;

            var user = new User()
            {
                Id = id,
                Dob = res.GetValue<LocalDate>("dob"),
                Email = res.GetValue<string>("email"),
                Username = res.GetValue<string>("username")
            };
            return user;
        }
        finally
        {
            await session.ShutdownAsync();
            await cluster.ShutdownAsync();
        }
    }

    public static async Task<List<User>?> GetUsers()
    {
        var cluster = Cluster.Builder().AddContactPoint(Environment.GetEnvironmentVariable("IS_DOCKER") == "true" ? "cassandra" : "localhost").Build();

        var session = await cluster.ConnectAsync("accounts");

        try
        {
            var statement = await session.PrepareAsync("SELECT * FROM users");
            var res = (await session.ExecuteAsync(statement.Bind()));

            if (res == null)
                return null;

            List<User> users = [];
            users.AddRange(res.Select(row => new User()
            {
                Id = row.GetValue<Guid>("id"), Dob = row.GetValue<LocalDate>("dob"),
                Email = row.GetValue<string>("email"), Username = row.GetValue<string>("username")
            }));
            return users;
        }
        finally
        {
            await session.ShutdownAsync();
            await cluster.ShutdownAsync();
        }
    }
}
