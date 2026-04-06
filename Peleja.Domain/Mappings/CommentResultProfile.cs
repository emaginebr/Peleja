namespace Peleja.Domain.Mappings;

using AutoMapper;
using Peleja.Domain.Models;
using Peleja.DTO;

public class CommentResultProfile : Profile
{
    public CommentResultProfile()
    {
        CreateMap<CommentModel, CommentResult>()
            .ForMember(d => d.Replies, opt => opt.MapFrom(s => s.Replies));
    }
}
