using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class SmartDevice
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UID { get; set; }
        public List<string> Topics { get; set; } = new List<string>();

    }
}
