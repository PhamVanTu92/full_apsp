using BackEndAPI.Data;
using BackEndAPI.Models.ItemGroups;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Unit;
using Microsoft.EntityFrameworkCore;
using System;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.Unit
{
    public class PackingService : Service<Packing>, IPackingService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PackingService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }
        public static readonly string[] VietNamChar = new string[]
        {
            "aAeEoOuUiIdDyY",
            "áàạảãâấầậẩẫăắằặẳẵ",
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ",
            "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ",
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ",
            "ÚÙỤỦŨƯỨỪỰỬỮ",
            "íìịỉĩ",
            "ÍÌỊỈĨ",
            "đ",
            "Đ",
            "ýỳỵỷỹ",
            "ÝỲỴỶỸ"
        };
        public static string LocDau(string str)
        {
            //Thay thế và lọc dấu từng char      
            for (int i = 1; i < VietNamChar.Length; i++)
            {
                for (int j = 0; j < VietNamChar[i].Length; j++)
                    str = str.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
            }
            return str;
        }
        public async Task<(bool, Mess)> SyncPackingAsync(List<Packing> models)
        {
            Mess mes = new Mess();
            try
            {
                foreach(var model in models)
                {
                    var packing = await _context.Packing.FirstOrDefaultAsync(p => p.SapId == model.SapId);
                    if (packing != null)
                    {
                        if(packing.Code!= model.Code || packing.Name != model.Name || packing.Volumn != model.Volumn || packing.Type != model.Type)
                        {
                            packing.Code = model.Code;
                            packing.Name = model.Name;
                            packing.Volumn = model.Volumn;
                            packing.Type = model.Type;
                            _context.Packing.Update(packing);
                            await _context.SaveChangesAsync();
                        }    
                    }    
                    packing = await _context.Packing.FirstOrDefaultAsync(p => p.SapId == null && (p.Name == model.Name || p.Code == model.Code));
                    if (packing != null)
                    {
                        packing.SapId = model.SapId;
                        packing.Name = model.Name;
                        packing.Code = model.Code;
                        packing.Volumn = model.Volumn;
                        packing.Type = model.Type;
                        _context.Packing.Update(packing);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        packing = await _context.Packing.FirstOrDefaultAsync(p => p.SapId == model.SapId &&p.Name == model.Name);
                        if (packing == null)
                        {
                            _context.Packing.Add(model);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            packing.SapId = model.SapId;
                            packing.Name = model.Name;
                            packing.Code = model.Code;
                            packing.Volumn = model.Volumn;
                            packing.Type = model.Type;
                            _context.Packing.Update(packing);
                            await _context.SaveChangesAsync();
                        }
                    }
                } 
                return (true, null);
            }
            catch (Exception ex)
            {
                mes.Status = 900;
                mes.Errors = ex.Message;
                return (false, mes);
            }
        }
    }
}
