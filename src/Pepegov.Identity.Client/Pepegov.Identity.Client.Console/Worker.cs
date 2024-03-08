using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.Client;

namespace Pepegov.Identity.Client.Console;

public class Worker : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly OpenIddictClientService _service;
    private readonly ILogger<Worker> _logger;

    public Worker(IHostApplicationLifetime lifetime, OpenIddictClientService service, ILogger<Worker> logger)
    {
        _lifetime = lifetime;
        _service = service;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for the host to confirm that the application has started.
        var source = new TaskCompletionSource<bool>();
        await using (_lifetime.ApplicationStarted.Register(static state => ((TaskCompletionSource<bool>) state!).SetResult(true), source))
        {
            await source.Task;
        }
        _logger.LogDebug("Start worker");

        try
        {
            // Ask OpenIddict to send a device authorization request and write
            // the complete verification endpoint URI to the console output.
            var result = await _service.ChallengeUsingDeviceAsync(new()
            {
                CancellationToken = stoppingToken
            });

            if (result.VerificationUriComplete is not null)
            {
                _logger.LogInformation($"""
                    Please visit [link]{result.VerificationUriComplete}[/] and confirm the
                    displayed code is '{result.UserCode}' to complete the authentication demand.[/]
                    """);
            }

            else
            {
                _logger.LogInformation($"""
                                        Please visit [link]{result.VerificationUri}[/] and enter
                                        '{result.UserCode}' to complete the authentication demand.[/]
                                        """);
            }

            // Wait for the user to complete the demand on the other device.
            var principal = (await _service.AuthenticateWithDeviceAsync(new()
            {
                CancellationToken = stoppingToken,
                DeviceCode = result.DeviceCode,
                Interval = result.Interval,
                Timeout = result.ExpiresIn < TimeSpan.FromMinutes(5) ? result.ExpiresIn : TimeSpan.FromMinutes(5)
            })).Principal;

            _logger.LogInformation("[green]Authentication successful:[/]");
            foreach (var claim in principal.Claims)
            {
                _logger.LogDebug($"Claim type: {claim.Type} | Claim value type: {claim.ValueType} | Claim value: {claim.Value} ");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("The authentication process was aborted.[/]");
        }
        catch (OpenIddictExceptions.ProtocolException exception) when (exception.Error is OpenIddictConstants.Errors.AccessDenied)
        {
            _logger.LogError("The authorization was denied by the end user.[/]");
        }
        catch
        {
            _logger.LogError("An error occurred while trying to authenticate the user.[/]");
        }
    }
}