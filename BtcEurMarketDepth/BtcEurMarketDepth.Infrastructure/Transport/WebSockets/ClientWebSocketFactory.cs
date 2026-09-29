using System.Net.WebSockets;

namespace BtcEurMarketDepth.Infrastructure.Transport.WebSockets
{
    public sealed class ClientWebSocketFactory
    : IClientWebSocketFactory
    {
        public IWebSocketClient Create()
        {
            return new ClientWebSocketAdapter();
        }
    }
}
