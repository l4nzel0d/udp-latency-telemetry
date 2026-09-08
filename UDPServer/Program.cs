using System;

namespace UDPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 11000; // Порт по умолчанию
            
            // Проверяем, передан ли порт в аргументах
            if (args.Length > 0)
            {
                if (int.TryParse(args[0], out int customPort))
                {
                    port = customPort;
                }
            }

            GameServer server = new GameServer(port);
            
            // Обработка Ctrl+C для корректного завершения
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("\nЗавершение работы сервера...");
                server.Stop();
                e.Cancel = true;
                Environment.Exit(0);
            };

            server.Start();
        }
    }
}