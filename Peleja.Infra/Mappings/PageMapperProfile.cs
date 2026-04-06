namespace Peleja.Infra.Mappings;

using AutoMapper;
using Peleja.Domain.Models;
using Peleja.Infra.Context;

public class PageMapperProfile : Profile
{
    public PageMapperProfile()
    {
        CreateMap<Page, PageModel>().ReverseMap()
            .ForMember(d => d.Comments, opt => opt.Ignore());
    }
}
