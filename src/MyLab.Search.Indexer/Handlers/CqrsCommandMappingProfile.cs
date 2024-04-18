using AutoMapper;
using MyLab.Search.Indexer.Handlers.IndexingRequest;

namespace MyLab.Search.Indexer.Handlers
{
    public class CqrsCommandMappingProfile : Profile
    {
        public CqrsCommandMappingProfile()
        {
            CreateMap<IndexingRequestCommand, Model.IndexInstructions>();
        }
    }
}
