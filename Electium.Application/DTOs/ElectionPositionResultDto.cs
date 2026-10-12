namespace Electium.Application.DTOs;

public class ElectionPositionResultDto
{
    public int ElectivePositionId { get; set; }

    public string PositionName { get; set; } = string.Empty;

    // Incluye los votos a candidatos y los votos "Ninguno".
    public int TotalVotes { get; set; }

    public int NoneVotes { get; set; }

    public decimal NonePercentage { get; set; }

    public IReadOnlyList<ElectionCandidateResultDto> Candidates { get; set; }
        = Array.Empty<ElectionCandidateResultDto>();
}