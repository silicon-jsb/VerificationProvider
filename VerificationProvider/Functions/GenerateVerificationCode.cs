using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VerificationProvider.Services;

namespace VerificationProvider.Functions;

public class GenerateVerificationCode(ILogger<GenerateVerificationCode> logger, IServiceProvider serviceProvider, IVerificationService verificationService)
{
	private readonly ILogger<GenerateVerificationCode> _logger = logger;
	private readonly IServiceProvider _serviceProvider = serviceProvider;
	private readonly IVerificationService _verificationService = verificationService;



	[Function(nameof(GenerateVerificationCode))]
	[ServiceBusOutput("email_request", Connection = "ServiceBus2Connection")]
	public async Task<string> Run([ServiceBusTrigger("verification_request", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
	{
		try
		{
			var verificationRequest = _verificationService.UnpackVerificationRquest(message);
			if (verificationRequest != null)
			{
				var code = _verificationService.GenerateCode();
				if (!string.IsNullOrEmpty(code))
				{
					var result = await _verificationService.SaveVerificationRequest(verificationRequest, code);
					if (result)
					{
						var emailRequest = _verificationService.GenerateEmailRequest(verificationRequest, code);
						if (emailRequest != null)
						{
							var payload = _verificationService.GenerateServiceBusEmailRequest(emailRequest);
							if (!string.IsNullOrEmpty(payload))
							{
								await messageActions.CompleteMessageAsync(message);
								return payload;
							}
						}
					}

				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError($"GenerateVerificationCode.Run() :: {ex.Message}");
		}
		return null!;
	}

}