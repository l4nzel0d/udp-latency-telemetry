using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace UDPClient
{
    public class GameClient
    {
        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;
        private int sequenceNumber;
        private Timer sendTimer;
        private bool isRunning;

        private const int SEND_INTERVAL_MS = 5000;

        public GameClient(string serverAddress, int serverPort)
        {
            udpClient = new UdpClient();
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverAddress), serverPort);
            sequenceNumber = 0;
            isRunning = true;
        }

        public void Start()
        {
            Console.WriteLine("=== UDP Game Client ===");
            Console.WriteLine($"Подключение к серверу {serverEndPoint.Address}:{serverEndPoint.Port}");
            Console.WriteLine("Отправка команд каждые 5 секунд...\n");

            // Запуск получения ответов в отдельном потоке
            Task.Run(ReceiveResponses);

            // Настройка таймера для отправки команд
            sendTimer = new Timer(SendCommand, null, 0, SEND_INTERVAL_MS);

            // Ожидание завершения
            Console.WriteLine("Нажмите Enter для выхода...\n");
            Console.ReadLine();

            Stop();
        }

        private void SendCommand(object state)
        {
            sequenceNumber++;
            
            // Чередование команд: четные - движение, нечетные - выстрел
            if (sequenceNumber % 2 == 1)
            {
                SendMovementCommand();
            }
            else
            {
                SendShootCommand();
            }
        }

        private void SendMovementCommand()
        {
            // Генерируем случайное движение
            Random random = new Random();
            MovementData movement = new MovementData
            {
                X = (float)(random.NextDouble() * 10 - 5),  // от -5 до 5
                Y = (float)(random.NextDouble() * 10 - 5)   // от -5 до 5
            };

            byte[] movementData = PacketSerializer.SerializeMovement(movement);
            byte[] packet = PacketSerializer.CreatePacket(CommandType.Movement, sequenceNumber, movementData);

            udpClient.Send(packet, packet.Length, serverEndPoint);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Отправлена команда MOVEMENT #{sequenceNumber}: dX={movement.X:F1}, dY={movement.Y:F1}");
        }

        private void SendShootCommand()
        {
            // Генерируем случайный выстрел
            Random random = new Random();
            ShootData shoot = new ShootData
            {
                Angle = (float)(random.NextDouble() * 360),  // от 0 до 360 градусов
                Power = (float)(random.NextDouble() * 100)   // от 0 до 100
            };

            byte[] shootData = PacketSerializer.SerializeShoot(shoot);
            byte[] packet = PacketSerializer.CreatePacket(CommandType.Shoot, sequenceNumber, shootData);

            udpClient.Send(packet, packet.Length, serverEndPoint);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Отправлена команда SHOOT #{sequenceNumber}: угол={shoot.Angle:F1}°, сила={shoot.Power:F1}");
        }

        private void ReceiveResponses()
        {
            while (isRunning)
            {
                try
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedData = udpClient.Receive(ref remoteEndPoint);

                    // Разбор ответа
                    if (receivedData.Length >= 12)
                    {
                        byte[] headerBytes = new byte[12];
                        Array.Copy(receivedData, 0, headerBytes, 0, 12);
                        PacketHeader header = PacketSerializer.DeserializeHeader(headerBytes);

                        if (header.Type == CommandType.Response && header.DataSize > 0)
                        {
                            byte[] responseBytes = new byte[header.DataSize];
                            Array.Copy(receivedData, 12, responseBytes, 0, header.DataSize);
                            ResponseData response = PacketSerializer.DeserializeResponse(responseBytes);

                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Получен ответ #{header.SequenceNumber}: {response.Message}");
                        }
                    }
                }
                catch (SocketException)
                {
                    // Сокет закрыт
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении: {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            isRunning = false;
            sendTimer?.Dispose();
            udpClient.Close();
            Console.WriteLine("Клиент остановлен");
        }
    }
}