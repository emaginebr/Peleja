namespace Peleja.Infra.Mappings;

using AutoMapper;
using Peleja.Domain.Models;
using Peleja.Infra.Context;

public class CommentMapperProfile : Profile
{
    public CommentMapperProfile()
    {
        CreateMap<Comment, CommentModel>()
            .ForMember(d => d.Replies, opt => opt.MapFrom(s => s.Replies));
        CreateMap<CommentModel, Comment>()
            .ForMember(d => d.Page, opt => opt.Ignore())
            .ForMember(d => d.ParentComment, opt => opt.Ignore())
            .ForMember(d => d.Replies, opt => opt.Ignore())
            .ForMember(d => d.CommentLikes, opt => opt.Ignore());
    }
}
