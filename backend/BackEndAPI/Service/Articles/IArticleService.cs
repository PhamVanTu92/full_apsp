

using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.Articles
{
    public interface IArticleService : IService<Article>
    {
        Task<(List<Article>, int, Mess)> GetAllAsync(GridifyQuery q);
        Task<(Article,Mess)> CreateAsync(ArticleView article);
    }
}
