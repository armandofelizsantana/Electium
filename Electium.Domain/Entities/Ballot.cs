using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Domain.Entities
{
    public class Ballot : BaseEntity
    {
        public int ElectionId { get; set; }

        public Election Election { get; set; } = null!;

        public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
