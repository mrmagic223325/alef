using System;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AsM.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace AsM.Services;

/// <summary>
/// Implementation of IEmailService for email-related operations
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ICassandraDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the EmailService class
    /// </summary>
    /// <param name="configuration">The application configuration</param>
    /// <param name="dbContext">The Cassandra database context</param>
    public EmailService(IConfiguration configuration, ICassandraDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<bool> SendEmailAsync(string to, string toName, string subject, string body, bool isHtml = true)
    {
        try
        {
            using var client = new SmtpClient
            {
                Host = _configuration["Mail:Host"] ?? throw new ArgumentNullException("Mail:Host"),
                Port = int.Parse(_configuration["Mail:Port"] ?? throw new ArgumentNullException("Mail:Port")),
                EnableSsl = bool.Parse(_configuration["Mail:EnableSsl"] ?? "false"),
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    _configuration["Mail:Credentials:Username"] ?? throw new ArgumentNullException("Mail:Credentials:Username"),
                    _configuration["Mail:Credentials:Password"] ?? throw new ArgumentNullException("Mail:Credentials:Password"))
            };

            var from = new MailAddress(
                _configuration["Mail:Address"] ?? throw new ArgumentNullException("Mail:Address"),
                _configuration["Mail:Name"] ?? throw new ArgumentNullException("Mail:Name"));
            
            var toAddress = new MailAddress(to, toName);
            
            using var message = new MailMessage(from, toAddress)
            {
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                Body = body,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = isHtml,
                Priority = MailPriority.Normal
            };

            await client.SendMailAsync(message);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to send email to {Recipient}", to);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendVerificationEmailAsync(string to, string toName, string verificationCode)
    {
        try
        {
            // Store the verification code in the database
            await _dbContext.StoreEmailVerificationCodeAsync(to, verificationCode);
            
            // Create a simple HTML email body with the verification code
            var body = $@"
                <html>
                <body>
                    <h2>Email Verification</h2>
                    <p>Thank you for registering with AsM. Please use the following code to verify your email address:</p>
                    <h3>{verificationCode}</h3>
                    <p>If you did not request this verification, please ignore this email.</p>
                </body>
                </html>";

            return await SendEmailAsync(to, toName, "Confirm Your AsM Account", body);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to send verification email to {Recipient}", to);
            return false;
        }
    }

    /// <inheritdoc />
    public string GenerateVerificationCode(int length = 8)
    {
        var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[length];
        var random = RandomNumberGenerator.Create();

        var bytes = new byte[length];
        random.GetBytes(bytes);

        for (var i = 0; i < length; i++)
        {
            stringChars[i] = chars[bytes[i] % chars.Length];
        }

        return new string(stringChars);
    }
}