using BtcEurMarketDepth.Infrastructure.Transport.WebSockets;

namespace BtcEurMarketDepth.Tests
{
    public sealed class FakeClientWebSocketFactory(
    IWebSocketClient socket) : IClientWebSocketFactory
    {
        public IWebSocketClient Create()
        {
            return socket;
        }
    }
}
