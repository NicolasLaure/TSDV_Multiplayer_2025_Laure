using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using CustomMath;
using UnityEditor.PackageManager;
using UnityEngine;

[ExecuteInEditMode]
public class MyTransformVisualizer : MonoBehaviour
{
    [SerializeField] private List<MyTransform> _transforms = new List<MyTransform>();
    [SerializeField] private List<Transform> _objects = new List<Transform>();

    private void OnEnable()
    {
        _transforms.Clear();
        for (int i = 0; i < _objects.Count; i++)
        {
            _transforms.Add(new MyTransform(" ", Vec3.One, MyQuaternion.identity, Vec3.One));
            if (i == 0)
                continue;

            _transforms[i].SetParent(_transforms[i - 1]);
        }
    }

    private void LateUpdate()
    {
        _transforms[0].TestUpdate();

        for (int i = 0; i < _objects.Count; i++)
        {
            _objects[i].SetPositionAndRotation(_transforms[i].Position, _transforms[i].Rotation.toQuaternion);
            _objects[i].localScale = _transforms[i].lossyScale;
        }
    }

    public MyTransform GetTransform(int index)
    {
        if (index > _transforms.Count)
            throw new IndexOutOfRangeException();

        return _transforms[index];
    }
}