using BackEndAPI.Data;
using BackEndAPI.Models.BusinessPartners;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.Other;
using Gridify;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Service.Articles
{
    public class ArticleService : Service<Article>, IArticleService
    {
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ArticleService(AppDbContext context, IHostingEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(Article, Mess)> CreateAsync(ArticleView article)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var articles = _context.Article.ToList();
                    if(articles.Count>0)
                    {
                        articles.ForEach(e => e.Status = "D");
                        _context.Article.UpdateRange(articles);
                        await _context.SaveChangesAsync();
                    }
                    string filePatch = "";
                    if (article.File != null)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "uploads");
                        Directory.CreateDirectory(uploadsFolder);
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(article.File.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await article.File.CopyToAsync(fileStream);
                        }

                        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
                        if (request != null)
                        {
                            var baseUrl = $"{request.Scheme}://{request.Host}";
                            var fileUrl = $"{baseUrl}/uploads/{fileName}";
                            filePatch = fileUrl;
                        }
                    }
                    var user = _httpContextAccessor.HttpContext?.User;
                    var claims = user.Identity as ClaimsIdentity;
                    var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var creator = _context.Users.FirstOrDefault(x => x.Id == int.Parse(userId)).FullName;
                    var articlea = new Article
                    {
                        Name = article.Name,
                        Creator = creator,
                        CreateDate = DateTime.UtcNow,
                        Status = "A",
                        Note = article.Note,
                        FilePath = filePatch
                    };
                    _context.Article.Add(articlea);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return (articlea, null);
                }
                catch (Exception ex)
                {
                    Mess mess = new Mess();
                    mess.Status = 900;
                    mess.Errors = ex.Message;
                    transaction.Rollback();
                    return (null, mess);
                }
            }    
                
        }

        public async Task<(List<Article>,int, Mess)> GetAllAsync(GridifyQuery q)
        {
            try
            {
                var query = _context.Set<Article>().AsQueryable();
                var totalCount = await query.ApplyFiltering(q).CountAsync();
                var items = await query.AsSplitQuery()
                    .ApplyFiltering(q)
                    .ApplyPaging(q)
                    .ToListAsync();
                return (items, totalCount, null);
            }
            catch (Exception ex)
            {
                Mess mess = new Mess();
                mess.Status = 900;
                mess.Errors = ex.Message;
                return (null,0, mess);
            }
        }
    }
}
