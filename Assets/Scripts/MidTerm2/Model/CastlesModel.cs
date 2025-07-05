using System;
using System.Collections.Generic;
using Input;
using MidTerm2.Model;
using Reflection;
using ReflectionTest;
using Vector2 = System.Numerics.Vector2;

namespace MidTerm2
{
    [Serializable]
    public class CastlesModel : IReflectiveModel
    {
        public Tile[][] board = new Tile[30][];
        private int mapSize = 30;
        [Sync] private bool isPlayer1Turn = true;

        [Sync] public Castle _castle;
        public Castle _OtherCastle;

        public List<Warrior> _warriors;
        public List<Warrior> _OtherWarriors;

        private int initialWarriorQty = 15;

        private Random _random;

        public CastlesModel(InputReader input, int clientID)
        {
            _random = new Random();
            SetMap();
            SetCastle(clientID == 1);
        }

        private void SetMap()
        {
            for (int i = 0; i < mapSize; i++)
            {
                board[i] = new Tile[30];
                for (int j = 0; j < mapSize; j++)
                {
                    board[i][j] = new Tile(new Vector2(i, j));
                }
            }
        }

        private void SetCastle(bool isPlayerOne)
        {
            Vector2 cornerIndex = isPlayerOne ? new Vector2(0, 0) : new Vector2(mapSize - 1, mapSize - 1);
            Vector2 xRandomRange = isPlayerOne ? new Vector2(cornerIndex.X, cornerIndex.X + 5) : new Vector2(cornerIndex.X - 5, cornerIndex.X);
            Vector2 yRandomRange = isPlayerOne ? new Vector2(cornerIndex.Y, cornerIndex.Y + 5) : new Vector2(cornerIndex.Y - 5, cornerIndex.Y);

            _castle = new Castle(100);
            _castle.SetTile(board[(int)cornerIndex.X][(int)cornerIndex.Y]);


            _warriors = new List<Warrior>();
            for (int i = 0; i < initialWarriorQty; i++)
            {
                Warrior warrior = new Warrior(50);
                Vector2 warriorPos;
                do
                {
                    warriorPos = new Vector2(_random.Next((int)xRandomRange.X, (int)xRandomRange.Y), _random.Next((int)yRandomRange.X, (int)yRandomRange.Y));
                } while (board[(int)warriorPos.X][(int)warriorPos.Y].currentObject != null);

                warrior.SetTile(board[(int)warriorPos.X][(int)warriorPos.Y]);
                _warriors.Add(warrior);
            }
        }
    }
}