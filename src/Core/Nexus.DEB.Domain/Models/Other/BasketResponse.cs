using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.DEB.Domain.Models.Other
{
    public class BasketResponse<T>
    {
        public List<T> BasketIds { get; set; } = new List<T>();
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = String.Empty;
    }
}
