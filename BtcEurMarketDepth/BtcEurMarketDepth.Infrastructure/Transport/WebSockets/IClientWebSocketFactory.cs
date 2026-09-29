namespace BtcEurMarketDepth.Infrastructure.Transport.WebSockets
{
    public interface IClientWebSocketFactory
    {
        IWebSocketClient Create();
    }
}
