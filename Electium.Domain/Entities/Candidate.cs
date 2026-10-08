using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Domain.Entities
{

    public class Candidate : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string PhotoPath { get; set; } = string.Empty;

        public int PoliticalPartyId { get; set; }

        public PoliticalParty PoliticalParty { get; set; } = null!;
    }
}
