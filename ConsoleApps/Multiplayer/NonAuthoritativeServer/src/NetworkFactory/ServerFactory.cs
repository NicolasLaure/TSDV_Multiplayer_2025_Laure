using Network.Messages;
using Network.Utilities;

namespace Network
{
    public class ServerFactory : NetworkFactory
    {
        public void BroadcastInstantiation(InstanceData instanceData, NonAuthoritativeServer server)
        {
            SaveInstance(instanceData);
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

        public InstantiateAll GetObjectsToInstantiate()
        {
            List<InstanceData> instanceDatas = instanceIdToInstanceData.Values.ToList();
            return new InstantiateAll(instanceDatas);
        }
    }
}