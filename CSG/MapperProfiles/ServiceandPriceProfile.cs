using AutoMapper;
using CSG.Models.Entities;
using CSG.ViewModels;

namespace CSG.MapperProfiles
{
    public class ServiceandPriceProfile:Profile
    {
        public ServiceandPriceProfile()
        {
            CreateMap<ServiceAndPrice , ServicePriceViewModel>().ReverseMap();
        }
    }
}
