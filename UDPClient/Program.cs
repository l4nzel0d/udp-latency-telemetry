using System;

namespace UDPClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverAddress = "127.0.0.1"; // Локальный адрес по умолчанию
            int serverPort = 11000;             // Порт по умолчанию

            // Проверяем аргументы командной строки
            if (args.Length > 0)
            {
                serverAddress = args[0];
            }
            
            if (args.Length > 1)
            {
                if (int.TryParse(args[1], out int customPort))
                {
                    serverPort = customPort;
                }
            }

            GameClient client = new GameClient(serverAddress, serverPort);
            client.Start();
        }
    }
}