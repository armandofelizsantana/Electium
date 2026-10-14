using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Electium.Application.DTOs;
using Electium.Application.Interfaces;
using Electium.Shared.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;


namespace Electium.Shared.Services;


public class MailKitVoteReceiptEmailSender : IVoteReceiptEmailSender
{
    private readonly SmtpEmailSettings _settings;
    private readonly ILogger<MailKitVoteReceiptEmailSender> _logger;

    public MailKitVoteReceiptEmailSender(
        SmtpEmailSettings settings,
        ILogger<MailKitVoteReceiptEmailSender> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task<bool> SendAsync(
        string recipientEmail,
        VoteReceiptDto receipt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.Host) ||
                _settings.Port <= 0 ||
                string.IsNullOrWhiteSpace(_settings.FromAddress))
            {
                throw new InvalidOperationException(
                    "La configuración SMTP está incompleta.");
            }

            if (!Enum.TryParse<SecureSocketOptions>(
                    _settings.SecureSocketOptions,
                    ignoreCase: true,
                    out var securityOption) ||
                securityOption == SecureSocketOptions.None)
            {
                throw new InvalidOperationException(
                    "La opción de seguridad SMTP no es válida.");
            }

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    _settings.FromName,
                    _settings.FromAddress));

            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject =
                $"Resumen de tu votación: {receipt.ElectionName}";

            var body = new StringBuilder();
            body.AppendLine("Tu votación se registró correctamente.");
            body.AppendLine();
            body.AppendLine($"Elección: {receipt.ElectionName}");
            body.AppendLine(
                "Fecha: " +
                receipt.SubmittedAtUtc.ToString(
                    "dd/MM/yyyy HH:mm 'UTC'",
                    CultureInfo.InvariantCulture));
            body.AppendLine();
            body.AppendLine("Resumen de tus selecciones:");

            foreach (var selection in receipt.SelectionSummary)
            {
                body.AppendLine($"- {selection}");
            }

            message.Body = new TextPart("plain")
            {
                Text = body.ToString()
            };

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                securityOption,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_settings.UserName))
            {
                if (string.IsNullOrWhiteSpace(_settings.Password))
                {
                    throw new InvalidOperationException(
                        "Falta la contraseña SMTP.");
                }

                await client.AuthenticateAsync(
                    _settings.UserName,
                    _settings.Password,
                    cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(
                quit: true,
                cancellationToken);

            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "No se pudo enviar el resumen de votación para la elección {ElectionId}.",
                receipt.ElectionId);

            return false;
        }
    }
}