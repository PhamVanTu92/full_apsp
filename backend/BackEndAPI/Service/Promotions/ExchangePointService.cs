using BackEndAPI.Data;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Promotion;
using Gridify;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BackEndAPI.Service.Promotions
{
    public class ExchangePointService : Service<ExchangePoint>, IExchangePointService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ExchangePointService(AppDbContext context, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(ExchangePointReadDto, Mess)> AddExchangePointAsync(ExchangePointCreateDto dto)
        {
            Mess mess = new Mess();
            try
            {
                var packing = _context.Item.Where(e => dto.Lines.Select(e => e.ItemCode).ToArray().Contains(e.ItemCode));
                var entity = new ExchangePoint
                {
                    Name = dto.Name,
                    Note = dto.Note,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    IsActive = dto.IsActive,
                    IsAllCustomer = dto.IsAllCustomer,
                    ExchangePointLine = dto.Lines?.Select(l => new ExchangePointLine
                    {
                        ItemId = l.ItemId,
                        ItemCode = l.ItemCode,
                        ItemName = l.ItemName,
                        PackingId = packing.FirstOrDefault(e => e.ItemCode == l.ItemCode)?.PackingId ?? 0,
                        Point = l.Point
                    }).ToList(),
                    PointCustomer = dto.Customers?.Select(c => new PointCustomer
                    {
                        Type = c.Type,
                        CustomerId = c.CustomerId,
                        CustomerCode = c.CustomerCode,
                        CustomerName = c.CustomerName
                    }).ToList()
                };

                _context.ExchangePoint.Add(entity);
                await _context.SaveChangesAsync();

                entity = await _context.ExchangePoint
                    .Include(e => e.ExchangePointLine).ThenInclude(l => l.Packing)
                    .Include(e => e.PointCustomer)
                    .FirstOrDefaultAsync(e => e.Id == entity.Id);
                var items = entity != null ? MapToReadDto(entity) : null;
                return(items, null) ;
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<ExchangePointReadDto>, int total, Mess)> GetExchangePointAsync(GridifyQuery gridifyQuery)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.ExchangePoint
                .Include(e => e.ExchangePointLine).ThenInclude(l => l.Packing)
                .Include(e => e.PointCustomer)
                .AsQueryable();

                // áp dụng Gridify (lọc, sort, phân trang)
                var filteredQuery = query.ApplyFiltering(gridifyQuery);
                var total = await filteredQuery.CountAsync();
                var sortedQuery = filteredQuery
                     .ApplyOrdering(gridifyQuery)
                     .ApplyPaging(gridifyQuery);

                var result = await sortedQuery.ToListAsync();


                // map sang DTO Read
                var items = result.Select(e => new ExchangePointReadDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Note = e.Note,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsActive = e.IsActive,
                    IsAllCustomer = e.IsAllCustomer,
                    Lines = e.ExchangePointLine?.Select(l => new ExchangePointLineReadDto
                    {
                        Id = l.Id,
                        ItemId = l.ItemId,
                        ItemCode = l.ItemCode,
                        ItemName = l.ItemName,
                        PackingId = l.PackingId,
                        PackingName = l.Packing?.Name,
                        Point = l.Point
                    }).ToList(),
                    Customers = e.PointCustomer?.Select(c => new PointCustomerReadDto
                    {
                        Id = c.Id,
                        Type = c.Type,
                        CustomerId = c.CustomerId,
                        CustomerCode = c.CustomerCode,
                        CustomerName = c.CustomerName
                    }).ToList()
                }).ToList();
                return (items, total, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, 0, mess);
            }
        }

        public async Task<(ExchangePointReadDto, Mess)> GetExchangePointByIdAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var entity = await _context.ExchangePoint
                .Include(e => e.ExchangePointLine).ThenInclude(l => l.Packing)
                .Include(e => e.PointCustomer)
                .FirstOrDefaultAsync(e => e.Id == id);

                if (entity == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tìm thấy bản ghi";
                    return (null, mess);
                }


                var items = new ExchangePointReadDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Note = entity.Note,
                    StartDate = entity.StartDate,
                    EndDate = entity.EndDate,
                    IsActive = entity.IsActive,
                    IsAllCustomer = entity.IsAllCustomer,
                    Lines = entity.ExchangePointLine?.Select(l => new ExchangePointLineReadDto
                    {
                        Id = l.Id,
                        ItemId = l.ItemId,
                        ItemCode = l.ItemCode,
                        ItemName = l.ItemName,
                        PackingId = l.PackingId,
                        PackingName = l.Packing?.Name,
                        Point = l.Point
                    }).ToList(),
                    Customers = entity.PointCustomer?.Select(c => new PointCustomerReadDto
                    {
                        Id = c.Id,
                        Type = c.Type,
                        CustomerId = c.CustomerId,
                        CustomerCode = c.CustomerCode,
                        CustomerName = c.CustomerName
                    }).ToList()
                };
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(ExchangePointReadDto, Mess)> UpdateExchangePointAsync(ExchangePointUpdateDto dto)
        {
            Mess mess = new Mess() ;
            try
            {
                var packing = _context.Item.Where(e => dto.Lines.Select(e => e.ItemCode).ToArray().Contains(e.ItemCode));
                var entity = await _context.ExchangePoint
                .Include(e => e.ExchangePointLine)
                .Include(e => e.PointCustomer)
                .FirstOrDefaultAsync(e => e.Id == dto.Id);

                if (entity == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tìm thấy bản ghi";
                    return (null, mess);
                } 
                entity.Name = dto.Name;
                entity.Note = dto.Note;
                entity.StartDate = dto.StartDate;
                entity.EndDate = dto.EndDate;
                entity.IsActive = dto.IsActive;
                entity.IsAllCustomer = dto.IsAllCustomer;
                entity.ExchangePointLine.Clear();
                entity.PointCustomer.Clear();
                // --------- Update Lines ---------
                if (dto.Lines != null)
                {
                    foreach (var lineDto in dto.Lines)
                    {
                        
                            entity.ExchangePointLine.Add(new ExchangePointLine
                            {
                                FatherId = entity.Id,
                                ItemId = lineDto.ItemId,
                                ItemCode = lineDto.ItemCode,
                                ItemName = lineDto.ItemName,
                                PackingId = packing.FirstOrDefault(e => e.ItemCode == lineDto.ItemCode)?.PackingId ?? 0,
                                Point = lineDto.Point
                            });
                    }
                }
                if (dto.Customers != null)
                {
                    foreach (var cusDto in dto.Customers)
                    {
                        entity.PointCustomer.Add(new PointCustomer
                        {
                            FatherId = entity.Id,
                            Type = cusDto.Type,
                            CustomerId = cusDto.CustomerId,
                            CustomerCode = cusDto.CustomerCode,
                            CustomerName = cusDto.CustomerName
                        });
                    }
                }
                await _context.SaveChangesAsync();

                // return DTO Read
                var item =  await _context.ExchangePoint
                    .Include(e => e.ExchangePointLine).ThenInclude(l => l.Packing)
                    .Include(e => e.PointCustomer)
                    .FirstOrDefaultAsync(e => e.Id == entity.Id);
                var items = item != null ? MapToReadDto(item) : null;
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        private static ExchangePointReadDto MapToReadDto(ExchangePoint entity)
        {
            return new ExchangePointReadDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Note = entity.Note,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                IsActive = entity.IsActive,
                IsAllCustomer = entity.IsAllCustomer,
                Lines = entity.ExchangePointLine?.Select(l => new ExchangePointLineReadDto
                {
                    Id = l.Id,
                    ItemId = l.ItemId,
                    ItemCode = l.ItemCode,
                    ItemName = l.ItemName,
                    PackingId = l.PackingId,
                    PackingName = l.Packing?.Name, // giả sử Packing có Name
                    Point = l.Point
                }).ToList(),
                Customers = entity.PointCustomer?.Select(c => new PointCustomerReadDto
                {
                    Id = c.Id,
                    Type = c.Type,
                    CustomerId = c.CustomerId,
                    CustomerCode = c.CustomerCode,
                    CustomerName = c.CustomerName
                }).ToList()
            };
        }

       
    }
}
