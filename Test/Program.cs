using CommonDb.DbResults;
using DbFactory;
using Enums;
using Test.TestZones;

namespace Test;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- Chay Test SignalR Client ---");
        // SignalRTest.RunClientAsync().GetAwaiter().GetResult();
        FaCareListenSignalTest.RunClientAsync().GetAwaiter().GetResult();
        Console.ReadKey();
        // DbFactoryIntegrationTest.DbFactoryIntegration().GetAwaiter();
    }
}