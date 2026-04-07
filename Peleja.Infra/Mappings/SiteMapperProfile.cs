namespace Peleja.Infra.Mappings;

using AutoMapper;
using Peleja.Domain.Models;
using Peleja.Infra.Context;

public class SiteMapperProfile : Profile
{
    public SiteMapperProfile()
    {
        CreateMap<Site, SiteModel>().ReverseMap();
    }
}
