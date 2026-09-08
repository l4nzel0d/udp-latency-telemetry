using System;
using System.Text;
using System.IO;

namespace UDPServer
{
    // Перечисление типов команд
    public enum CommandType : int
    {
        Movement = 1,
        Shoot = 2,
        Response = 3
    }

    // Заголовок пакета (12 байт)
    public struct PacketHeader
    {
        public CommandType Type;      // 4 байта
        public int SequenceNumber;    // 4 байта
        public int DataSize;          // 4 байта
    }

    // Данные для команды движения
    public struct MovementData
    {
        public float X;               // координата X
        public float Y;               // координата Y
    }

    // Данные для команды выстрела
    public struct ShootData
    {
        public float Angle;           // угол в градусах (0-360)
        public float Power;           // сила выстрела (0-100)
    }

    // Ответ сервера
    public struct ResponseData
    {
        public bool Success;          // успешность операции
        public string Message;        // текстовое сообщение
    }

    // Класс для сериализации и десериализации пакетов
    public static class PacketSerializer
    {
        // Сериализация заголовка
        public static byte[] SerializeHeader(PacketHeader header)
        {
            byte[] data = new byte[12];
            BitConverter.GetBytes((int)header.Type).CopyTo(data, 0);
            BitConverter.GetBytes(header.SequenceNumber).CopyTo(data, 4);
            BitConverter.GetBytes(header.DataSize).CopyTo(data, 8);
            return data;
        }

        // Десериализация заголовка
        public static PacketHeader DeserializeHeader(byte[] data)
        {
            PacketHeader header = new PacketHeader();
            header.Type = (CommandType)BitConverter.ToInt32(data, 0);
            header.SequenceNumber = BitConverter.ToInt32(data, 4);
            header.DataSize = BitConverter.ToInt32(data, 8);
            return header;
        }

        // Сериализация данных движения
        public static byte[] SerializeMovement(MovementData movement)
        {
            byte[] data = new byte[8];
            BitConverter.GetBytes(movement.X).CopyTo(data, 0);
            BitConverter.GetBytes(movement.Y).CopyTo(data, 4);
            return data;
        }

        // Десериализация данных движения
        public static MovementData DeserializeMovement(byte[] data)
        {
            MovementData movement = new MovementData();
            movement.X = BitConverter.ToSingle(data, 0);
            movement.Y = BitConverter.ToSingle(data, 4);
            return movement;
        }

        // Сериализация данных выстрела
        public static byte[] SerializeShoot(ShootData shoot)
        {
            byte[] data = new byte[8];
            BitConverter.GetBytes(shoot.Angle).CopyTo(data, 0);
            BitConverter.GetBytes(shoot.Power).CopyTo(data, 4);
            return data;
        }

        // Десериализация данных выстрела
        public static ShootData DeserializeShoot(byte[] data)
        {
            ShootData shoot = new ShootData();
            shoot.Angle = BitConverter.ToSingle(data, 0);
            shoot.Power = BitConverter.ToSingle(data, 4);
            return shoot;
        }

        // Сериализация ответа
        public static byte[] SerializeResponse(ResponseData response)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(response.Message);
            byte[] data = new byte[1 + 4 + messageBytes.Length];
            data[0] = response.Success ? (byte)1 : (byte)0;
            BitConverter.GetBytes(messageBytes.Length).CopyTo(data, 1);
            messageBytes.CopyTo(data, 5);
            return data;
        }

        // Десериализация ответа
        public static ResponseData DeserializeResponse(byte[] data)
        {
            ResponseData response = new ResponseData();
            response.Success = data[0] == 1;
            int messageLength = BitConverter.ToInt32(data, 1);
            response.Message = Encoding.UTF8.GetString(data, 5, messageLength);
            return response;
        }

        // Создание полного пакета
        public static byte[] CreatePacket(CommandType type, int sequenceNumber, byte[] data)
        {
            PacketHeader header = new PacketHeader
            {
                Type = type,
                SequenceNumber = sequenceNumber,
                DataSize = data.Length
            };

            byte[] headerBytes = SerializeHeader(header);
            byte[] packet = new byte[headerBytes.Length + data.Length];
            headerBytes.CopyTo(packet, 0);
            data.CopyTo(packet, headerBytes.Length);
            return packet;
        }
    }
}