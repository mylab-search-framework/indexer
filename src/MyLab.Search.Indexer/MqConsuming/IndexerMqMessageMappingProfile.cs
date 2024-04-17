using AutoMapper;
using MyLab.Search.Indexer.Model;

namespace MyLab.Search.Indexer.MqConsuming
{
    class IndexerMqMessageMappingProfile : Profile
    {
        public IndexerMqMessageMappingProfile()
        {
            CreateMap<IndexerMqMessage, IndexInstructions>();
        }
    }
}
