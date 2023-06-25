using Grpc.Core;
using GrpcService1;
using System.Collections.Concurrent;

namespace GrpcPrimeNumberService.Services
{
   
    public class PrimeNumberService : Primenumber.PrimenumberBase
    {
        private static ConcurrentDictionary<long, PrimeNumberWithWeights> primeNumberDictionary = new ConcurrentDictionary<long, PrimeNumberWithWeights>();
        private static object lockObject = new object();
        private static long TotalMessagesReceived = 0;
        private readonly ILogger<PrimeNumberService> _logger;
        public PrimeNumberService(ILogger<PrimeNumberService> logger)
        {
            _logger = logger;
            Task.Run(DisplayRecordsPrimeNumbersPeriodically);
        }


        public override async Task IsPrimeStream(IAsyncStreamReader<RequestPrimeNumber> requestStream,
            IServerStreamWriter<ReplyPrimeNumber> responseStream, ServerCallContext context)
        {
            var response = new ReplyPrimeNumber();
            await foreach (RequestPrimeNumber request in requestStream.ReadAllAsync())
            {
                lock (lockObject)
                {
                    TotalMessagesReceived++;
                }

                bool IsPrime = true;
                Int64 requestedNumber = request.Number;

                if (requestedNumber <= 1)
                {
                    response.Result.Add(new RequestPrimeNumber
                    {
                        Id = request.Id,
                        Number = requestedNumber,
                        Message = requestedNumber + " not prime.",
                        Timestamp = request.Timestamp
                    });
                }
                else
                {
                    NumberIsPrime(response, request, IsPrime, requestedNumber);
                }

            }

            await responseStream.WriteAsync(response);

        }

        private void NumberIsPrime(ReplyPrimeNumber response, RequestPrimeNumber request, bool IsPrime, long requestedNumber)
        {
            Int64 iterations = requestedNumber / 2;
            for (int i = 2; i <= iterations; i++)
            {
                if (requestedNumber % i == 0)
                {
                    IsPrime = false;

                    response.Result.Add(new RequestPrimeNumber
                    { Id = request.Id, Number = requestedNumber, Message = requestedNumber + " not prime.", Timestamp = request.Timestamp });
                    break;
                }
            }
            if (IsPrime)
            {
                if (primeNumberDictionary.Where(p => p.Key == request.Number).Count() == 0)
                {
                    primeNumberDictionary.TryAdd(requestedNumber, new PrimeNumberWithWeights { IsPrime = true, Number = requestedNumber, weight = 1});
                }
                else if(primeNumberDictionary.Where(p => p.Key == request.Number).Count() == 1)
                {
                    var item = primeNumberDictionary.Where(p => p.Key == request.Number).FirstOrDefault();
                    item.Value.weight += 1;
                }
                response.Result.Add(new RequestPrimeNumber
                { Id = request.Id, Number = requestedNumber, Message = requestedNumber + " IS PRIME NUMBER.", Timestamp = request.Timestamp });
            }
        }

        private async Task DisplayRecordsPrimeNumbersPeriodically()
        {
            while (true)
            {
                
                if (primeNumberDictionary.Count > 0)
                {
                    var sortedPrimeNumberDic = (from p in primeNumberDictionary
                                                orderby p.Value.weight descending, p.Value.Number descending
                                                select p).Take(10);
                    Console.WriteLine("  Total Number of Messages Received:           " + TotalMessagesReceived);
                    Console.WriteLine("  Top 10 prime numbers are: ");
                    foreach (var item in sortedPrimeNumberDic)
                    {
                        Console.WriteLine("     " + item.Value.Number);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }
    }
    public class PrimeNumberWithWeights
    {
        public long Number { get; set; }
        public bool IsPrime { get; set; } = false;
        public int weight { get; set; } = 0;
    }
}
