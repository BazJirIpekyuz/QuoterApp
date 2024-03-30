using System;

namespace QuoterApp
{
    /// <summary>
    /// Interface to access market orders
    /// </summary>
    public interface IMarketOrderSource
    {
        /// <summary>
        /// Blocking method that will return next available market order.
        /// </summary>
        /// <returns>Market order containing InstrumentId, Price and Quantity</returns>
        public MarketOrder GetNextMarketOrder();
    }
}
