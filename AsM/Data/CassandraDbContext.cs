using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AsM.Interfaces;
using AsM.Models;
using Cassandra;
using Cassandra.Mapping;
using Microsoft.Extensions.Configuration;
using Serilog;
using ISession = Cassandra.ISession;

namespace AsM.Data;

/// <summary>
/// Implementation of ICassandraDbContext for Cassandra database operations
/// </summary>
public class CassandraDbContext : ICassandraDbContext
{
    private readonly IConfiguration _configuration;
    private readonly Cluster _cluster;

    /// <summary>
    /// Initializes a new instance of the CassandraDbContext class
    /// </summary>
    /// <param name="configuration">The application configuration</param>
    public CassandraDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
        try
        {
            Log.Information("Initializing Cassandra cluster connection.");
            var contactPoint = Environment.GetEnvironmentVariable("IS_DOCKER") == "true"
                ? _configuration["Cassandra:ProdUrl"]
                : _configuration["Cassandra:LocalUrl"];

            if (string.IsNullOrEmpty(contactPoint))
            {
                throw new ArgumentException("Cassandra URL is not configured.");
            }

            _cluster = Cluster.Builder().AddContactPoint(contactPoint).Build();
            Log.Information("Successfully initialized Cassandra cluster connection.");
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Failed to initialize Cassandra cluster.");
            throw new Exception("Unrecoverable", e);
        }
    }

    /// <inheritdoc />
    public async Task<ISession?> ConnectToKeyspaceAsync(string keyspace)
    {
        try
        {
            return await _cluster.ConnectAsync(keyspace);
        }
        catch (NoHostAvailableException e)
        {
            Log.Error(e, "Could not connect to Cassandra cluster keyspace '{Keyspace}'. No hosts available.", keyspace);
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e, "An unexpected error occurred while connecting to keyspace '{Keyspace}'.", keyspace);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<User?> GetUserAsync(Guid id)
    {
        try
        {
            using var session = await ConnectToKeyspaceAsync("accounts");
            if (session == null) return null;

            var statement = await session.PrepareAsync("SELECT * FROM users WHERE id = ?");
            var result = await session.ExecuteAsync(statement.Bind(id));
            var row = result.FirstOrDefault();

            if (row == null) return null;

            return new User
            {
                Id = id,
                Dob = row.GetValue<LocalDate?>("dob"),
                Email = row.GetValue<string>("email"),
                Username = row.GetValue<string>("username"),
                Displayname = row.GetValue<string>("displayname")
            };
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get user with ID {UserId}", id);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<List<User>?> GetUsersAsync()
    {
        try
        {
            using var session = await ConnectToKeyspaceAsync("accounts");
            if (session == null) return null;

            var statement = await session.PrepareAsync("SELECT * FROM users");
            var result = await session.ExecuteAsync(statement.Bind());

            if (result == null) return null;

            var users = new List<User>();
            foreach (var row in result)
            {
                users.Add(new User
                {
                    Id = row.GetValue<Guid>("id"),
                    Dob = row.GetValue<LocalDate?>("dob"),
                    Email = row.GetValue<string>("email"),
                    Username = row.GetValue<string>("username"),
                    Displayname = row.GetValue<string>("displayname")
                });
            }

            return users;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get users");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        try
        {
            using var session = await ConnectToKeyspaceAsync("accounts");
            if (session == null) return null;

            var mapper = new Mapper(session);
            return await mapper.SingleOrDefaultAsync<User>("SELECT * FROM users WHERE username = ? ALLOW FILTERING", username);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get user by username {Username}", username);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            using var session = await ConnectToKeyspaceAsync("accounts");
            if (session == null) return null;

            var mapper = new Mapper(session);
            return await mapper.SingleOrDefaultAsync<User>("SELECT * FROM users WHERE email = ? ALLOW FILTERING", email);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get user by email {Email}", email);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CreateUserAsync(User user)
    {
        try
        {
            using var session = await ConnectToKeyspaceAsync("accounts");
            if (session == null) return false;

            var statement = await session.PrepareAsync("INSERT INTO accounts.users (id, dob, email, username, displayname) VALUES (?, ?, ?, ?, ?)");
            await session.ExecuteAsync(statement.Bind(user.Id, user.Dob, user.Email, user.Username, user.Displayname));
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to create user {UserId}", user.Id);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateUserSettingAsync(Guid id, string settingType, string value)
    {
        try
        {
            using var session = await ConnectToKeyspaceAsync("accounts");
            if (session == null) return false;

            string updateColumn;
            switch (settingType)
            {
                case "Displayname":
                    updateColumn = "displayname";
                    break;
                case "Username":
                    updateColumn = "username";
                    break;
                case "Email":
                    updateColumn = "email";
                    break;
                default:
                    return false;
            }

            var statement = await session.PrepareAsync($"UPDATE users SET {updateColumn} = ? WHERE id = ?");
            await session.ExecuteAsync(statement.Bind(value, id));
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to update user setting {SettingType} for user {UserId}", settingType, id);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UsernameExistsAsync(string username)
    {
        try
        {
            using var session = await ConnectToKeyspaceAsync("accounts");
            if (session == null) return false;

            var statement = await session.PrepareAsync("SELECT count(*) FROM accounts.users WHERE username = ? ALLOW FILTERING");
            var result = await session.ExecuteAsync(statement.Bind(username));
            return result.First().GetValue<long>("count") > 0;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to check if username {Username} exists", username);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            using var session = await ConnectToKeyspaceAsync("accounts");
            if (session == null) return false;

            var statement = await session.PrepareAsync("SELECT count(*) FROM accounts.users WHERE email = ? ALLOW FILTERING");
            var result = await session.ExecuteAsync(statement.Bind(email));
            return result.First().GetValue<long>("count") > 0;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to check if email {Email} exists", email);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetPasswordHashAsync(Guid id)
    {
        try
        {
            using var session = await ConnectToKeyspaceAsync("accounts");
            if (session == null) return null;

            var statement = await session.PrepareAsync("SELECT hash FROM credentials WHERE id = ?");
            var result = await session.ExecuteAsync(statement.Bind(id));
            return result.FirstOrDefault()?.GetValue<string>("hash");
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get password hash for user {UserId}", id);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SetPasswordHashAsync(Guid id, string hash)
    {
        try
        {
            using var session = await ConnectToKeyspaceAsync("accounts");
            if (session == null) return false;

            var statement = await session.PrepareAsync("INSERT INTO credentials (id, hash) VALUES (?, ?)");
            await session.ExecuteAsync(statement.Bind(id, hash));
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to set password hash for user {UserId}", id);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> StoreEmailVerificationCodeAsync(string email, string code)
    {
        try
        {
            using var session = await ConnectToKeyspaceAsync("accounts");
            if (session == null) return false;

            var statement = await session.PrepareAsync("INSERT INTO accounts.email_verification (id, email) VALUES (?, ?)");
            await session.ExecuteAsync(statement.Bind(code, email));
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to store email verification code for {Email}", email);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetEmailVerificationCodeAsync(string email)
    {
        try
        {
            using var session = await ConnectToKeyspaceAsync("accounts");
            if (session == null) return null;

            var statement = await session.PrepareAsync("SELECT id FROM email_verification WHERE email = ?");
            var result = await session.ExecuteAsync(statement.Bind(email));
            return result.FirstOrDefault()?.GetValue<string>("id");
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get email verification code for {Email}", email);
            return null;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cluster?.Dispose();
    }
}