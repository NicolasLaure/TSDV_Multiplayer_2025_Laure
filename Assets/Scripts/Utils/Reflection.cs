using System.Collections;
using UnityEngine;
using System.Reflection;

public class Reflection : MonoBehaviour
{
    private TestModel _testModel = new TestModel();

    [ContextMenu("Reflect")]
    public void ReflectModel()
    {
        Reflect(_testModel);
    }

    private void Reflect(object obj)
    {
        foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                            BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            if (field.FieldType != typeof(string) &&
                (field.FieldType.IsArray || typeof(ICollection).IsAssignableFrom(field.FieldType)))
            {
                Debug.Log($"Field Name: {field.Name} IS COLLECTION");
                foreach (object item in field.GetValue(obj) as ICollection)
                {
                    Reflect(item);
                }
            }
            else if (!field.FieldType.IsPrimitive)
            {
                Debug.Log($"Field: {field.Name} Is not primitive");
                Reflect(field.GetValue(obj));
            }

            Debug.Log("Field Name: " + field.Name + ", Value: " + field.GetValue(obj));
        }
    }
}