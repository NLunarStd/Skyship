public interface INetworkSync 
{
    // for item or character that can be synchronized across network, and will be updated to all clients when it's changed, e.g. player, enemy, item, etc.
     void Sync();
}
