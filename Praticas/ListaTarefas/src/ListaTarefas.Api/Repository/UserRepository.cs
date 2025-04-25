using ListaTarefas.Api.Context;
using ListaTarefas.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ListaTarefas.Api.Repository;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser> GetByIdAsync(string id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<ApplicationUser> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(ApplicationUser user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task UpdateAsync(ApplicationUser user)
    {
        _context.Users.Update(user);
    }
}