using CacheInterceptor;
using Examples.Models;
using System.Threading;

namespace Examples
{
    public interface IDataRepository
    {
        Customer[] GetCustomers();
    }

    public class DataRepository : IDataRepository
    {
        [Cache(Seconds: 60)]
        public Customer[] GetCustomers()
        {
            // It takes 10 seconds to load this data.
            // Once the data is loaded and cached, it should only take a few milliseconds.
            Thread.Sleep(10 * 1000);

            return new Customer[]
            {
                new Customer { CustomerId = 1, FirstName = "Sam", LastName = "Smith" },
                new Customer { CustomerId = 2, FirstName = "Monique", LastName = "Williams" },
            };
        }
    }
}
