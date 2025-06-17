using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AsM.Models;

namespace AsM.Interfaces;

/// <summary>
/// Interface for user-related operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets a user by their ID
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
    /// Creates a new user
    /// </summary>
    /// <param name="user">The user to create</param>
    /// <param name="password">The user's password</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> CreateUserAsync(User user, string password);
    
    /// <summary>
    /// Updates a user's settings
    /// </summary>
    /// <param name="id">The user's ID</param>
    /// <param name="settingType">The type of setting to update</param>
    /// <param name="value">The new value</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> UpdateUserSettingAsync(Guid id, string settingType, string value);
    
    /// <summary>
    /// Checks if a username is available
    /// </summary>
    /// <param name="username">The username to check</param>
    /// <returns>True if available, false otherwise</returns>
    Task<bool> IsUsernameAvailableAsync(string username);
    
    /// <summary>
    /// Checks if an email is available
    /// </summary>
    /// <param name="email">The email to check</param>
    /// <returns>True if available, false otherwise</returns>
    Task<bool> IsEmailAvailableAsync(string email);
}