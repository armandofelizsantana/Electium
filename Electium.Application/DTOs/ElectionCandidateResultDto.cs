namespace Electium.Application.DTOs;

public class ElectionCandidateResultDto
{
    public int ElectionCandidateId { get; set; }

    public string CandidateFirstName { get; set; } = string.Empty;

    public string CandidateLastName { get; set; } = string.Empty;

    public string CandidatePhotoPath { get; set; } = string.Empty;

    public string PartyName { get; set; } = string.Empty;

    public string PartyAcronym { get; set; } = string.Empty;

    public string PartyLogoPath { get; set; } = string.Empty;

    public int Votes { get; set; }

    public decimal Percentage { get; set; }
}