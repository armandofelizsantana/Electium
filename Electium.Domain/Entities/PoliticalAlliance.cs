using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Domain.Entities
{
    public class PoliticalAlliance : BaseEntity
    {
        public int FirstPartyId { get; set; }

        public PoliticalParty FirstParty { get; set; } = null!;

        public int SecondPartyId { get; set; }

        public PoliticalParty SecondParty { get; set; } = null!;

        public DateTime AcceptedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
