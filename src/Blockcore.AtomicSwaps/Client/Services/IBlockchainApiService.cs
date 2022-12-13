namespace Blockcore.AtomicSwaps.Client.Services
{
    /// <summary>
    /// A class that will handle communication with the indexers.
    /// </summary>
    public interface IBlockchainApiService
    {
        Task<int> GetConfirmationsAsync(string network, string trxId, string? trxHex = null);

        Task Broadcast(string network, string? trxHex);

    }
}