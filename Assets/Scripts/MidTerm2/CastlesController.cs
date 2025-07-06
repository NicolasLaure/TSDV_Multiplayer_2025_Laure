using MidTerm2.Model;
using MidTerm2.View;
using Network.Factory;
using Reflection;
using UnityEngine;

namespace MidTerm2
{
    public class CastlesController : MonoBehaviourSingleton<CastlesController>
    {
        public CastlesModel model;
        public ReflectiveFactory<CastlesModel> factory;
        public ReflectionHandler<CastlesModel> reflection;

        public void NextTurn()
        {
            model?.ChangeTurn();
        }

        public void SelectWarrior(GameObject warriorObject)
        {
            if (factory == null || model == null || !model.isPlayerTurn)
                return;

            Debug.Log("WarriorClicked");
            factory.TryGetInstanceId(warriorObject, out int instanceId, out int originalClientId);
            if (originalClientId == CastlesClient.Instance.clientId && factory.TryGetObjectRoute(instanceId, out int[] route))
            {
                if (reflection.GetDataAt(route).GetType() == typeof(Warrior))
                {
                    model.selectedWarrior = reflection.GetDataAt(route) as Warrior;
                    Debug.Log($"Selected Warrior At {model.selectedWarrior.position}");
                }
            }
        }
    }
}