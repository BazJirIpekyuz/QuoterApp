using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuoterApp.Caching
{
    public interface IDistributedCache<T>
    {
        Task<T?> GetAsync(string key);
        Task RemoveAsync(string key);
        Task SetAsync(string key, T item, int minutesToCache);
        Task<(bool Found, T? Value)> TryGetValueAsync(string key);
    }
}
