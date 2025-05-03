using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IDeviceRepository
    {
        Task<SmartDevice> AddAsync(SmartDevice device);
        Task<IEnumerable<SmartDevice>> GetAllAsync();
        Task<SmartDevice?> GetByIdAsync(Guid id);
        Task DeleteAsync(SmartDevice device);
    }
}
