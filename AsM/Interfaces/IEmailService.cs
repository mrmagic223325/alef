using System.Threading.Tasks;

namespace AsM.Interfaces;

/// <summary>
/// Interface for email-related operations
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="toName">Recipient name</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <param name="isHtml">Whether the body is HTML</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SendEmailAsync(string to, string toName, string subject, string body, bool isHtml = true);
    
    /// <summary>
    /// Sends a verification email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="toName">Recipient name</param>
    /// <param name="verificationCode">The verification code</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SendVerificationEmailAsync(string to, string toName, string verificationCode);
    
    /// <summary>
    /// Generates a random verification code
    /// </summary>
    /// <param name="length">The length of the code</param>
    /// <returns>A random verification code</returns>
    string GenerateVerificationCode(int length = 8);
}