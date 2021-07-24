using Droid.Core;

namespace Droid.Framework.Async
{
    public interface INetworkSystem
    {
        void ServerSendReliableMessage(int clientNum, BitMsg msg);
        void ServerSendReliableMessageExcluding(int clientNum, BitMsg msg);
        int ServerGetClientPing(int clientNum);
        int ServerGetClientPrediction(int clientNum);
        int ServerGetClientTimeSinceLastPacket(int clientNum);
        int ServerGetClientTimeSinceLastInput(int clientNum);
        int ServerGetClientOutgoingRate(int clientNum);
        int ServerGetClientIncomingRate(int clientNum);
        float ServerGetClientIncomingPacketLoss(int clientNum);
        void ClientSendReliableMessage(BitMsg msg);
        int ClientPrediction { get; }
        int ClientTimeSinceLastPacket { get; }
        int ClientOutgoingRate { get; }
        int ClientIncomingRate { get; }
        float ClientIncomingPacketLoss { get; }
    }

    class NetworkSystemLocal : INetworkSystem
    {
        public void ServerSendReliableMessage(int clientNum, BitMsg msg)
        {
            if (AsyncNetwork.server.IsActive)
                AsyncNetwork.server.SendReliableGameMessage(clientNum, msg);
        }

        public void ServerSendReliableMessageExcluding(int clientNum, BitMsg msg)
        {
            if (AsyncNetwork.server.IsActive)
                AsyncNetwork.server.SendReliableGameMessageExcluding(clientNum, msg);
        }

        public int ServerGetClientPing(int clientNum)
            => AsyncNetwork.server.IsActive ? AsyncNetwork.server.GetClientPing(clientNum) : 0;

        public int ServerGetClientPrediction(int clientNum)
            => AsyncNetwork.server.IsActive ? AsyncNetwork.server.GetClientPrediction(clientNum) : 0;

        public int ServerGetClientTimeSinceLastPacket(int clientNum)
            => AsyncNetwork.server.IsActive ? AsyncNetwork.server.GetClientTimeSinceLastPacket(clientNum) : 0;

        public int ServerGetClientTimeSinceLastInput(int clientNum)
            => AsyncNetwork.server.IsActive ? AsyncNetwork.server.GetClientTimeSinceLastInput(clientNum) : 0;

        public int ServerGetClientOutgoingRate(int clientNum)
            => AsyncNetwork.server.IsActive ? AsyncNetwork.server.GetClientOutgoingRate(clientNum) : 0;

        public int ServerGetClientIncomingRate(int clientNum)
            => AsyncNetwork.server.IsActive ? AsyncNetwork.server.GetClientIncomingRate(clientNum) : 0;

        public float ServerGetClientIncomingPacketLoss(int clientNum)
            => AsyncNetwork.server.IsActive ? AsyncNetwork.server.GetClientIncomingPacketLoss(clientNum) : 0f;

        public void ClientSendReliableMessage(BitMsg msg)
        {
            if (AsyncNetwork.client.IsActive)
                AsyncNetwork.client.SendReliableGameMessage(msg);
            else if (AsyncNetwork.server.IsActive)
                AsyncNetwork.server.LocalClientSendReliableMessage(msg);
        }

        public int ClientPrediction
            => AsyncNetwork.client.IsActive ? AsyncNetwork.client.Prediction : 0;

        public int ClientTimeSinceLastPacket
            => AsyncNetwork.client.IsActive ? AsyncNetwork.client.TimeSinceLastPacket : 0;

        public int ClientOutgoingRate
            => AsyncNetwork.client.IsActive ? AsyncNetwork.client.OutgoingRate : 0;

        public int ClientIncomingRate
            => AsyncNetwork.client.IsActive ? AsyncNetwork.client.IncomingRate : 0;

        public float ClientIncomingPacketLoss
            => AsyncNetwork.client.IsActive ? AsyncNetwork.client.IncomingPacketLoss : 0f;
    }

    //extern idNetworkSystem *	networkSystem;
}
