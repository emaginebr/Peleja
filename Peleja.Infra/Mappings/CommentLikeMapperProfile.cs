namespace Peleja.Infra.Mappings;

using AutoMapper;
using Peleja.Domain.Models;
using Peleja.Infra.Context;

public class CommentLikeMapperProfile : Profile
{
    public CommentLikeMapperProfile()
    {
        CreateMap<CommentLike, CommentLikeModel>().ReverseMap()
            .ForMember(d => d.Comment, opt => opt.Ignore());
    }
}
