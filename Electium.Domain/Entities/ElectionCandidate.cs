using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Domain.Entities
{
    public class ElectionCandidate : BaseEntity
    {
        public int ElectionId { get; set; }

        public Election Election { get; set; } = null!;

        public int CandidateId { get; set; }

        public int PoliticalPartyId { get; set; }

        public int ElectivePositionId { get; set; }

        public string CandidateFirstName { get; set; } = string.Empty;

        public string CandidateLastName { get; set; } = string.Empty;

        public string CandidatePhotoPath { get; set; } = string.Empty;

        public string PartyName { get; set; } = string.Empty;

        public string PartyAcronym { get; set; } = string.Empty;

        public string PartyLogoPath { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;
    }
}
