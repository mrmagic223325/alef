using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AsM.Models;
using Cassandra;
using ISession = Cassandra.ISession;

namespace AsM.Interfaces;

/// <summary>
/// Interface for Cassandra database operations
/// </summary>
public interface ICassandraDbContext : IDisposable
{
    /// <summary>
    /// Gets a session connected to the specified keyspace
    /// </summary>
    /// <param name="keyspace">The keyspace to connect to</param>
    /// <returns>A session if successful, null otherwise</returns>
    Task<ISession?> ConnectToKeyspaceAsync(string keyspace);
    
    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">The user's ID</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetUserAsync(Guid id);
    
    /// <summary>
    /// Gets all users
    /// </summary>
    /// <returns>A list of all users</returns>
    Task<List<User>?> GetUsersAsync();
    
    /// <summary>
    /// Gets a user by username
    /// </summary>
    /// <param name="username">The username to search for</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetUserByUsernameAsync(string username);
    
    /// <summary>
    /// Gets a user by email
    /// </summary>
    /// <param name="email">The email to search for</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetUserByEmailAsync(string email);
    
    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="user">The user to create</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> CreateUserAsync(User user);
    
    /// <summary>
    /// Updates a user setting
    /// </summary>
    /// <param name="id">The user's ID</param>
    /// <param name="settingType">The type of setting to update</param>
    /// <param name="value">The new value</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> UpdateUserSettingAsync(Guid id, string settingType, string value);
    
    /// <summary>
    /// Checks if a username exists
    /// </summary>
    /// <param name="username">The username to check</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> UsernameExistsAsync(string username);
    
    /// <summary>
    /// Checks if an email exists
    /// </summary>
    /// <param name="email">The email to check</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> EmailExistsAsync(string email);
    
    /// <summary>
    /// Gets the password hash for a user
    /// </summary>
    /// <param name="id">The user's ID</param>
    /// <returns>The password hash if found, null otherwise</returns>
    Task<string?> GetPasswordHashAsync(Guid id);
    
    /// <summary>
    /// Sets the password hash for a user
    /// </summary>
    /// <param name="id">The user's ID</param>
    /// <param name="hash">The password hash</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SetPasswordHashAsync(Guid id, string hash);
    
    /// <summary>
    /// Stores an email verification code
    /// </summary>
    /// <param name="email">The email to verify</param>
    /// <param name="code">The verification code</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> StoreEmailVerificationCodeAsync(string email, string code);
    
    /// <summary>
    /// Gets the verification code for an email
    /// </summary>
    /// <param name="email">The email</param>
    /// <returns>The verification code if found, null otherwise</returns>
    Task<string?> GetEmailVerificationCodeAsync(string email);
}