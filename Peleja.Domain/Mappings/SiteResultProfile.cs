namespace Peleja.Domain.Mappings;

using AutoMapper;
using Peleja.Domain.Models;
using Peleja.DTO;

public class SiteResultProfile : Profile
{
    public SiteResultProfile()
    {
        CreateMap<SiteModel, SiteResult>();
    }
}
