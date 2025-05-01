using Android.Content;
using Android.Net;
using Domain.Interfaces;
using YourApp.Platforms.Android.Services;
using Application = Android.App.Application;

[assembly: Dependency(typeof(AndroidNetworkInfo))]
namespace YourApp.Platforms.Android.Services
{
    public class AndroidNetworkInfo : INetworkInfo
    {
        public string GetIpAddress()
        {
            try
            {
                var manager = (ConnectivityManager)Application.Context
                    .GetSystemService(Context.ConnectivityService);

                if (manager == null)
                    return string.Empty;

                var networks = manager.GetAllNetworks();

                foreach (var network in networks)
                {
                    var linkProperties = manager.GetLinkProperties(network);
                    if (linkProperties == null)
                        continue;

                    var ipAddress = linkProperties.LinkAddresses
                        .FirstOrDefault(addr => IsIPv4Address(addr?.Address))?
                        .Address?.ToString();

                    if (!string.IsNullOrEmpty(ipAddress))
                        return ipAddress.Split('%')[0]; // Remove IPv6 scope if present
                }
            }
            catch
            {
                // Handle any exceptions (like missing permissions)
            }

            return string.Empty;
        }
        private bool IsIPv4Address(Java.Net.InetAddress address)
        {
            if (address == null)
                return false;

            // Check if the address is an instance of Inet4Address
            return address is Java.Net.Inet4Address;
        }
    }
}