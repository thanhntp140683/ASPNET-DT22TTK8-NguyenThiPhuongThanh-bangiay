using ShoeStore.Models;
using ShoeStore.Models.Momo;

namespace ShoeStore.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfo model);
        Task<MomoExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection);
    }
}
