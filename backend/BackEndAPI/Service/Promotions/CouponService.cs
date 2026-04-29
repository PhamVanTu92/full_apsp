using BackEndAPI.Data;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static Azure.Core.HttpHeader;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.Promotions
{
    public class CouponService : Service<Coupon>, ICouponService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CouponService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Coupon> AddCouponAsync(Coupon model)
        {
            if(string.IsNullOrEmpty(model.CouponCode))
            {
                var codes = await GenerateByCode("PHCP", "", 10, model);
                model.CouponCode = codes;
            }
            model.CouponLine = null;
            _context.Coupon.Add(model);
            await _context.SaveChangesAsync();
            return model;

        }

        public async Task<(IEnumerable<Coupon>, int total)> GetCouponAsync(int skip, int limit)
        {
            var query = _context.Set<Coupon>().AsQueryable();
            var totalCount = await query.CountAsync();
            var items = await query.Skip(skip * limit).Take(limit).Include(p => p.CouponLine).Include(p=>p.CouponItem).ToListAsync();
            return (items, totalCount);
        }

        public async Task<Coupon> GetCouponByIdAsync(int id)
        {
            return await _context.Coupon
                    .AsNoTracking()
                    .Include(p => p.CouponLine)
                    .Include(p => p.CouponItem)
                    .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Coupon> UpdateCouponAsync(int id, Coupon model)
        {
            var coupon = await _context.Coupon
           .AsNoTracking()
           .Include(p => p.CouponItem)
           .FirstOrDefaultAsync(p => p.Id == id);
            if (coupon == null)
            {
                return null;
            }
            if (id != model.Id)
                return null;
            var dtoType = model.GetType();
            var entityType = coupon.GetType();

            foreach (var prop in dtoType.GetProperties())
            {
                var dtoValue = prop.GetValue(model);
                if (dtoValue != null)
                {
                    var entityProp = entityType.GetProperty(prop.Name);
                    if (!prop.Name.Equals("CreatedDate"))
                    {
                        if (entityProp != null)
                        {
                            entityProp.SetValue(coupon, dtoValue);
                        }
                    }
                }
            }
            foreach (var couponItems in model.CouponItem)
            {
                if (couponItems.Status.Equals("D"))
                {
                    var couponItem = coupon.CouponItem.FirstOrDefault(i => i.Id == couponItems.Id);
                    if (couponItem != null)
                    {
                        _context.CouponItem.Remove(couponItem);
                        coupon.CouponItem.Remove(couponItem);
                    }
                }
                else if (couponItems.Status.Equals("U"))
                {
                    var couponItem = coupon.CouponItem.FirstOrDefault(i => i.Id == couponItems.Id);
                    if (couponItem != null)
                    {
                        couponItem.Type = couponItems.Type;
                        couponItem.ItemId = couponItems.ItemId;
                        couponItem.ItemCode = couponItems.ItemCode;
                        couponItem.ItemName = couponItems.ItemName;
                        couponItem.ItmsGrpName = couponItems.ItmsGrpName;
                        couponItem.ItemGroupId = couponItems.ItemGroupId;
                    }
                }
                else if (couponItems.Status.Equals("A"))
                {
                    coupon.CouponItem.Add(new CouponItem
                    {
                        Type = couponItems.Type,
                        ItemId = couponItems.ItemId,
                        ItemCode = couponItems.ItemCode,
                        ItemName = couponItems.ItemName,
                        ItmsGrpName = couponItems.ItmsGrpName,
                        ItemGroupId = couponItems.ItemGroupId
                    });
                }
            }
            _context.Coupon.Update(coupon);
            await _context.SaveChangesAsync();
            var couponDTO = await GetCouponByIdAsync(coupon.Id);
            return couponDTO;
        }
    }
    public class CouponLineService : Service<CouponLine>, ICouponLineService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly Random _random = new Random();
        public CouponLineService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor; 
        }

        public async Task<IEnumerable<CouponLine>> CreateCouponLineAsync(CouponCodeRule model)
        {
            var coupon = await _context.Coupon
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == model.CouponId);
            if (coupon == null)
                return null;

            for (int i = 0; i < model.Quantity; i++)
            {
                string couponCode;
                do
                {
                    couponCode = model.StartChar+GenerateRandomCode(model.Length-model.StartChar.Length - model.EndChar.Length) + model.EndChar;
                } while (await _context.CouponLine.AnyAsync(c => c.CouponCode == model.StartChar+couponCode+ model.EndChar));
                var CouponLine = new CouponLine
                {
                    CouponId = model.CouponId,
                    CouponCode = couponCode,
                    Status = "NU"
                };

                _context.CouponLine.Add(CouponLine);
            }

            await _context.SaveChangesAsync();
            var (coupons , total)= await GetCouponLineAsync(0,30, model.CouponId);
            return (coupons);
        }
        private string GenerateRandomCode(int length)
        {
            var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringBuilder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(characters[_random.Next(characters.Length)]);
            }

            return stringBuilder.ToString();
        }
        public async Task<(IEnumerable<CouponLine>, int total)> GetCouponLineAsync(int skip = 0, int limit = 30, int id = 0, string Status = "", string couponCode = "")
        {
            var query = _context.Set<CouponLine>().AsQueryable();
            var items = await query.Where(p => p.CouponId == id 
                                &&(Status == "" ? true : p.Status == Status)
                                && (couponCode == "" ? true : p.CouponCode.Contains(couponCode))
                                )
                            .Skip(skip * limit).Take(limit).ToListAsync();
            var totalCount = await query.Where(p => p.CouponId == id
                                && (Status == "" ? true : p.Status == Status)
                                && (couponCode == "" ? true : p.CouponCode.Contains(couponCode))
                                ).CountAsync();
            return (items, totalCount);
        }

        public async Task<(IEnumerable<CouponLine>, Mess)> UpdateCouponLineAysnc(int id, string Status,List<CouponLineView> model)
        {
            Mess mess = new Mess();
            if(Status == "R")
            {
                var couponLineDTO = model.Select(c => c.Id).ToList();
                var couponLine = await _context.CouponLine
                    .AsNoTracking()
                    .Where(c => couponLineDTO.Contains(c.Id) && c.Status != "NU").ToListAsync();
                var couponLineCheck = await _context.CouponLine
                    .AsNoTracking()
                    .Where(c => couponLineDTO.Contains(c.Id)).ToListAsync();
                if (couponLine.Count > 0)
                {
                    mess.Status = 800;
                    mess.Errors = "Existing status different 'Not Use'";
                    return (null, mess);
                }    
                if(couponLineDTO.Count != couponLineCheck.Count)
                {
                    mess.Status = 801;
                    mess.Errors = "CouponLine Id not in database";
                    return (null, mess);
                }   
                foreach(var c in couponLineCheck)
                {
                    c.Status = "R";
                    c.ReleaseDate = DateTime.Now;
                }
                _context.CouponLine.UpdateRange(couponLineCheck);
                await _context.SaveChangesAsync();
                var (coupons, total) = await GetCouponLineAsync(0, 30, id);
                return (coupons, null);
            }
            else if(Status == "C")
            {
                var couponLineDTO = model.Select(c => c.Id).ToList();
                var couponLine = await _context.CouponLine
                    .AsNoTracking()
                    .Where(c => couponLineDTO.Contains(c.Id) && c.Status == "U").ToListAsync();
                var couponLineCheck = await _context.CouponLine
                   .AsNoTracking()
                   .Where(c => couponLineDTO.Contains(c.Id)).ToListAsync();
                if (couponLine.Count > 0)
                {
                    mess.Status = 800;
                    mess.Errors = "Coupon is used";
                    return (null, mess);
                }
                if (couponLineDTO.Count != couponLineCheck.Count)
                {
                    mess.Status = 801;
                    mess.Errors = "CouponLine Id not in database";
                    return (null, mess);
                }
                foreach (var c in couponLineCheck)
                {
                    c.Status = "C";
                }
                _context.CouponLine.UpdateRange(couponLineCheck);
                await _context.SaveChangesAsync();
                var (coupons, total) = await GetCouponLineAsync(0, 30, id);
                return (coupons, null);
            }
            else
            {
                mess.Status = 801;
                mess.Errors = "Not Existing Status";
                return (null, mess);
            }
        }
    }
}
