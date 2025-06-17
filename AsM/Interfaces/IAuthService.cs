using System;
using System.Threading.Tasks;
using AsM.Models;

namespace AsM.Interfaces;

/// <summary>
/// Interface for authentication-related operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with username/email and password
    /// </summary>
    /// <param name="account">Username or email</param>
    /// <param name="password">Password</param>
    /// <returns>The authenticated user if successful, null otherwise</returns>
    Task<User?> AuthenticateAsync(string account, string password);
    
    /// <summary>
    /// Checks if a password is valid for a user
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="password">The password to check</param>
    /// <returns>True if valid, false otherwise</returns>
    Task<bool> CheckPasswordAsync(Guid userId, string password);
    
    /// <summary>
    /// Creates or updates a password for a user
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="password">The new password</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SetPasswordAsync(Guid userId, string password);
    
    /// <summary>
    /// Sends an email verification code
    /// </summary>
    /// <param name="email">The email to verify</param>
    /// <param name="displayName">The user's display name</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SendVerificationEmailAsync(string email, string displayName);
    
    /// <summary>
    /// Verifies an email with a verification code
    /// </summary>
    /// <param name="email">The email to verify</param>
    /// <param name="code">The verification code</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> VerifyEmailAsync(string email, string code);
}