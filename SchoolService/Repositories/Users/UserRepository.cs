using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolService.Databases;
using SchoolService.Exceptions;
using SchoolService.Models;
using static SchoolService.Databases.SchoolDb;

using Cancel = System.Threading.CancellationToken;

namespace SchoolService.Repositories.Users;

public class UserRepository(SchoolDb db) : IUserRepository
{
    public async Task<Id> CreateUser(User user, Cancel cancel)
    {
        var entityEntry = await db.Users.AddAsync(user);
        await db.SaveChangesAsync(cancel);
        return entityEntry.Entity.Id;
    }

    public async Task<User?> GetUser(int userId, Cancel cancel)
    {
        return await db.Users.FindAsync(userId, cancel);
    }

    public async Task<IEnumerable<User>> GetUsers(Cancel cancel)
    {
        return await db.Users.ToListAsync(cancel);
    }

    public async Task<IEnumerable<StudyGroup>> GetStudyGroups(int userId, Cancel cancel)
    {
        var user = await db.Users.Include(u => u.StudyGroups).FirstAsync(u => u.Id == userId, cancellationToken: cancel) ?? throw new NotFoundException("user", userId);
        return user.StudyGroups;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        db.Dispose();
    }
}