using System;
using System.Collections.Generic;
using System.Text;

namespace BtcEurMarketDepth.Domain.Model
{
    /// <summary>
    /// Represents the price and available quantity at one order-book level.
    /// </summary>
    public record PriceLevel
    {
        /// <summary>
        /// Gets the price of the level.
        /// </summary>
        public decimal Price { get; }

        /// <summary>
        /// Gets the quantity available at the level.
        /// </summary>
        public decimal Quantity { get; }

        /// <summary>
        /// Creates a valid price level.
        /// </summary>
        /// <param name="price">The price of the level.</param>
        /// <param name="quantity">The available quantity.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the price or quantity is zero or negative.
        /// </exception>
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
