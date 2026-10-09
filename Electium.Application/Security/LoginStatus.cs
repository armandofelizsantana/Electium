using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Application.Security;
public enum LoginStatus
{
    Success = 1,
    InvalidCredentials = 2,
    Inactive = 3,
    PartyNotAssigned = 4,
    UnauthorizedRole = 5
}