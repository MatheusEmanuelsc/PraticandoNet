using System.Linq.Expressions;
using Curso.Api.Pagination;

namespace Curso.Api.Repositorys
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>>GetAllAsync();
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
        T Create(T entity);
        T Update(T entity);
        T Delete(T entity);

         
        Task<PagedList<T>> GetPagedAsync(PaginationParameters paginationParameters);
    }
}
