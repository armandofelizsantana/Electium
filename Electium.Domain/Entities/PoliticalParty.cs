using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Domain.Entities
{
    public class PoliticalParty : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Acronym { get; set; } = string.Empty;

        public string LogoPath { get; set; } = string.Empty;
    }
}
