namespace Peleja.Domain.Mappings;

using AutoMapper;
using Peleja.Domain.Models;
using Peleja.DTO;

public class CommentLikeResultProfile : Profile
{
    public CommentLikeResultProfile()
    {
        CreateMap<CommentModel, CommentLikeResult>()
            .ForMember(d => d.IsLikedByUser, opt => opt.Ignore());
    }
}
