using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Api.Core.Services
{
    public class SmsService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public SmsService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<bool> SendSmsAsync(string number, string data)
        {
            // Read API key and username from appsettings.json
            var apiKey = _configuration["AT:apiKey"];
            var username = _configuration["AT:username"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Error: Missing Africa's Talking API credentials.");
                return false;
            }

            // Construct the SMS message
            var smsMessage = $"famtrac\n\n{data}";

            // Prepare the API request data
            var values = new Dictionary<string, string>
            {
                { "username", username },
                { "to", number },
                { "message", smsMessage }
            };

            var content = new FormUrlEncodedContent(values);

            // Set API key in headers (correct way)
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apiKey", apiKey);

            try
            {
                // Send the HTTP POST request
                var response = await _httpClient.PostAsync("https://api.africastalking.com/version1/messaging", content);
                var responseString = await response.Content.ReadAsStringAsync();

                // Log full response details
                Console.WriteLine($"Status Code: {response.StatusCode}");
                Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType?.MediaType}");
                Console.WriteLine("Raw Response: " + responseString);

                // Check Content-Type to determine how to parse it
                var contentType = response.Content.Headers.ContentType?.MediaType;

                if (contentType == "application/json" || responseString.Trim().StartsWith("{"))
                {
                    try
                    {
                        // Parse JSON response
                        var jsonResponse = JObject.Parse(responseString);
                        var status = jsonResponse["SMSMessageData"]?["Recipients"]?[0]?["status"]?.ToString();

                        if (status == "Success")
                        {
                            Console.WriteLine("✅ SMS sent successfully!");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"❌ Failed to send SMS: {status}");
                            return false;
                        }
                    }
                    catch (Exception jsonEx)
                    {
                        Console.WriteLine($"⚠️ Error parsing JSON: {jsonEx.Message}");
                        return false;
                    }
                }
                else if (contentType == "text/xml" || responseString.Trim().StartsWith("<"))
                {
                    try
                    {
                        // Parse XML response
                        var xmlResponse = System.Xml.Linq.XDocument.Parse(responseString);
                        var status = xmlResponse.Descendants("status").FirstOrDefault()?.Value;

                        if (status == "Success")
                        {
                            Console.WriteLine("✅ SMS sent successfully!");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"❌ Failed to send SMS: {status}");
                            return false;
                        }
                    }
                    catch (Exception xmlEx)
                    {
                        Console.WriteLine($"⚠️ Error parsing XML: {xmlEx.Message}");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ Unexpected response format. Unable to parse.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ An error occurred while sending SMS: {ex.Message}");
                return false;
            }
        }
        }
    }
