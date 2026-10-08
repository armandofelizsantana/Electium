using System;
using System.Collections.Generic;
using System.Text;

namespace Electium.Domain.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
