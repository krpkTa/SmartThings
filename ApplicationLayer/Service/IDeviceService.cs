using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Service
{
    public interface IDeviceService
    {
        Task<SmartDevice> AddDeviceAsync(string name, string uid, string topic);
        Task<IEnumerable<SmartDevice>> GetAllDevicesAsync();
        Task<SmartDevice?> GetDeviceByIdAsync(Guid id);
        Task<bool> DeleteDeviceAsync(Guid id);
    }
}
