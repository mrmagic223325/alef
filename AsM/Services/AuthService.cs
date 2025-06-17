using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AsM.Interfaces;
using AsM.Models;
using Serilog;

namespace AsM.Services;

/// <summary>
/// Implementation of IAuthService for authentication-related operations
/// </summary>
public class AuthService : IAuthService
{
    private readonly ICassandraDbContext _dbContext;
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of the AuthService class
    /// </summary>
    /// <param name="dbContext">The Cassandra database context</param>
    /// <param name="emailService">The email service</param>
    public AuthService(ICassandraDbContext dbContext, IEmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }

    /// <inheritdoc />
    public async Task<User?> AuthenticateAsync(string account, string password)
    {
        try
        {
            // Determine if the account is an email or username
            var isEmail = account.Contains('@');
            
            // Get the user by email or username
            var user = isEmail 
                ? await _dbContext.GetUserByEmailAsync(account) 
                : await _dbContext.GetUserByUsernameAsync(account);
            
            if (user?.Id == null)
            {
                return null;
            }
            
            // Check if the password is valid
            var isValid = await CheckPasswordAsync(user.Id.Value, password);
            
            return isValid ? user : null;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to authenticate user with account {Account}", account);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CheckPasswordAsync(Guid userId, string password)
    {
        try
        {
            // Get the password hash from the database
            var hash = await _dbContext.GetPasswordHashAsync(userId);
            
            if (string.IsNullOrEmpty(hash))
            {
                return false;
            }
            
            // Convert the hash from base64
            byte[] hashBytes = Convert.FromBase64String(hash);
            
            // Extract the salt (first 16 bytes)
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            
            // Generate a hash from the provided password using the same salt
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);
            byte[] computedHash = pbkdf2.GetBytes(20);
            
            // Compare the computed hash with the stored hash
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != computedHash[i])
                {
                    return false;
                }
            }
            
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to check password for user {UserId}", userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SetPasswordAsync(Guid userId, string password)
    {
        try
        {
            // Generate a random salt
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);
            
            // Generate a hash from the password using PBKDF2
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);
            byte[] hash = pbkdf2.GetBytes(20);
            
            // Combine the salt and hash
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            
            // Convert to base64 and store in the database
            string base64Hash = Convert.ToBase64String(hashBytes);
            return await _dbContext.SetPasswordHashAsync(userId, base64Hash);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to set password for user {UserId}", userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendVerificationEmailAsync(string email, string displayName)
    {
        try
        {
            // Generate a verification code
            var code = _emailService.GenerateVerificationCode();
            
            // Send the verification email
            return await _emailService.SendVerificationEmailAsync(email, displayName, code);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to send verification email to {Email}", email);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> VerifyEmailAsync(string email, string code)
    {
        try
        {
            // Get the stored verification code
            var storedCode = await _dbContext.GetEmailVerificationCodeAsync(email);
            
            // Check if the codes match
            return !string.IsNullOrEmpty(storedCode) && storedCode == code;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to verify email {Email}", email);
            return false;
        }
    }
}