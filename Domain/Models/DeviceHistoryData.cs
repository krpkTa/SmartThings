using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class DeviceHistoryData
    {
        public string DeviceId { get; set; }
        public List<SensorHistoryEntry> History { get; set; } = new();
    }
}
