using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace Function.Tests
{
    class TestAsyncCollector:IAsyncCollector<string>
    {
        public Task AddAsync(string item, CancellationToken cancellationToken = new CancellationToken())
        {
            Debug.WriteLine(item);
            return Task.FromResult(true);
        }

        public Task FlushAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(true);
        }
    }
}
