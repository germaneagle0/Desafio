using System;

class Program
{
    static async Task Main(string[] args)
    {
        int RequestPeriod = 90;
        if (args.Length != 3 && args.Length != 4)
        {
            Console.WriteLine("Arguments received:");
            foreach (var arg in args)
            {
                Console.WriteLine(arg);
            }
            throw new ArgumentException("Arguments expected: STOCK_NAME MINIMUM MAXIMUM [SECOND_PER_REQUEST]");
        }
        string stock = args[0];
        float min = float.Parse(args[1]);
        float max = float.Parse(args[2]);
        if (args.Length == 4) RequestPeriod = int.Parse(args[3]);
        // CONFIG FILE EXTRACTION
        Console.WriteLine("Opening configuration file...");
        Config.JSON<Config.ConfigurationEmail> fileHandler = new("./config.json");
        Config.ConfigurationEmail emailData = fileHandler.LoadData();
        List<string> emails = emailData.Emails;
        Console.WriteLine("OK!");

        // EMAIL SERVER
        Console.WriteLine("Connecting to Gmail API...");
        Email.Gmail sender = new(emailData.PrivateEmailData);

        Console.WriteLine("OK!");

        // STOCK API SERVER
        Stock.BRAPI stockAPI = new();

        // DATA DISTRIBUTER CONFIGURATION
        Distributor.ObservableStock updater = new(min, max, stockAPI, stock);

        foreach (string email in emails)
        {
            Distributor.EmailObserver client = new(sender, email);
            updater.Subscribe(client);
        }

        // PeriodicRequest
        Console.WriteLine($"Starting Request every {RequestPeriod} seconds...");
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(RequestPeriod));
        
        // First Request
        await updater.Update();
        Console.WriteLine("Updated...");
        while (await timer.WaitForNextTickAsync())
        {
            await updater.Update();
            Console.WriteLine("Updated...");
        }
    }
}