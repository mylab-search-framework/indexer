using System.Net;
using System.Threading.Tasks;
using MyLab.ApiClient;

namespace FuncTests
{
    [Api]
    public interface IApiKickerContract
    {
        [Post]
        [ExpectedCode(HttpStatusCode.NotFound)]
        Task KickAsync();
    }
}