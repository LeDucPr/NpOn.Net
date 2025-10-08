using CommonDb.DbResults;
using DbFactory;
using Enums;

namespace SystemController;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        Gh();
    }

    private static async Task<INpOnWrapperResult?> Gh()
    {
        
        
        IDbFactoryWrapper wrapper =
            new DbFactoryWrapper(
                "Host=localhost;Port=5432;Database=np_on_db;Username=postgres;Password=password",
                EDb.Postgres
            );
        var a = await wrapper.QueryAsync(StaticCommands.ConnectionCtrlGetAll);
        return a;
    }
}