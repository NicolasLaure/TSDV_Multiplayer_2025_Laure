using System;
using System.Collections.Generic;
using Input;
using MidTerm2.Model;
using Network;
using Reflection;
using Utils;
using Random = System.Random;
using Vector2 = System.Numerics.Vector2;

namespace MidTerm2
{
    [Serializable]
    public class CastlesModel : IReflectiveModel
    {
        public Tile[][] board = new Tile[30][];
        private int mapSize = 30;
        [Sync] private bool isPlayer1Turn = true;

        [Sync] public Castle _castle = new Castle();
        [Sync] public Castle _OtherCastle = new Castle();

        public List<Warrior> _warriors = new List<Warrior>();
        public List<Warrior> _OtherWarriors = new List<Warrior>();

        private int initialWarriorQty = 15;

        private Random _random;

        public CastlesModel(InputReader input, ReflectiveClient<CastlesModel> client)
        {
            _random = new Random();
        }

        public void Initialize()
        {
            SetMap();
            SetArmy(ClientManager.Instance.networkClient.Id == 0);
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

        public Vector2 GetWarriorPos(bool isPlayerOne)
        {
            Vector2 cornerIndex = GetCastlePos(isPlayerOne);
            Vector2 xRandomRange = isPlayerOne ? new Vector2(cornerIndex.X, cornerIndex.X + 5) : new Vector2(cornerIndex.X - 5, cornerIndex.X);
            Vector2 yRandomRange = isPlayerOne ? new Vector2(cornerIndex.Y, cornerIndex.Y + 5) : new Vector2(cornerIndex.Y - 5, cornerIndex.Y);

            Vector2 warriorPos;
            do
            {
                warriorPos = new Vector2(_random.Next((int)xRandomRange.X, (int)xRandomRange.Y), _random.Next((int)yRandomRange.X, (int)yRandomRange.Y));
            } while (board[(int)warriorPos.X][(int)warriorPos.Y].currentObject != null);

            return warriorPos;
        }

        private void SetArmy(bool isPlayerOne)
        {
            ReflectiveClient<CastlesModel> client = ClientManager.Instance.networkClient;
            if (isPlayerOne)
                client.SendInstantiateRequest(_castle, MatrixHandler.Vector2To4X4(GetCastlePos(true)));
            else
                client.SendInstantiateRequest(_OtherCastle, MatrixHandler.Vector2To4X4(GetCastlePos(false)));

            for (int i = 0; i < initialWarriorQty; i++)
            {
                if (isPlayerOne)
                    client.SendInstantiateRequest(_warriors, MatrixHandler.Vector2To4X4(GetWarriorPos(true)));
                else
                    client.SendInstantiateRequest(_OtherWarriors, MatrixHandler.Vector2To4X4(GetWarriorPos(false)));
            }
        }

        public void SetTileObject(TileObject tileObject, Vector2 pos)
        {
            tileObject.SetTile(board[(int)pos.X][(int)pos.Y]);
        }

        public Vector2 GetCastlePos(bool isPlayerOne)
        {
            return isPlayerOne ? new Vector2(0, 0) : new Vector2(mapSize - 1, mapSize - 1);
        }
    }
}