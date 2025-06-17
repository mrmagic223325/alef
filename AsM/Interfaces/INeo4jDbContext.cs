using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace AsM.Interfaces;

/// <summary>
/// Interface for Neo4j graph database operations
/// </summary>
public interface INeo4jDbContext : IDisposable
{
    /// <summary>
    /// Gets the Neo4j driver
    /// </summary>
    IDriver Driver { get; }
    
    /// <summary>
    /// Executes a Cypher query and returns the results
    /// </summary>
    /// <typeparam name="T">The type to map the results to</typeparam>
    /// <param name="query">The Cypher query</param>
    /// <param name="parameters">The query parameters</param>
    /// <returns>The query results</returns>
    Task<IEnumerable<T>> ExecuteQueryAsync<T>(string query, object? parameters = null);
    
    /// <summary>
    /// Executes a Cypher query without returning results
    /// </summary>
    /// <param name="query">The Cypher query</param>
    /// <param name="parameters">The query parameters</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> ExecuteCommandAsync(string query, object? parameters = null);
}