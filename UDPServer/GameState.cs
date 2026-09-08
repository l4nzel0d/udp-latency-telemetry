using System;

namespace UDPServer
{
    // Класс для хранения состояния игры
    public class GameState
    {
        public float PlayerX { get; private set; }
        public float PlayerY { get; private set; }

        public GameState()
        {
            PlayerX = 0;
            PlayerY = 0;
        }

        // Обработка движения
        public void ProcessMovement(float deltaX, float deltaY)
        {
            PlayerX += deltaX;
            PlayerY += deltaY;
        }

        // Обработка выстрела
        public string ProcessShoot(float angle, float power)
        {
            // Просто имитация выстрела
            return $"Выстрел произведен: угол={angle:F1}°, сила={power:F1}";
        }

        // Получение текущей позиции
        public string GetPosition()
        {
            return $"Позиция: X={PlayerX:F1}, Y={PlayerY:F1}";
        }
    }
}