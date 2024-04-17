using AutoMapper;
using MyLab.Search.Indexer.Handlers.Delete;
using MyLab.Search.Indexer.Handlers.IndexInstructions;
using MyLab.Search.Indexer.Handlers.Patch;
using MyLab.Search.Indexer.Handlers.Put;

namespace MyLab.Search.Indexer.Handlers
{
    public class CqrsCommandMappingProfile : Profile
    {
        public CqrsCommandMappingProfile()
        {
            CreateMap<DeleteCommand, Model.IndexInstructions>()
                .ForMember
                (
                    i => i.DeleteList, 
                    e => e.MapFrom
                        (
                            (cmd, _) => new[ ]{ cmd.DocumentId }
                        )
                );
            CreateMap<PutCommand, Model.IndexInstructions>()
                .ForMember
                (
                    i => i.PutList, 
                    e => e.MapFrom
                        (
                            (cmd, _) => new[] { cmd.Document }
                        )
                );
            CreateMap<PatchCommand, Model.IndexInstructions>()
                .ForMember
                (
                    i => i.PatchList, 
                    e => e.MapFrom
                        (
                            (cmd, _) => new[] { cmd.Document }
                        )
                );
            CreateMap<IndexInstructionsCommand, Model.IndexInstructions>();
        }
    }
}
