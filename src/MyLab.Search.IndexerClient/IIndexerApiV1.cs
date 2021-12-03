using System.Threading.Tasks;
using MyLab.ApiClient;

namespace MyLab.Search.IndexerClient
{
    [Api("v1")]
    public interface IIndexerApiV1
    {
        [Post("{job}")]
        Task IndexAsync([Path] string job, [JsonContent] object entity);

        [Post("{job}/{id}/kick")]
        Task KickIndexAsync([Path] string job, [Path] string id);
    }
}
