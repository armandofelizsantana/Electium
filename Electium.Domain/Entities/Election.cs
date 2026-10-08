using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Enums;

namespace Electium.Domain.Entities
{
    public class Election : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public int Year { get; set; }

        public DateTime StartsAtUtc { get; set; }

        public DateTime? EndsAtUtc { get; set; }

        public ElectionStatus Status { get; set; } = ElectionStatus.Configuring;
    }
}
