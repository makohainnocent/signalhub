using Application.Common.Abstractions;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly string _apiSecret;

        public EmailService(IConfiguration configuration)
        {
            _apiKey = configuration["Mailjet:ApiKey"] ?? throw new ArgumentNullException(nameof(configuration), "Mailjet API Key is missing.");
            _apiSecret = configuration["Mailjet:ApiSecret"] ?? throw new ArgumentNullException(nameof(configuration), "Mailjet API Secret is missing.");
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email address cannot be null or empty.", nameof(email));
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Subject cannot be null or empty.", nameof(subject));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            // Initialize Mailjet client
            var client = new MailjetClient(_apiKey, _apiSecret);

            // Create the recipient contact
            var toContact = new SendContact(email);
            if (toContact == null)
                throw new InvalidOperationException("Failed to create SendContact for the recipient.");

            // Create the email message
            var emailMessage = new TransactionalEmail
            {
                From = new SendContact("noreply@yourapp.com", "Your App"),
                To = { toContact },
                Subject = subject,
                TextPart = message,
                HTMLPart = message
            };

            // Send the email
            var response = await client.SendTransactionalEmailAsync(emailMessage).ConfigureAwait(false);

            // Validate the response
            if (response == null)
                throw new InvalidOperationException("Mailjet API returned a null response.");

            if (response.Messages == null || !response.Messages.Any())
                throw new InvalidOperationException("Mailjet API returned no messages in the response.");

            // Check for failed messages
            var failedMessages = response.Messages.Where(msg => msg.Status != "success").ToList();
            if (failedMessages.Any())
            {
                var errorDetails = string.Join(", ", failedMessages.Select(msg => msg.Errors?.FirstOrDefault()?.ErrorMessage ?? "Unknown error"));
                throw new InvalidOperationException($"Failed to send email. Errors: {errorDetails}");
            }
        }
    }
}