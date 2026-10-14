using Electium.Application.DTOs;

namespace Electium.Application.Interfaces;

public interface IVoteReceiptEmailSender
{
    Task<bool> SendAsync(
        string recipientEmail,
        VoteReceiptDto receipt,
        CancellationToken cancellationToken = default);
}