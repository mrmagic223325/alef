using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AsM.Interfaces;
using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using Serilog;

namespace AsM.Data;

/// <summary>
/// Implementation of INeo4jDbContext for Neo4j graph database operations
/// </summary>
public class Neo4jDbContext : INeo4jDbContext
{
    /// <inheritdoc />
    public IDriver Driver { get; }

    /// <summary>
    /// Initializes a new instance of the Neo4jDbContext class
    /// </summary>
    /// <param name="configuration">The application configuration</param>
    public Neo4jDbContext(IConfiguration configuration)
    {
        try
        {
            Log.Information("Initializing Neo4j driver connection.");
            var url = Environment.GetEnvironmentVariable("IS_DOCKER") == "true"
                ? configuration["Neo4j:ProdUrl"]
                : configuration["Neo4j:LocalUrl"];

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("Neo4j URL is not configured.");
            }

            var username = configuration["Neo4j:Username"];
            var password = configuration["Neo4j:Password"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Neo4j credentials are not configured.");
            }

            Driver = GraphDatabase.Driver(url, AuthTokens.Basic(username, password));
            Log.Information("Successfully initialized Neo4j driver connection.");
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Failed to initialize Neo4j driver.");
            throw new Exception("Unrecoverable", e);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string query, object? parameters = null)
    {
        try
        {
            var session = Driver.AsyncSession();
            try
            {
                var result = await session.ExecuteReadAsync(async tx =>
                {
                    var cursor = await tx.RunAsync(query, parameters);
                    var records = await cursor.ToListAsync();
                    return records;
                });

                var list = new List<T>();
                foreach (var record in result)
                {
                    // This is a simplified implementation. In a real application,
                    // you would need to map the Neo4j records to your domain objects.
                    if (record[0] is T value)
                    {
                        list.Add(value);
                    }
                }

                return list;
            }
            finally
            {
                await session.CloseAsync();
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to execute Neo4j query: {Query}", query);
            return new List<T>();
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteCommandAsync(string query, object? parameters = null)
    {
        try
        {
            var session = Driver.AsyncSession();
            try
            {
                await session.ExecuteWriteAsync(async tx =>
                {
                    await tx.RunAsync(query, parameters);
                });
                return true;
            }
            finally
            {
                await session.CloseAsync();
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to execute Neo4j command: {Query}", query);
            return false;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Driver?.Dispose();
    }
}