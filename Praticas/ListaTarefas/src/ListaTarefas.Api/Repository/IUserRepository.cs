using ListaTarefas.Api.Entities;

namespace ListaTarefas.Api.Repository;

public interface IUserRepository
{
    Task<ApplicationUser> GetByIdAsync(string id);
    Task<ApplicationUser> GetByEmailAsync(string email);
    Task AddAsync(ApplicationUser user);
    Task UpdateAsync(ApplicationUser user);
}