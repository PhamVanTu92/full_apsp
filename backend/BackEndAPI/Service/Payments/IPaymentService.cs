using BackEndAPI.Models.Banks;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;

namespace BackEndAPI.Service.Payments
{
    public interface IPaymentService
    {
        Task<(Payment,Mess)> AddPaymentAsync(PaymentDto paymentDto);
        Task<(IEnumerable<Payment> t, int total, double BeginBalance, double InComingPayment, double OutGoingPayment, double EndBalance, Mess)> GetAllPaymentWithPaginationAsync(int skip, int limit, int branchId);
        Task<Payment> GetPaymentById(int id);
    }
}
