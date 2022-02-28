using AutoMapper;
using CSG.Models.Identity;
using CSG.ViewModels;

namespace CSG.MapperProfiles
{
    public class AccountProfile:Profile
    {
        public AccountProfile()
        {
            CreateMap<ApplicationUser, UserProfileViewModel>().ReverseMap();
            CreateMap<ApplicationUser, RegisterLoginViewModel>().ReverseMap();
        }
    }
}
