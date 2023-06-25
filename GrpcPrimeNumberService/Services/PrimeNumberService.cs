using Grpc.Core;
using GrpcService1;
using System.Collections.Concurrent;

namespace GrpcPrimeNumberService.Services
{
    public class PrimeNumberService : Primenumber.PrimenumberBase
    {
        private static object lockObject = new object();
        private static long TotalMessagesReceived = 0;
        private readonly ILogger<PrimeNumberService> _logger;
        public PrimeNumberService(ILogger<PrimeNumberService> logger)
        {
            _logger = logger;
        }

        ConcurrentDictionary<long, long> primeNumberDictionary = new ConcurrentDictionary<long, long>();

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
                    primeNumberDictionary.TryAdd(requestedNumber, requestedNumber);
                }
                response.Result.Add(new RequestPrimeNumber
                { Id = request.Id, Number = requestedNumber, Message = requestedNumber + " IS PRIME NUMBER.", Timestamp = request.Timestamp });
            }
        }
    }
}
