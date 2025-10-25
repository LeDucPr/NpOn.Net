﻿using CommonDb.DbResults;
using DbFactory;
using Enums;
using HandlerFlow.AlgObjs.CtrlObjs;
using HandlerFlow.AlgObjs.CtrlObjs.Connections;
using HandlerFlow.AlgObjs.RaisingRouters;
using HandlerFlow.AlgObjs.SqlQueries;
using SystemController.ResultConverters;
using Test.TestZones;

namespace Test;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- Chay Test SignalR Client ---");
        SignalRTest.RunClientAsync().GetAwaiter().GetResult();
        Console.ReadKey();
        // DbFactoryIntegrationTest.DbFactoryIntegration().GetAwaiter();
    }

}