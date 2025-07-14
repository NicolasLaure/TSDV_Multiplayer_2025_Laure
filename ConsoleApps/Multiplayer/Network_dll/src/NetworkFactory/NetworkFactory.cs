using System.Collections.Generic;

namespace Network
{
    public abstract class NetworkFactory
    {
        protected readonly Dictionary<int, InstanceData> instanceIdToInstanceData = new Dictionary<int, InstanceData>();

        private static int instanceId = 0;

        public void SaveInstance(ref InstanceData instanceData)
        {
            instanceData.instanceID = instanceId;
            instanceIdToInstanceData[instanceId] = instanceData;
            instanceId++;
        }

        protected void RemoveInstance(int instanceId)
        {
            if (instanceIdToInstanceData.ContainsKey(instanceId))
                instanceIdToInstanceData.Remove(instanceId);
        }
    }
}