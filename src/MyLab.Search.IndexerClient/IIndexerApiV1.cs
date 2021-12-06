using System.Threading.Tasks;
using MyLab.ApiClient;

namespace MyLab.Search.IndexerClient
{
    [Api("v1", Key = "indexer")]
    public interface IIndexerApiV1
    {
        [Post("{ns}")]
        Task IndexAsync([Path] string ns, [JsonContent] object entity);

        [Post("{ns}/{ent_id}/kick")]
        Task KickIndexAsync([Path] string ns, [Path("ent_id")] string id);
    }
}
