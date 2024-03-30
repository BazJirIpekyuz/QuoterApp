using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuoterApp.Tests.Shared
{
    internal interface IMarketOrderTestDataSource : IMarketOrderSource
    {
        List<MarketOrder> GetData();
    }
}
