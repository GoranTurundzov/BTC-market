using System;
using System.Collections.Generic;
using System.Text;

namespace BtcEurMarketDepth.Domain.Model
{
    public record PriceLevel
    {
        public decimal Price { get; }
        public decimal Quantity { get; }

        public PriceLevel(decimal price, decimal quantity)
        {
            if (price <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(price), "Price must be greater than zero.");
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            }

            Price = price;
            Quantity = quantity;
        }
    }
}
