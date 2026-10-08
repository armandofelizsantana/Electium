using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Domain.Entities
{
    public class VoteSelection : BaseEntity
    {
        public int BallotId { get; set; }

        public Ballot Ballot { get; set; } = null!;

        public int ElectivePositionId { get; set; }

        public ElectivePosition ElectivePosition { get; set; } = null!;

        public int? ElectionCandidateId { get; set; }

        public ElectionCandidate? ElectionCandidate { get; set; }

        public bool IsNone { get; set; }
    }
}
