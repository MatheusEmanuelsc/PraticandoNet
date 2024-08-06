using Curso.Api.Context;
using Curso.Api.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Curso.Api.Repositorys
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;

        public Repository(AppDbContext context){
            _context = context;
        }

        public T Create(T entity)
        {
            _context.Set<T>().Add(entity);
            
            return entity;
        }

        public T Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
            return entity;
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
        {
           return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync();
           
        }

        public T Update(T entity)
        {
            _context.Set<T>().Update(entity);
            return entity;
        }

        public async Task<PagedList<T>> GetPagedAsync(PaginationParameters paginationParameters)
        {
        var source = _context.Set<T>().AsQueryable();
        var count = await source.CountAsync();
        var items = await source.Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
                                .Take(paginationParameters.PageSize)
                                .ToListAsync();

        return new PagedList<T>(items, count, paginationParameters.PageNumber, paginationParameters.PageSize);
        }
    }
}
