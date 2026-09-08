using System;
using System.Net;
using System.Net.Sockets;

namespace UDPServer
{
    public class GameServer
    {
        private UdpClient udpClient;
        private IPEndPoint clientEndPoint;
        private GameState gameState;
        private bool isRunning;

        public GameServer(int port)
        {
            udpClient = new UdpClient(port);
            gameState = new GameState();
            isRunning = true;
            clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        public void Start()
        {
            Console.WriteLine("=== UDP Game Server ===");
            Console.WriteLine($"Сервер запущен на порту {((IPEndPoint)udpClient.Client.LocalEndPoint).Port}");
            Console.WriteLine("Ожидание команд...\n");

            while (isRunning)
            {
                try
                {
                    // Получение пакета
                    byte[] receivedData = udpClient.Receive(ref clientEndPoint);
                    
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Получена команда от {clientEndPoint.Address}:{clientEndPoint.Port}");

                    // Обработка пакета
                    ProcessPacket(receivedData);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Ошибка сокета: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        private void ProcessPacket(byte[] data)
        {
            if (data.Length < 12)
            {
                Console.WriteLine("Ошибка: пакет слишком маленький");
                return;
            }

            // Разбор заголовка
            byte[] headerBytes = new byte[12];
            Array.Copy(data, 0, headerBytes, 0, 12);
            PacketHeader header = PacketSerializer.DeserializeHeader(headerBytes);

            Console.WriteLine($"Тип: {header.Type}, Номер: {header.SequenceNumber}");

            // Извлечение данных
            byte[] dataBytes = new byte[header.DataSize];
            Array.Copy(data, 12, dataBytes, 0, header.DataSize);

            // Обработка в зависимости от типа команды
            ResponseData response = new ResponseData();
            
            switch (header.Type)
            {
                case CommandType.Movement:
                    response = ProcessMovementCommand(dataBytes);
                    break;
                    
                case CommandType.Shoot:
                    response = ProcessShootCommand(dataBytes);
                    break;
                    
                default:
                    response.Success = false;
                    response.Message = "Неизвестный тип команды";
                    break;
            }

            // Отправка ответа
            SendResponse(response, header.SequenceNumber);
        }

        private ResponseData ProcessMovementCommand(byte[] data)
        {
            MovementData movement = PacketSerializer.DeserializeMovement(data);
            Console.WriteLine($"Движение: dX={movement.X:F1}, dY={movement.Y:F1}");

            // Обновляем позицию
            gameState.ProcessMovement(movement.X, movement.Y);
            
            string position = gameState.GetPosition();

            return new ResponseData 
            { 
                Success = true, 
                Message = $"Движение выполнено. {position}" 
            };
        }

        private ResponseData ProcessShootCommand(byte[] data)
        {
            ShootData shoot = PacketSerializer.DeserializeShoot(data);
            Console.WriteLine($"Выстрел: угол={shoot.Angle:F1}°, сила={shoot.Power:F1}");

            // Обрабатываем выстрел
            string result = gameState.ProcessShoot(shoot.Angle, shoot.Power);
            
            return new ResponseData 
            { 
                Success = true, 
                Message = result 
            };
        }

        private void SendResponse(ResponseData response, int sequenceNumber)
        {
            byte[] responseData = PacketSerializer.SerializeResponse(response);
            byte[] packet = PacketSerializer.CreatePacket(CommandType.Response, sequenceNumber, responseData);

            udpClient.Send(packet, packet.Length, clientEndPoint);
            Console.WriteLine($"Отправлен ответ: {response.Message}\n");
        }

        public void Stop()
        {
            isRunning = false;
            udpClient.Close();
            Console.WriteLine("Сервер остановлен");
        }
    }
}