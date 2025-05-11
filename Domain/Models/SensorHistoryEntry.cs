using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class SensorHistoryEntry
    {
        public DateTime Timestamp { get; set; }
        public float? Temperature { get; set; }
        public float? Humidity { get; set; }
        public float? Pressure { get; set; }
    }
}
