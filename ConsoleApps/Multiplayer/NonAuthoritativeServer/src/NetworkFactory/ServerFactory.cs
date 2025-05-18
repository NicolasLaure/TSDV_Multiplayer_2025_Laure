using Network.Messages;
using Network.Utilities;

namespace Network
{
    public class ServerFactory : NetworkFactory
    {
        private readonly Dictionary<int, InstanceData> heldInstanceIdToData = new Dictionary<int, InstanceData>();

        public void BroadcastInstantiation(InstanceData instanceData, NonAuthoritativeServer server)
        {
            SaveInstance(ref instanceData);
            heldInstanceIdToData.Add(instanceData.instanceID, instanceData);
            server.Broadcast(new InstantiateRequest(instanceData).Serialize());
        }

        public void BroadcastDeInstantiation(int clientId, int instanceId, NonAuthoritativeServer server)
        {
            if (instanceIdToInstanceData.ContainsKey(instanceId) && instanceIdToInstanceData[instanceId].originalClientID == clientId)
            {
                server.Broadcast(new DeInstantiateRequest(instanceId).Serialize());
                RemoveInstance(instanceId);
            }
        }

        public void BroadcastDeInstantiation(int instanceId, NonAuthoritativeServer server)
        {
            server.Broadcast(new DeInstantiateRequest(instanceId).Serialize());
            RemoveInstance(instanceId);
        }

        public InstantiateAll GetObjectsToInstantiate()
        {
            List<InstanceData> instanceDatas = new List<InstanceData>();
            foreach (var instanceData in instanceIdToInstanceData.Values)
            {
                instanceDatas.Add(instanceData);
            }

            return new InstantiateAll(instanceDatas);
        }

        public void CheckIntegrity(List<InstanceData> instanceDatas, int instanceId, NonAuthoritativeServer server)
        {
            bool isIntegral = true;
            for (int i = 0; isIntegral && i < instanceDatas.Count; i++)
            {
                for (int j = i; j < instanceDatas.Count; j++)
                {
                    if (instanceDatas[i] == instanceDatas[j]) continue;

                    Logger.LogError($"Instances Weren't similar, First Id was {instanceDatas[i].instanceID} Second Id was {instanceDatas[j].instanceID}");
                    isIntegral = false;
                    break;
                }
            }

            if (isIntegral && heldInstanceIdToData.ContainsKey(instanceId))
            {
                Logger.Log("Instance Was Integral");
                heldInstanceIdToData.Remove(instanceId);
                return;
            }

            BroadcastDeInstantiation(instanceId, server);
            BroadcastInstantiation(heldInstanceIdToData[instanceId], server);
            heldInstanceIdToData.Remove(instanceId);
        }
    }
}