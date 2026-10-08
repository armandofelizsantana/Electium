using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Domain.Entities
{
    public class Citizen : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string IdentityNumber { get; set; } = string.Empty;
    }
}
