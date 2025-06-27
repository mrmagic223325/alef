using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AsM.Interfaces;
using AsM.Models;
using Serilog;

namespace AsM.Services;

/// <summary>
/// Implementation of IUserService for user-related operations
/// </summary>
public class UserService : IUserService
{
    private readonly ICassandraDbContext _dbContext;
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the UserService class
    /// </summary>
    /// <param name="dbContext">The Cassandra database context</param>
    /// <param name="authService">The authentication service</param>
    public UserService(ICassandraDbContext dbContext, IAuthService authService)
    {
        _dbContext = dbContext;
        _authService = authService;
    }

    /// <inheritdoc />
    public async Task<User?> GetUserAsync(Guid id)
    {
        try
        {
            return await _dbContext.GetUserAsync(id);
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
            return await _dbContext.GetUsersAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get users");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CreateUserAsync(User user, string password)
    {
        try
        {
            // Check if username and email are available
            if (!await IsUsernameAvailableAsync(user.Username ?? string.Empty))
            {
                Log.Warning("Username {Username} is already taken", user.Username);
                return false;
            }

            if (!await IsEmailAvailableAsync(user.Email ?? string.Empty))
            {
                Log.Warning("Email {Email} is already in use", user.Email);
                return false;
            }

            // Ensure the user has an ID
            if (user.Id == null)
            {
                user.Id = Guid.NewGuid();
            }

            // Create the user
            var userCreated = await _dbContext.CreateUserAsync(user);
            if (!userCreated)
            {
                return false;
            }

            // Set the user's password
            var passwordSet = await _authService.SetPasswordAsync(user.Id.Value, password);
            if (!passwordSet)
            {
                Log.Error("Failed to set password for user {UserId}", user.Id);
                // TODO: Consider deleting the user if password setting fails
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to create user {Username}", user.Username);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateUserSettingAsync(Guid id, string settingType, string value)
    {
        try
        {
            // Validate the setting type
            if (settingType != "Displayname" && settingType != "Username" && settingType != "Email")
            {
                Log.Warning("Invalid setting type: {SettingType}", settingType);
                return false;
            }

            // If updating username or email, check availability
            if (settingType == "Username" && !await IsUsernameAvailableAsync(value))
            {
                Log.Warning("Username {Username} is already taken", value);
                return false;
            }

            if (settingType == "Email" && !await IsEmailAvailableAsync(value))
            {
                Log.Warning("Email {Email} is already in use", value);
                return false;
            }

            // Update the setting
            return await _dbContext.UpdateUserSettingAsync(id, settingType, value);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to update {SettingType} for user {UserId}", settingType, id);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        try
        {
            return !await _dbContext.UsernameExistsAsync(username);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to check if username {Username} is available", username);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        try
        {
            return !await _dbContext.EmailExistsAsync(email);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to check if email {Email} is available", email);
            return false;
        }
    }
}