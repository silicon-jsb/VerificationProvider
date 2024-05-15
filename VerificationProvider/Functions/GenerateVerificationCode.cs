using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Models;

namespace VerificationProvider.Functions;

public class GenerateVerificationCode(ILogger<GenerateVerificationCode> logger, IServiceProvider serviceProvider)
{
    private readonly ILogger<GenerateVerificationCode> _logger = logger;
	private readonly IServiceProvider _serviceProvider = serviceProvider;

	[Function(nameof(GenerateVerificationCode))]
	[ServiceBusOutput("email_request", Connection = "ServiceBusConnection")]
    public async Task Run([ServiceBusTrigger("verification_request", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message,ServiceBusMessageActions messageActions)
    {
		try
		{
			var verificationRequest = UnpackVerificationRquest(message);
			if (verificationRequest != null)
			{
				var code = GenerateCode();
				if (!string.IsNullOrEmpty(code))
				{
					var emailRequest = GenerateEmailRequest(verificationRequest, code);
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError($"GenerateVerificationCode.Run() :: {ex.Message}");
		}
	}

	public VerificationRequest UnpackVerificationRquest(ServiceBusReceivedMessage message)
	{
		try
		{
			var verificationRequest = JsonConvert.DeserializeObject<VerificationRequest>(message.Body.ToString());
			if (verificationRequest != null && !string.IsNullOrEmpty(verificationRequest.Email))
				return verificationRequest; 
			
		}
		catch (Exception ex)
		{
			_logger.LogError($"GenerateVerificationCode.UnpackVerificationRquest :: {ex.Message}");
		}
		return null!;
	}

	public string GenerateCode()
	{
		try
		{
			var rnd = new Random();
			var code = rnd.Next(100000, 999999);

			return code.ToString();
		}
		catch (Exception ex)
		{
			_logger.LogError($"GenerateVerificationCode.GenerateCode :: {ex.Message}");
		}
		return null!;
	}

	private EmailRequest GenerateEmailRequest(VerificationRequest verificationRequest, string code)
	{
		try
		{
			if (!string.IsNullOrEmpty(verificationRequest.Email) && !string.IsNullOrEmpty(code))
			{
				var emailRequest = new EmailRequest()
				{
					To = verificationRequest.Email,
					Subject = $"Verification Code {code}",
					HtmlBody = $@"
					<html lang='en'>
						<head>
							<meta charset='UTF-8'>
							<meta name='viewport' content='width=device-width, initial-scale=1.0'>
							<title>Verification Code</title>
						</head>
						<body>
						<div style='color: #191919; max-width: 500px'>
							<h1 style='font-weight: 400;'>Verification Code</h1>
						</div>

		

					"
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError($"GenerateVerificationCode.GenerateEmailRequest :: {ex.Message}");
		}
		return null!;
	}

}
