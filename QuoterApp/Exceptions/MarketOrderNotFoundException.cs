using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuoterApp.Exceptions
{
    internal class MarketOrderNotFoundException : Exception
    {
        public MarketOrderNotFoundException(string message) : base(message)
        {
        }
    }
}
