using BackEndAPI.Data;
using BackEndAPI.Models.Branchs;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using static Azure.Core.HttpHeader;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.Branchs
{
    public class BranchService : Service<Branch>, IBranchService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IModelUpdater _modelUpdater;
        Mess mess = new Mess();
        public BranchService(AppDbContext context, IModelUpdater modelUpdater, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _modelUpdater = modelUpdater;
        }

        public async Task<(Branch, Mess)> CreateBranchAsync(Branch model)
        {
            return (null, null);
            //using (var transaction = _context.Database.BeginTransaction())
            //{
            //    try
            //    {
            //        await _context.Branch.AddAsync(model);
            //        await _context.SaveChangesAsync();
            //        var oitms = await _context.Item.AsNoTracking().ToListAsync();
            //        if(oitms !=null)
            //        {
            //            List<OITW> listOITW = new List<OITW>();
            //            foreach (var oitm in oitms)
            //            {
            //                OITW oitw = new OITW();
            //                oitw.BranchId = model.Id;
            //                oitw.BranchName = model.BranchName;
            //                oitw.ItemId = oitm.Id;
            //                oitw.ItemName = oitm.ItemName;
            //                oitw.ItemCode = oitm.ItemCode;
            //                listOITW.Add(oitw);
            //            }
            //            if (listOITW != null)
            //            {
            //                await _context.OITW.AddRangeAsync(listOITW);
            //            } 
                            
            //        }    
                      
            //        await _context.SaveChangesAsync();
            //        transaction.Commit();
            //        return (model, null);
            //    }
            //    catch (Exception ex)
            //    {
            //        mess.Status = 900;
            //        mess.Errors = ex.Message;
            //        transaction.Rollback(); 
            //        return (null, mess);
            //    }
            //}  
        }

        public  async Task<(IEnumerable<Branch>,Mess, int total)> GetAllBranchAsync(int skip, int limit)
        {
            try
            {
                var query = _context.Set<Branch>().AsQueryable();
                var totalCount = await query.CountAsync();
                var items = await query.Include(p => p.BranchAddress)
                                .Skip(skip * limit).Take(limit)
                                .ToListAsync();
                return (items,null, totalCount);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess,0);
            }
           
        }

        public async  Task<(IEnumerable<Branch>, Mess)> GetAllBranchAsync()
        {
            try
            {
                var query = _context.Set<Branch>().AsQueryable();
                var items = await query.Include(p => p.BranchAddress)
                                .ToListAsync();
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<Branch>,Mess)> GetBranchAsync(string search)
        {
            Mess mess = new Mess();
            try
            {
                var items = await _context.Branch
                    .Where(p=>p.BranchName.Contains(search))
                    .ToListAsync();
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(Branch, Mess)> GetBranchByIdAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var items = await _context.Branch
                    .AsNoTracking()
                    .Include(p => p.BranchAddress)
                    .FirstOrDefaultAsync(p => p.Id == id);
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }

        }

        public async Task<(Branch, Mess)> UpdateBranchAsync(int id, Branch model)
        {
            Mess mess = new Mess();
            try
            {
                if (model.Status == "A" || model.Status == "D")
                {
                    var branch = await _context.Branch
                    .AsNoTracking()
                    .Include(p => p.BranchAddress)
                    .FirstOrDefaultAsync(p => p.Id == id);
                    if (branch == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Không tìm thấy bản ghi để cập nhập";
                        return (null, mess);
                    }
                    if (id != model.Id)
                    {
                        mess.Status = 400;
                        mess.Errors = "Không tìm thấy bản ghi để cập nhập";
                        return (null, mess);
                    }
                    _modelUpdater.UpdateModel(branch, model, "CreatedDate", "BranchAddress", "Na", "NA", "Na", "NA");
                    foreach (var branchAddress in model.BranchAddress.ToList())
                    {
                        var branchAddress1 = branch.BranchAddress
                        .FirstOrDefault(c => c.Id == branchAddress.Id);
                        if (branchAddress1 != null)
                        {
                            if (string.IsNullOrEmpty(branchAddress.Status))
                            { }
                            else if (branchAddress.Status.Equals("D"))
                            {
                                _context.BranchAddress.Remove(branchAddress1);
                                branch.BranchAddress.Remove(branchAddress1);
                            }
                            else if (branchAddress.Status.Equals("U"))
                            {
                                _modelUpdater.UpdateModel(branchAddress1, branchAddress, "A", "B", "Na", "NA", "Na", "NA");
                            }
                        }
                        else
                            branch.BranchAddress.Add(branchAddress);
                    }
                    _context.Branch.Update(branch);
                    await _context.SaveChangesAsync();
                    return (branch, null);
                }
                else
                {
                    mess.Status = 600;
                    mess.Errors = "Trạng thái cập nhập không đúng";
                    return (null, mess);
                }   
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(Branch, Mess)> UpdateBranchAsync(int id, string status)
        {
            Mess mess = new Mess();
            try
            {
                if(status == "A" || status == "D")
                {
                    
                    var branch = await _context.Branch
                    .AsNoTracking()
                    .Include(p => p.BranchAddress)
                    .FirstOrDefaultAsync(p => p.Id == id);
                    if (branch == null)
                    {
                        mess.Status = 400;
                        mess.Errors = "Không tìm thấy bản ghi để cập nhập";
                        return (null, mess);
                    }
                    branch.Status = status;
                    _context.Branch.Update(branch);
                    await _context.SaveChangesAsync();
                    return (branch, null);
                }
                else
                {
                    mess.Status = 600;
                    mess.Errors = "Trạng thái cập nhập không đúng";
                    return (null, mess);
                }
                
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }
    }

    public class BranchAddressService : Service<BranchAddress>, IBranchAddressService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IModelUpdater _modelUpdater;
        Mess mess = new Mess();
        public BranchAddressService(AppDbContext context, IModelUpdater modelUpdater, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _modelUpdater = modelUpdater;
        }

        public async Task<(BranchAddress, Mess)> CreateBranchAddressAsync(BranchAddress model)
        {
            try
            {
                _context.BranchAddress.Add(model);
                await _context.SaveChangesAsync();
                return (model, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<BranchAddress>, Mess, int total)> GetAllBranchAddressAsync(int id, int skip, int limit)
        {
            try
            {
                var query = _context.Set<BranchAddress>().AsQueryable();
                var totalCount = await query.Where(p => p.BranchId == id).CountAsync();
                var items = await query.Where(p=>p.BranchId == id).Skip(skip * limit).Take(limit)
                                .ToListAsync();
                return (items, null, totalCount);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess, 0);
            }
        }

        public async Task<(BranchAddress, Mess)> UpdateBranchAddressAsync(int id, BranchAddress model)
        {
            Mess mess = new Mess();
            try
            {
                var branchAdd = await _context.BranchAddress
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
                if (branchAdd == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tìm thấy bản ghi để cập nhập";
                    return (null, mess);
                }
                if (id != model.Id)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tìm thấy bản ghi để cập nhập";
                    return (null, mess);
                }
                _modelUpdater.UpdateModel(branchAdd, model, "NA", "NA", "Na", "NA", "Na", "NA");
                _context.BranchAddress.Update(branchAdd);
                await _context.SaveChangesAsync();
                return (branchAdd, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }
    }
    }
