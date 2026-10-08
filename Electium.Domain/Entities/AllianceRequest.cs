using System;
using System.Collections.Generic;
using System.Text;
using Electium.Domain.Enums;


namespace Electium.Domain.Entities
{
    public class AllianceRequest : BaseEntity
    {
        public int RequestingPartyId { get; set; }

        public PoliticalParty RequestingParty { get; set; } = null!;

        public int ReceivingPartyId { get; set; }

        public PoliticalParty ReceivingParty { get; set; } = null!;

        public AllianceRequestStatus Status { get; set; } =
            AllianceRequestStatus.Pending;

        public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAtUtc { get; set; }
    }
}
