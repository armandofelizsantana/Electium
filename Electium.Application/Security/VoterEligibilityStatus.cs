using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.Security;

public enum VoterEligibilityStatus
{
    InvalidIdentityNumber,
    NoActiveElection,
    CitizenNotFound,
    CitizenInactive,
    AlreadyParticipated,
    Eligible
}