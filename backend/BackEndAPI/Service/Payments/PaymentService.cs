using System.Net;
using BackEndAPI.Data;
using BackEndAPI.Infr.Exceptions;
using BackEndAPI.Models.Banks;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using Microsoft.EntityFrameworkCore;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext         _context;
        private readonly IHostingEnvironment  _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentService(AppDbContext context, IHostingEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            _context             = context;
            _webHostEnvironment  = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Payment> GetPaymentById(int id)
        {
            var payment = await _context.Payment.FirstOrDefaultAsync(x => x.Id == id);
            if (payment == null)
            {
                throw new BusinessException($"Payment with id {id} not found", HttpStatusCode.NotFound, "");
            }

            if (!payment.IsChecked)
            {
                PayOne.QueryDRApi(payment.PaymentCode);
            }

            return payment;
        }

        public async Task<(Payment, Mess)> AddPaymentAsync(PaymentDto paymentDto)
        {
            Mess mess = new Mess();
            try
            {
                // if (string.IsNullOrEmpty(model.PaymentCode))
                // {
                //     var codes = "";
                //     if (model.PaymentType.Equals("PC"))
                //     {
                //         codes = await GenerateByCode("PCTT", "", 10, model);
                //         model.PaymentCode = codes;
                //     }
                //     if (model.PaymentType.Equals("PT"))
                //     {
                //         codes = await GenerateByCode("PTTT", "", 10, model);
                //         model.PaymentCode = codes;
                //     }
                // };

                var doc =
                    await _context.ODOC
                        .Where(x => x.Id == paymentDto.DocId)
                        .Include(x => x.Payments)
                        .Include(x => x.BP).FirstOrDefaultAsync();
                if (doc == null) throw new KeyNotFoundException("not found");
                var payment = new Payment();

                if (doc.Payments.Any(e => e.Status == "A"))
                {
                    throw new Exception("Đơn hàng này đã được thanh toán");
                }

                if (doc.Payments.Count == 0)
                {
                    var maxCode = _context.Payment
                        .Where(c => c.PaymentCode.StartsWith("HD"))
                        .OrderByDescending(c => c.PaymentCode)
                        .Select(c => c.PaymentCode)
                        .FirstOrDefault();

                    int newNumber = 1;
                    if (!string.IsNullOrEmpty(maxCode))
                    {
                        var numberPart = maxCode.Substring(4);
                        if (int.TryParse(numberPart, out var currentNumber))
                        {
                            newNumber = currentNumber + 1;
                        }
                    }

                    payment = new Payment
                    {
                        PaymentDate       = DateTime.UtcNow,
                        TotalAmount       = doc.Total,
                        PaymentAmount     = paymentDto.PaymentAmount,
                        PartnerId         = doc.CardId,
                        PartnerName       = doc.CardName,
                        PartnerContactNo  = doc.CardCode,
                        PaymentMethodCode = "PayNow",
                        PaymentMethodName = "OnePay",
                        CreatedDate       = DateTime.UtcNow,
                        DocId             = doc.Id,
                        DocCode           = doc.InvoiceCode,
                        DocType           = doc.ObjType,
                        Crd4              = null,
                    };
                    payment.PaymentCode = $"HD{newNumber:00000}";
                    doc.Payments.Add(payment);
                }
                else
                {
                    payment = doc.Payments.FirstOrDefault(e => e.Status == "P" && e.PaymentMethodCode == "PayNow");
                }


                payment.RedirectLink = PayOne.MerchantSendRequet(new PaymentRequest
                {
                    VpcMerchTxnRef   = payment.PaymentCode,
                    VpcOrderInfo     = doc.Id.ToString(),
                    VpcAmount        = paymentDto.PaymentAmount.ToString() ?? "1",
                    Title            = $"Thanh toán đơn hàng {doc.InvoiceCode}",
                    VpcCustomerPhone = doc.BP?.Phone          ?? "036081286",
                    VpcCustomerEmail = doc.BP?.Email          ?? "demo@gmail.com",
                    VpcCustomerId    = doc?.BP?.Id.ToString() ?? "1",
                });

                // _context.Payment.Add(payment);
                //
                await _context.SaveChangesAsync();
                return (payment, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async
            Task<(IEnumerable<Payment> t, int total, double BeginBalance, double InComingPayment, double OutGoingPayment
                , double EndBalance, Mess)> GetAllPaymentWithPaginationAsync(int skip, int limit, int branchId)
        {
            Mess mess = new Mess();
            try
            {
                if (branchId == 0)
                {
                    var query      = _context.Payment.AsQueryable();
                    var query1     = _context.Payment.AsQueryable();
                    var totalCount = await query.CountAsync();
                    var beginBalance = query1.Where(p => p.PaymentDate.Date < DateTime.Now.Date)
                        .Sum(p => p.PaymentAmount);
                    var inComingPayment = query1.Where(p => p.PaymentDate.Date == DateTime.Now.Date)
                        .Sum(p => p.PaymentAmount);
                    var outGoingPayment = query1.Where(p => p.PaymentDate.Date == DateTime.Now.Date)
                        .Sum(p => p.PaymentAmount);
                    var endBalance = beginBalance + inComingPayment - outGoingPayment;
                    var items      = await query.Skip(skip * limit).Take(limit).ToListAsync();
                    return (items, totalCount, beginBalance, inComingPayment, outGoingPayment, endBalance, null);
                }
                else
                {
                    var query      = _context.Payment.AsQueryable();
                    var query1     = _context.Payment.AsQueryable();
                    var query2     = _context.Payment.AsQueryable();
                    var totalCount = await query.Where(p => p.BranchId == branchId).CountAsync();
                    var beginBalance = query2
                        .Where(p => p.PaymentDate.Date < DateTime.Now.Date && p.BranchId == branchId)
                        .Sum(p => p.PaymentAmount);
                    var inComingPayment = query2
                        .Where(p => p.PaymentDate.Date == DateTime.Now.Date && p.BranchId == branchId)
                        .Sum(p => p.PaymentAmount);
                    var outGoingPayment = query2
                        .Where(p => p.PaymentDate.Date == DateTime.Now.Date && p.BranchId == branchId)
                        .Sum(p => p.PaymentAmount);
                    var endBalance = beginBalance + inComingPayment - outGoingPayment;
                    query1 = query1.Include(p => p.PaymentLine).Where(p => p.BranchId == branchId);
                    var items = await query1.Skip(skip * limit).Take(limit).ToListAsync();
                    return (items, totalCount, beginBalance, inComingPayment, outGoingPayment, endBalance, null);
                }
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, 0, 0, 0, 0, 0, mess);
            }
        }
    }
}