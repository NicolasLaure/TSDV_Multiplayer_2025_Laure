using System;
using System.Collections.Generic;
using MidTerm2.Model;
using Network;
using Network.Factory;
using Reflection;
using UnityEngine;
using Utils;

namespace ReflectionTest
{
    public class ReflectionTest : MonoBehaviour
    {
        private TestModel model;
        [SerializeField] private ColorHandler color;
        [SerializeField] private HashHandler hashHandler;

        ReflectionHandler<TestModel> reflectionHandler;
        private ReflectiveFactory<TestModel> _factory;

        void Start()
        {
            reflectionHandler = new ReflectionHandler<TestModel>(ref model);
            List<Type> types = new List<Type>();
            types.AddRange(new[] { typeof(Castle), typeof(Warrior) });
            _factory = new ReflectiveFactory<TestModel>(reflectionHandler, types, color, hashHandler);
            hashHandler.Initialize();
        }

        private int index = 0;

        [ContextMenu("AddWarrior")]
        void AddWarrior()
        {
            int[] route = { 4, index };
            InstanceData instanceData = new InstanceData
            {
                originalClientID = 1,
                prefabHash = _factory.typeHashes.typeToHash[typeof(Warrior)],
                instanceID = -1,
                trs = ByteFormat.Get4X4Bytes(Matrix4x4.identity),
                color = 0,
                routeLength = route.Length,
                route = route
            };
            _factory.Instantiate(instanceData);

            //            reflectionHandler.SetData<Warrior>(new int[] { 4, index }, new Warrior());
            index++;

            LogAllChildren(reflectionHandler.Root);
        }

        private void LogAllChildren(Node root)
        {
            foreach (Node child in root.Children)
            {
                LogAllChildren(child);
                Debug.Log($"Child Route: {Route.RouteString(child.GetRoute())}");
            }
        }

        [ContextMenu("SetCastlePos")]
        void SetCastlePos()
        {
            Debug.Log($"Castle Pos: {reflectionHandler.GetDataAt(new int[] { 3, 3, 0 })}");
            reflectionHandler.SetData<Castle>(new int[] { 3, 3, 0 }, 4);
            Debug.Log($"Castle Pos: {reflectionHandler.GetDataAt(new int[] { 3, 3, 0 })}");
        }

        [ContextMenu("SetWarriorPos")]
        void SetWarriorPos()
        {
            Debug.Log($"Warrior Pos: {reflectionHandler.GetDataAt(new int[] { 4, 0, 3, 0 })}");
            reflectionHandler.SetData<Warrior>(new int[] { 4, 0, 3, 0 }, 12);
            Debug.Log($"Warrior Pos: {reflectionHandler.GetDataAt(new int[] { 4, 0, 3, 0 })}");
        }


        [ContextMenu("GetBaseItem")]
        void GetBaseItem()
        {
            Debug.Log($"Object recovered: {reflectionHandler.GetDataAt(new int[] { 2, 2 })}");
        }
    }
}