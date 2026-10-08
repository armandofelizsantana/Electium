using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Domain.Entities
{
    public class CandidateNomination : BaseEntity
    {
        public int CandidateId { get; set; }

        public Candidate Candidate { get; set; } = null!;

        public int PoliticalPartyId { get; set; }

        public PoliticalParty PoliticalParty { get; set; } = null!;

        public int ElectivePositionId { get; set; }

        public ElectivePosition ElectivePosition { get; set; } = null!;
    }
}
