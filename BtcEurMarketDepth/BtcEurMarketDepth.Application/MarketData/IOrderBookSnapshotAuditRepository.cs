using BtcEurMarketDepth.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace BtcEurMarketDepth.Application.MarketData
{
    public interface IOrderBookSnapshotAuditRepository
    {
        Task SaveAsync(OrderBookSnapshot snapshot, CancellationToken cancellationToken = default);
    }
}
