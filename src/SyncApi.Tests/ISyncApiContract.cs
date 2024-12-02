using System.Text.Json;
using System.Text.Json.Nodes;
using MyLab.ApiClient;

namespace SyncApi.Tests
{
    [Api]
    public interface ISyncApiContract
    {
        Task<CallDetails> PutAsync(string idxId, JsonNode document);
        Task<CallDetails> PatchAsync(string idxId, JsonNode document);
        Task<CallDetails> DeleteAsync(string idxId, string docId);
    }
}
