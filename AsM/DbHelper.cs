using AsM.Models;
using Cassandra;
using ISession = Cassandra.ISession;

namespace AsM;

public static class DbHelper
{
    public static async Task<User?> GetUser(Guid id)
    {
        // If running in Docker use cassandra as hostname, otherwise localhost
        var cluster = Cluster.Builder().AddContactPoint(Environment.GetEnvironmentVariable("IS_DOCKER") == "true" ? "cassandra" : "localhost").Build();

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
