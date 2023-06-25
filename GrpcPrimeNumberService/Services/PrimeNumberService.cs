using Grpc.Core;
using GrpcService1;

namespace GrpcPrimeNumberService.Services
{
    public class PrimeNumberService : Primenumber.PrimenumberBase
    {
        private readonly ILogger<PrimeNumberService> _logger;
        public PrimeNumberService(ILogger<PrimeNumberService> logger)
        {
            _logger = logger;
        }
    }
}
