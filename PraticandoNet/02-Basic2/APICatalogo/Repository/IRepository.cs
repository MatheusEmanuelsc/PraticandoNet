using System.Linq.Expressions;

namespace APICatalogo.Repository
{
    public interface IRepository<T>
    {
        T? Get(Expression<Func<T,bool>>predicate);
        IEnumerable<T> GetAll();
        T Create(T entity);
        T Update(T entity);
        T Delete(T entity);
    }
}
