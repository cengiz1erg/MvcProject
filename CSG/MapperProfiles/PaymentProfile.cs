using AutoMapper;
using CSG.Models.Payment;
using Iyzipay.Model;

namespace CSG.MapperProfiles
{
    public class PaymentProfile:Profile
    {
        public PaymentProfile()
        {
            CreateMap<CardModel, PaymentCard>().ReverseMap();
            CreateMap<BasketModel, BasketItem>().ReverseMap();
            CreateMap<AddressModel, Address>().ReverseMap();
            CreateMap<CustomerModel, Buyer>().ReverseMap();
            CreateMap<InstallmentPriceModel, InstallmentPrice>().ReverseMap();
            CreateMap<InstallmentModel, InstallmentDetail>().ReverseMap();
            CreateMap<PaymentResponseModel, Payment>().ReverseMap();
        }
    }
}
