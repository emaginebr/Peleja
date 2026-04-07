namespace Peleja.Domain.Mappings;

using AutoMapper;
using Peleja.Domain.Models;
using Peleja.DTO;

public class PageResultProfile : Profile
{
    public PageResultProfile()
    {
        CreateMap<PageModel, PageResult>()
            .ForMember(d => d.CommentCount, opt => opt.Ignore());
    }
}
