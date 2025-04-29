using Domain.Interfaces;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
namespace Infrastructure.Services
{
    public class NetworkDiscoveryService : INetworkDiscoveryService
    {
        public async Task<string> GetLocalNetworkIpAsync()
        {
            try
            {
                Debug.WriteLine("Попытка получить IP через socket...");
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                await socket.ConnectAsync("8.8.8.8", 65530);

                if (socket.LocalEndPoint is IPEndPoint endPoint)
                {
                    var ip = endPoint.Address.ToString();
                    Debug.WriteLine($"Успешно получен IP: {ip}");
                    return ip;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка socket метода: {ex.Message}");
            }

            try
            {
                Debug.WriteLine("Попытка получить IP через fallback метод...");
                var host = Dns.GetHostEntry(Dns.GetHostName());
                var ip = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "127.0.0.1";
                Debug.WriteLine($"Fallback IP: {ip}");
                return ip;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка fallback метода: {ex.Message}");
                return "Не удалось определить IP";
            }
        }
    }
}
