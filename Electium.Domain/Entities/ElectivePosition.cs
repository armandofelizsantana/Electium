using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Domain.Entities
{
    public class ElectivePosition : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
