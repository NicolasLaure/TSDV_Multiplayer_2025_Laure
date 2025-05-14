// using System;
//
// namespace Network
// {
//     public class ServerFactory : NetworkFactory
//     {
//         public override T Instantiate<T>(T objectToInstantiate, InstanceData instanceData)
//         {
//             throw new NotImplementedException();
//         }
//
//         public override void BroadcastInstantiation<T>(T objectToInstantiate, InstanceData instanceData)
//         {
//             NonAuthoritativeServer.Instance.Broadcast(new InstantiateRequest(instanceData).Serialize());
//         }
//
//         public override T DeInstantiate<T>(T objectToInstantiate)
//         {
//             throw new NotImplementedException();
//         }
//     }
// }