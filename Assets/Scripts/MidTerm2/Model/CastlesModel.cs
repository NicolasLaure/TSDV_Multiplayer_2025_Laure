using System;
using System.Collections.Generic;
using Input;
using MidTerm2.Model;
using Network;
using Reflection;
using Reflection.RPC;
using UnityEngine;
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
        public bool isPlayerTurn = false;

        [Sync] public Castle _castle = new Castle();
        [Sync] public Castle _OtherCastle = new Castle();

        public List<Warrior> _warriors = new List<Warrior>();
        public List<Warrior> _OtherWarriors = new List<Warrior>();

        private int initialWarriorQty = 15;
        [Sync] public int remainingMoves = 10;
        private const int maxMoves = 10;
        private Random _random;

        public Tile selectedTile = null;
        public Warrior selectedWarrior = null;

        public CastlesModel(InputReader input, ReflectiveClient<CastlesModel> client)
        {
            _random = new Random();
        }

        public void Initialize()
        {
            SetMap();
            SetArmy(ClientManager.Instance.networkClient.Id == 0);
            isPlayerTurn = ClientManager.Instance.networkClient.Id == 0;
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

        [RPC]
        public void ChangeTurn()
        {
            isPlayerTurn = !isPlayerTurn;
            remainingMoves = maxMoves;
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
            } while (board[(int)warriorPos.X][(int)warriorPos.Y].currentObject != null || board[(int)warriorPos.X][(int)warriorPos.Y].isTaken);

            board[(int)warriorPos.X][(int)warriorPos.Y].isTaken = true;
            return warriorPos;
        }

        private void SetArmy(bool isPlayerOne)
        {
            ReflectiveClient<CastlesModel> client = ClientManager.Instance.networkClient;
            Tile castleTile;
            if (isPlayerOne)
            {
                castleTile = GetTile(GetCastlePos(true));
                client.SendInstantiateRequest(_castle, MatrixHandler.Vector2To4X4(castleTile.position));
            }
            else
            {
                castleTile = GetTile(GetCastlePos(false));
                client.SendInstantiateRequest(_OtherCastle, MatrixHandler.Vector2To4X4(castleTile.position));
            }

            castleTile.isTaken = true;

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
            board[(int)pos.X][(int)pos.Y].currentObject = tileObject;
        }

        public Vector2 GetCastlePos(bool isPlayerOne)
        {
            return isPlayerOne ? new Vector2(0, 0) : new Vector2(mapSize - 1, mapSize - 1);
        }

        public Tile GetTile(Vector2 pos)
        {
            return board[(int)pos.X][(int)pos.Y];
        }

        public void SelectTile(Vector2 pos)
        {
            selectedTile = board[(int)pos.X][(int)pos.Y];
            Debug.Log($"Selected Tile Position {selectedTile.position}");
            if (selectedWarrior != null)
            {
                selectedWarrior.Move(selectedTile, ref remainingMoves);
                selectedWarrior = null;
            }
        }

        public void SelectTileObject(Vector2 pos)
        {
            selectedTile = board[(int)pos.X][(int)pos.Y];
            if (selectedTile.currentObject as Warrior != null)
                selectedWarrior = (Warrior)board[(int)pos.X][(int)pos.Y].currentObject;
        }
    }
}