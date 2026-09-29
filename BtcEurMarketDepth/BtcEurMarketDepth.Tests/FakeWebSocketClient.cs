using System.Net.WebSockets;
using System.Text;

using BtcEurMarketDepth.Infrastructure.Transport.WebSockets;

namespace BtcEurMarketDepth.Tests
{
    public sealed class FakeWebSocketClient : IWebSocketClient
    {
        private readonly Queue<FakeMessage> messages = new();

        public List<string> SentMessages { get; } = [];

        public WebSocketState State { get; private set; }
            = WebSocketState.None;

        public void EnqueueTextMessage(string message)
        {
            messages.Enqueue(
                new FakeMessage(
                    Encoding.UTF8.GetBytes(message),
                    WebSocketMessageType.Text));
        }

        public void EnqueueCloseMessage()
        {
            messages.Enqueue(
                new FakeMessage(
                    [],
                    WebSocketMessageType.Close));
        }

        public Task ConnectAsync(
            Uri uri,
            CancellationToken cancellationToken)
        {
            State = WebSocketState.Open;
            return Task.CompletedTask;
        }

        public ValueTask SendAsync(
            ReadOnlyMemory<byte> buffer,
            WebSocketMessageType messageType,
            bool endOfMessage,
            CancellationToken cancellationToken)
        {
            SentMessages.Add(
                Encoding.UTF8.GetString(buffer.Span));

            return ValueTask.CompletedTask;
        }

        public ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken)
        {
            if (messages.Count == 0)
            {
                State = WebSocketState.Closed;

                return ValueTask.FromResult(
                    new ValueWebSocketReceiveResult(
                        0,
                        WebSocketMessageType.Close,
                        true));
            }

            var message = messages.Dequeue();

            message.Payload.CopyTo(buffer);

            if (message.MessageType == WebSocketMessageType.Close)
            {
                State = WebSocketState.Closed;
            }

            return ValueTask.FromResult(
                new ValueWebSocketReceiveResult(
                    message.Payload.Length,
                    message.MessageType,
                    true));
        }

        public void Dispose()
        {
            State = WebSocketState.Closed;
        }

        private sealed record FakeMessage(
            byte[] Payload,
            WebSocketMessageType MessageType);
    }
}
