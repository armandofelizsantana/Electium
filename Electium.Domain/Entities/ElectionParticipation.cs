using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Domain.Entities
{
    public class ElectionParticipation : BaseEntity
    {
        public int ElectionId { get; set; }

        public Election Election { get; set; } = null!;

        public int CitizenId { get; set; }

        public Citizen Citizen { get; set; } = null!;

        public DateTime ParticipatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
