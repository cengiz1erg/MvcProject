using System;
using System.Collections.Generic;
using System.Globalization;
using AutoMapper;
using CSG.Models.Payment;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Configuration;
using MUsefulMethods;

namespace CSG.Services.Payment
{
    public class IyzicoPaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IyzicoPaymentOptions _options;
        private readonly IMapper _mapper;

        public IyzicoPaymentService(IConfiguration configuration, IMapper mapper)
        {
            _mapper = mapper;
            _configuration = configuration;
            var section = _configuration.GetSection(IyzicoPaymentOptions.Key);
            _options = new IyzicoPaymentOptions()
            {
                ApiKey = section["ApiKey"],
                SecretKey = section["SecretKey"],
                BaseUrl = section["BaseUrl"],
                ThreedsCallbackUrl = section["ThreedsCallbackUrl"],
            };
        }

        public PaymentResponseModel Pay(PaymentModel model)
        {
            CreatePaymentRequest request = this.InitialPaymentRequest(model);
            var payment = Iyzipay.Model.Payment.Create(request, _options);
            return _mapper.Map<PaymentResponseModel>(payment);
        }

        private CreatePaymentRequest InitialPaymentRequest(PaymentModel model)
        {
            var paymentRequest = new CreatePaymentRequest
            {
                Installment = model.Installment,
                Locale = Locale.TR.ToString(),
                ConversationId = GenerateConversationId(),
                Price = model.Price.ToString(new CultureInfo("en-US")),
                PaidPrice = model.PaidPrice.ToString(new CultureInfo("en-US")),
                Currency = Currency.TRY.ToString(),
                BasketId = StringHelpers.GenerateUniqueCode(),
                PaymentChannel = PaymentChannel.WEB.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                PaymentCard = _mapper.Map<PaymentCard>(model.CardModel),
                Buyer = _mapper.Map<Buyer>(model.Customer),
                BillingAddress = _mapper.Map<Address>(model.Address),
                ShippingAddress = _mapper.Map<Address>(model.Address),
                
            };

            var basketItems = new List<BasketItem>();

            foreach (var basketModel in model.BasketList)
            {
                basketItems.Add(_mapper.Map<BasketItem>(basketModel));
            }

            paymentRequest.BasketItems = basketItems;

            return paymentRequest;
        }

        public InstallmentModel CheckInstallments(string binNumber, decimal price)
        {
            var conversationId = GenerateConversationId();

            var request = new RetrieveInstallmentInfoRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = conversationId,
                BinNumber = binNumber,
                Price = price.ToString(new CultureInfo("en-US")),
            };

            var result = InstallmentInfo.Retrieve(request, _options);

            if (result.Status == "failure")
            {
                throw new Exception(result.ErrorMessage);
            }

            if (result.ConversationId != conversationId)
            {
                throw new Exception("Hatalı istek oluturuldu");
            }

            InstallmentModel resultModel = _mapper.Map<InstallmentModel>(result.InstallmentDetails[0]);

            System.Console.WriteLine();
            return resultModel;
        }

        private string GenerateConversationId()
        {
            return StringHelpers.GenerateUniqueCode();
        }
    }
}