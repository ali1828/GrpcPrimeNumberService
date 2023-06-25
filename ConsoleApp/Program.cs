using Grpc.Core;
using Grpc.Net.Client;
using GrpcService1;
using System.Collections.Concurrent;

var runMain = new Main();
await runMain.PrimeNumberBiDirectional();

public class Main
{

    private static long RequestNumber = 0;
    private static object lockObject = new object();
    ConcurrentDictionary<long, PrimeNumber> primeNumberDictionary = new ConcurrentDictionary<long, PrimeNumber>();
    public async Task PrimeNumberBiDirectional()
    {
        try
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5028");
            var client = new Primenumber.PrimenumberClient(channel);
            Random rnd = new Random();

            while (true)
            {
                bool retryUnProcessedMessages = false;
                var listOfUnProcessedMessages = primeNumberDictionary.Where(p => p.Value.IsMessageProcessed == false);
                if (listOfUnProcessedMessages?.Count() > 0)
                {
                    retryUnProcessedMessages = true;
                }
                await FindPrimeNumberBiDirectionalCall(client, rnd, retryUnProcessedMessages);
            }
        }
        catch (RpcException rpcExcp)
        {
            Console.WriteLine(rpcExcp.Message);
            Console.WriteLine("Trying to established a channel.....");
            lock (lockObject)
            {
                RequestNumber = 0;
            }
            await PrimeNumberBiDirectional();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine("Trying to established a channel.....");
            lock (lockObject)
            {
                RequestNumber = 0;
            }
            await PrimeNumberBiDirectional();
        }
    }

    private async Task FindPrimeNumberBiDirectionalCall(Primenumber.PrimenumberClient client, Random rnd, bool retryUnProcessedMessage = false)
    {
        using (var call = client.IsPrimeStream())
        {
            var responseReaderTask = Task.Run(async () =>
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine("Request #: " + RequestNumber);

                    foreach (var item in response.Result)
                    {
                        var numberFound = primeNumberDictionary.Where(p => p.Key == item.Id);
                        if (numberFound?.Count() >= 1)
                        {
                            numberFound.FirstOrDefault().Value.IsMessageProcessed = true;
                            if (item.Message == item.Number + " IS PRIME NUMBER.")
                            {
                                numberFound.FirstOrDefault().Value.IsPrime = true;
                            }
                        }
                        Console.WriteLine("ID: " + item.Id + " Number: " + item.Number + " Message: " + item.Message +
                            " RTT: " + (Convert.ToInt64(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds) - item.Timestamp));
                    }
                    Console.WriteLine("Request Completed...");
                }
            });

            // Send multiple requests
            for (int i = 1; i <= 10000; i++)
            {
                Int64 randomNumber = Convert.ToInt64(rnd.Next(1001));
                await call.RequestStream.WriteAsync(new RequestPrimeNumber
                {

                    Id = RequestNumber,
                    Number = randomNumber,
                    Timestamp = Convert.ToInt64(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds)
                });
                primeNumberDictionary.TryAdd(RequestNumber, new PrimeNumber { Number = randomNumber, IsPrime = false });

                lock (lockObject)
                {
                    RequestNumber++;
                }

            }
            await call.RequestStream.CompleteAsync();
            await responseReaderTask;

            await Task.Delay(TimeSpan.FromSeconds(1));

            if (retryUnProcessedMessage)
            {
                var listOfUnProcessedMessages = primeNumberDictionary.Where(p => p.Value.IsMessageProcessed == false);
                Console.WriteLine("Retrying: Sending unprocessed messages: ");
                foreach (var message in listOfUnProcessedMessages)
                {
                    Console.WriteLine("Retry for " + message.Value.Number);
                    await call.RequestStream.WriteAsync(new RequestPrimeNumber
                    {
                        Id = RequestNumber,
                        Number = message.Value.Number,
                        Timestamp = Convert.ToInt64(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds)
                    });
                    lock (lockObject)
                    {
                        RequestNumber++;
                    }
                }
            }
        }

    }

}

public class PrimeNumber
{
    public long Number { get; set; }
    public bool IsPrime { get; set; } = false;
    public bool IsMessageProcessed { get; set; } = false;
}