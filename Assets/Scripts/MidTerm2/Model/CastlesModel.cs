using System;
using CustomMath;
using Input;
using MidTerm2.Model;
using Reflection;
using Reflection.RPC;
using ReflectionTest;
using UnityEngine;

namespace MidTerm2
{
    [Serializable]
    public class CastlesModel : IReflectiveModel
    {
        public TestModel test = new TestModel(2, 3);

        //public List<List<TestModel>> tests = new List<List<TestModel>>();
        public Tile[][] board = new Tile[30][];
        private int mapSize = 30;
        [Sync] private bool isPlayer1Turn = true;

        public CastlesModel(InputReader input)
        {
            SetMap();
        }

        private void SetMap()
        {
            for (int i = 0; i < mapSize; i++)
            {
                board[i] = new Tile[30];
                for (int j = 0; j < mapSize; j++)
                {
                    board[i][j] = new Tile();
                }
            }
        }
    }
}