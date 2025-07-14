using System;
using System.Collections.Generic;
using FPS.AuthServer;
using Input;
using MidTerm2.Model;
using Network;
using Reflection.RPC;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using Random = System.Random;

namespace MidTerm2
{
    [Serializable]
    public class CastlesModel : IReflectiveModel
    {
        public Tile[][] board = new Tile[30][];
        private int mapSize = 30;
        public bool isPlayerOneTurn = false;

        public Castle _castle = new Castle();
        public Castle _OtherCastle = new Castle();

        public List<Warrior> _warriors = new List<Warrior>();
        public List<Warrior> _OtherWarriors = new List<Warrior>();

        private int initialWarriorQty = 15;
        public int remainingMoves = 10;
        private const int maxMoves = 10;
        private Random _random;

        public Tile selectedTile = null;
        public int selectedWarriorIndex = -1;

        public Action onTurnChange;

        public CastlesModel()
        {
            _random = new Random();
        }

        public void Initialize()
        {
            SetMap();
            isPlayerOneTurn = true;
        }

        public void Update()
        {
            UpdateWarriors(_warriors);
            UpdateWarriors(_OtherWarriors);
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


        public void ChangeTurnLocal()
        {
            onTurnChange?.Invoke();
        }

        [RPC]
        public void ChangeTurn()
        {
            isPlayerOneTurn = !isPlayerOneTurn;
            remainingMoves = maxMoves;
            ChangeTurnLocal();
        }

        private void UpdateWarriors(List<Warrior> warriors)
        {
            foreach (Warrior warrior in warriors)
            {
                if (board[(int)warrior.position.x][(int)warrior.position.y].currentObject != warrior)
                    SetTileObject(warrior, warrior.position);
            }
        }

        public Vector2 GetWarriorPos(bool isPlayerOne)
        {
            Vector2 cornerIndex = GetCastlePos(isPlayerOne);
            Vector2 xRandomRange = isPlayerOne ? new Vector2(cornerIndex.x, cornerIndex.x + 5) : new Vector2(cornerIndex.x - 5, cornerIndex.x);
            Vector2 yRandomRange = isPlayerOne ? new Vector2(cornerIndex.y, cornerIndex.y + 5) : new Vector2(cornerIndex.y - 5, cornerIndex.y);

            Vector2 warriorPos;
            do
            {
                warriorPos = new Vector2(_random.Next((int)xRandomRange.x, (int)xRandomRange.y), _random.Next((int)yRandomRange.x, (int)yRandomRange.y));
            } while (board[(int)warriorPos.x][(int)warriorPos.y].currentObject != null || board[(int)warriorPos.x][(int)warriorPos.y].isTaken);

            board[(int)warriorPos.x][(int)warriorPos.y].isTaken = true;
            return warriorPos;
        }

        public void SetArmy(bool isPlayerOne)
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
                    client.SendInstantiateRequest(_warriors, MatrixHandler.Vector2To4X4(GetWarriorPos(true)), i);
                else
                    client.SendInstantiateRequest(_OtherWarriors, MatrixHandler.Vector2To4X4(GetWarriorPos(false)), i);
            }
        }

        public void SetServerArmy(int id, bool isPlayerOne, short color)
        {
            ReflectiveAuthoritativeServer<CastlesModel> server = CastlesAuthServer.Instance._server;
            Tile castleTile;
            if (isPlayerOne)
            {
                castleTile = GetTile(GetCastlePos(true));
                server.BroadcastInstantiateRequest(_castle, MatrixHandler.Vector2To4X4(castleTile.position), color, id);
            }
            else
            {
                castleTile = GetTile(GetCastlePos(false));
                server.BroadcastInstantiateRequest(_OtherCastle, MatrixHandler.Vector2To4X4(castleTile.position), color, id);
            }

            castleTile.isTaken = true;

            for (int i = 0; i < initialWarriorQty; i++)
            {
                if (isPlayerOne)
                    server.BroadcastInstantiateRequest(_warriors, MatrixHandler.Vector2To4X4(GetWarriorPos(true)), color, id, i);
                else
                    server.BroadcastInstantiateRequest(_OtherWarriors, MatrixHandler.Vector2To4X4(GetWarriorPos(false)), color, id, i);
            }
        }

        public void SetTileObject(TileObject tileObject, Vector2 pos)
        {
            board[(int)tileObject.position.x][(int)tileObject.position.y].currentObject = null;
            tileObject.SetTile(board[(int)pos.x][(int)pos.y]);
            board[(int)pos.x][(int)pos.y].currentObject = tileObject;
        }

        public Vector2 GetCastlePos(bool isPlayerOne)
        {
            return isPlayerOne ? new Vector2(0, 0) : new Vector2(mapSize - 1, mapSize - 1);
        }

        public Tile GetTile(Vector2 pos)
        {
            return board[(int)pos.x][(int)pos.y];
        }

        public void SelectTile(Vector2 pos, int clientId)
        {
            selectedTile = board[(int)pos.x][(int)pos.y];
            Debug.Log($"Selected Tile Position {selectedTile.position}");
            if (selectedWarriorIndex != -1)
            {
                if (clientId == 0)
                    _warriors[selectedWarriorIndex].Move(selectedTile, ref remainingMoves);
                else
                    _OtherWarriors[selectedWarriorIndex].Move(selectedTile, ref remainingMoves);

                selectedWarriorIndex = -1;
            }
        }

        public void SelectTileObject(Vector2 pos, int clientId)
        {
            selectedTile = board[(int)pos.x][(int)pos.y];
            if (selectedTile.currentObject as Warrior != null)
            {
                List<Warrior> warriors = _warriors;
                if (clientId != 0)
                    warriors = _OtherWarriors;
                for (int i = 0; i < warriors.Count; i++)
                {
                    if (warriors[i].position == pos)
                    {
                        selectedWarriorIndex = i;
                        return;
                    }
                }
            }
        }
    }
}