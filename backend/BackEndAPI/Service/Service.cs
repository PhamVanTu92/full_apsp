using BackEndAPI.Data;
using BackEndAPI.Models.Banks;
using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace BackEndAPI.Service
{
    public class Service<T> : IService<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;
        public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<AppDbContext, Task<TResult>> action)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Thực hiện tác vụ truyền vào (action) và lấy kết quả trả về
                var result = await action(_context);

                // Commit transaction nếu không có lỗi xảy ra
                await transaction.CommitAsync();

                // Trả về kết quả của action
                return result;
            }
            catch (Exception)
            {
                // Rollback nếu có ngoại lệ
                await transaction.RollbackAsync();
                throw;
            }
        }
        public Service(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task<IEnumerable<T>> GetAllAsync(int skip, int limit)
        {
            return await _context.Set<T>().Skip(skip).Take(limit).ToListAsync();
        }
        public async Task<(IEnumerable<T>, Mess)> GetAllAsync()
        {
            Mess mess = new Mess();
            try
            {
                return (await _context.Set<T>().ToListAsync(), null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }

        }
        public async Task<(T, Mess)> GetByIdAsync(int id, params string[] child)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<T>().AsQueryable();
                query = query.Where(e => EF.Property<int>(e, "Id") == id);
                if (child != null)
                    if (child.Length > 0)
                        for (int i = 0; i < child.Length; i++)
                        {
                            query = query.Include(child[i]);
                        }
                var items = await query.FirstOrDefaultAsync();
                return (items, null);
            } catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }
        static bool IsNullable<T>(T obj)
        {
            if (obj == null) return true; // obvious
            Type type = typeof(T);
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }
        public async Task<(T, Mess)> AddAsync(T entity)
        {
            Mess mess = new Mess();
            try
            {
                _dbSet.Add(entity);
                await _context.SaveChangesAsync();
                return (entity, null);
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("Cannot insert duplicate"))
                {
                    mess.Status = 400;
                    mess.Errors = "Dữ liệu bị trùng lặp";
                    return (null, mess);
                }
                if (ex.InnerException.Message.Contains("Cannot insert duplicate key"))
                {
                    mess.Status = 400;
                    mess.Errors = "Dữ liệu bị trùng lặp";
                    return (null, mess);
                }
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }

        }

        public async Task<(T, Mess)> UpdateAsync(int id, T entities)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<T>().AsQueryable();
                query = query.Where(e => EF.Property<int>(e, "Id") == id);
                var entity = await query.FirstOrDefaultAsync();
                if (entity == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tìm thấy bản ghi để cập nhập";
                    return (null, mess);
                }
                var dtoType = entities.GetType();
                var entityType = entity.GetType();
                foreach (var prop in dtoType.GetProperties())
                {
                    var dtoValue = prop.GetValue(entities);
                    if (dtoValue != null)
                    {
                        var entityProp = entityType.GetProperty(prop.Name);
                        if (prop.Name.Equals("CreatedDate")) { }
                        else
                            if (entityProp != null)
                        {
                            entityProp.SetValue(entity, dtoValue);
                        }
                    }
                }
                _context.Set<T>().Update(entity);
                await _context.SaveChangesAsync();
                return (entity, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }

        }

        public async Task<(bool, Mess)> DeleteAsync(int id)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<T>().AsQueryable();
                query = query.Where(e => EF.Property<int>(e, "Id") == id);
                var entity = await query.FirstOrDefaultAsync();
                if (entity == null)
                {
                    mess.Status = 400;
                    mess.Errors = "Không tìm thấy bản ghi để xóa";
                    return (false,mess);
                }
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                return (true,mess);
            }catch(Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (false, mess);
            }
            
        }

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<(IEnumerable<T> t, int total)> GetAllWithPaginationAsync(int skip = 0, int limit = 30)
        {
            var query = _context.Set<T>().AsQueryable();
            var totalCount = await query.CountAsync();
            var items = await query.Skip(skip * limit).Take(limit).ToListAsync();
            return (items, totalCount);
        }

        public async Task<IEnumerable<T>> AddListAsync(List<T> entity)
        {
            _dbSet.AddRange(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<string> GenerateByCode(string prefix, string suffixes, int length, T entity)
        {

            string obj = entity.ToString();
            char[] charArray = obj.ToCharArray();
            Array.Reverse(charArray);
            string objs = new string(charArray);
            string result = objs.Substring(0, objs.IndexOf("."));
            charArray = result.ToCharArray();
            Array.Reverse(charArray);
            objs = new string(charArray);
            var lastCode = "";
            if (string.IsNullOrEmpty(suffixes))
            {
                lastCode = await _context.Set<T>()
               .Where(e => EF.Property<string>(e, objs + "Code").StartsWith(prefix))
               .OrderByDescending(e => EF.Property<string>(e, objs + "Code"))
               .Select(e => EF.Property<string>(e, objs + "Code"))
               .FirstOrDefaultAsync();
                int lastNumber = 0;
                if (!string.IsNullOrEmpty(lastCode))
                {
                    var postfix = lastCode.Substring(prefix.Length);
                    int.TryParse(postfix, out lastNumber);
                }
                int newNumber = lastNumber + 1;
                return prefix + newNumber.ToString().PadLeft(length - prefix.Length, '0');
            }
            else
            {
                lastCode = await _context.Set<T>()
               .Where(e => EF.Property<string>(e, objs + "Code").StartsWith(prefix) && EF.Property<string>(e, objs + "Code").EndsWith(suffixes))
               .OrderByDescending(e => EF.Property<string>(e, objs + "Code"))
               .Select(e => EF.Property<string>(e, objs + "Code"))
               .FirstOrDefaultAsync();
                int lastNumber = 0;
                if (!string.IsNullOrEmpty(lastCode))
                {
                    var postfix = lastCode.Substring(prefix.Length);
                    int.TryParse(postfix, out lastNumber);
                }
                int newNumber = lastNumber + 1;
                return prefix + newNumber.ToString().PadLeft(length - prefix.Length, '0');
            }
        }
        public async Task<string> GenerateDocument(string prefix, string suffixes, int length, T entity)
        {
            var lastCode = "";
            if (string.IsNullOrEmpty(suffixes))
            {
                lastCode = await _context.Set<T>()
               .Where(e => EF.Property<string>(e,  "InvoiceCode").StartsWith(prefix))
               .OrderByDescending(e => EF.Property<string>(e, "InvoiceCode"))
               .Select(e => EF.Property<string>(e,  "InvoiceCode"))
               .FirstOrDefaultAsync();
                int lastNumber = 0;
                if (!string.IsNullOrEmpty(lastCode))
                {
                    var postfix = lastCode.Substring(prefix.Length);
                    int.TryParse(postfix, out lastNumber);
                }
                int newNumber = lastNumber + 1;
                return prefix + newNumber.ToString().PadLeft(length - prefix.Length, '0');
            }
            else
            {
                lastCode = await _context.Set<T>()
               .Where(e => EF.Property<string>(e,"InvoiceCode").StartsWith(prefix) && EF.Property<string>(e,  "InvoiceCode").EndsWith(suffixes))
               .OrderByDescending(e => EF.Property<string>(e, "InvoiceCode"))
               .Select(e => EF.Property<string>(e, "InvoiceCode"))
               .FirstOrDefaultAsync();
                int lastNumber = 0;
                if (!string.IsNullOrEmpty(lastCode))
                {
                    var postfix = lastCode.Substring(prefix.Length);
                    int.TryParse(postfix, out lastNumber);
                }
                int newNumber = lastNumber + 1;
                return prefix + newNumber.ToString().PadLeft(length - prefix.Length, '0');
            }
        }
        public async Task<(IEnumerable<T> t, int total, Mess)> GetAllWithPaginationAsync(int skip, int limit, params string[] child)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<T>().AsQueryable();
                var totalCount = await query.CountAsync();
                if (child != null)
                    if (child.Length > 0)
                        for (int i = 0; i < child.Length; i++)
                        {
                            query = query.Include(child[i]);
                        }

                var items = await query.Skip(skip * limit).Take(limit).ToListAsync();
                return (items, totalCount,null);
            }catch(Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null,0, mess);
            }
            
        }
        public async Task<(IEnumerable<T>, Mess)> GetAsyncs(List<string> child, params string[] search)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<T>().AsQueryable();
                if (search != null)
                    if (search.Length > 0)
                        for (int i = 0; i < search.Length; i += 2)
                        {
                            query = query.Where(e => EF.Property<string>(e, search[i]).Contains(search[i + 1]));
                            
                        }
                if (child != null)
                    if (child.Count > 0)
                        for (int i = 0; i < child.Count; i++)
                        {
                            query = query.Include(child[i]);
                        }
                var items = await query.ToListAsync();
                return (items, null);
            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }
        public async Task<(IEnumerable<T>, Mess)> GetAsync(List<string> child, params string[] search)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<T>().AsQueryable();

                if (search != null && search.Length > 0)
                {
                    var parameter = Expression.Parameter(typeof(T), "e");
                    Expression combinedExpression = null;

                    for (int i = 0; i < search.Length; i += 2)
                    {
                        if (i + 1 >= search.Length) continue; // Ensure there is a value for each field

                        var field = search[i];
                        var value = search[i + 1];

                        // Create the property access expression
                        var property = Expression.Property(parameter, field);

                        // Ensure property is of type string
                        if (property.Type != typeof(string))
                        {
                            throw new ArgumentException($"The field '{field}' must be of type string.");
                        }

                        // Create the contains method call
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        var valueExpression = Expression.Constant(value, typeof(string));
                        var containsExpression = Expression.Call(property, containsMethod, valueExpression);

                        // Create lambda expression for the contains condition
                        var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);

                        // Combine all expressions with OR
                        if (combinedExpression == null)
                        {
                            combinedExpression = lambda.Body;
                        }
                        else
                        {
                            combinedExpression = Expression.OrElse(combinedExpression, lambda.Body);
                        }
                    }

                    if (combinedExpression != null)
                    {
                        var combinedLambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
                        query = query.Where(combinedLambda);
                    }
                }
                return (query, null);

            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }

        public async Task<(IEnumerable<T>, Mess)> GetAllAsync(params string[] child)
        {
            Mess mess = new Mess();
            try
            {
                var query = _context.Set<T>().AsQueryable();
                var totalCount = await query.CountAsync();
                if (child != null)
                    if (child.Length > 0)
                        for (int i = 0; i < child.Length; i++)
                        {
                            query = query.Include(child[i]);
                        }

                var items = await query.ToListAsync();
                return (items, null);

            }
            catch (Exception ex)
            {
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null, mess);
            }
        }
    }
    public class ModelUpdater : IModelUpdater
    {
        public void UpdateModel<TModel, TViewModel>(TModel model, TViewModel viewModel,params string[] param)
        {
            var modelType = typeof(TModel);
            var viewModelType = typeof(TViewModel);

            foreach (var prop in viewModelType.GetProperties())
            {
                var dtoValue = prop.GetValue(viewModel);
                if (dtoValue != null)
                {
                    var entityProp = modelType.GetProperty(prop.Name);
                    if (prop.Name.Equals(param[0]) || prop.Name.Equals(param[1]) || prop.Name.Equals(param[2])
                                    | prop.Name.Equals(param[3]) || prop.Name.Equals(param[4]) || prop.Name.Equals(param[5])) { }
                    else
                    {
                        if (entityProp != null)
                        {
                            entityProp.SetValue(model, dtoValue);
                        }
                    }
                }    
            }
        }
    }
    
}
