using System.Net;

public interface IReceiveData
{
    void OnReceiveData(byte[] inputdata, IPEndPoint ipEndpoint);
}