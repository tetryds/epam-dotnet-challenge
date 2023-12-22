using SchoolService.Models;
using static SchoolService.Databases.SchoolDb;

using Cancel = System.Threading.CancellationToken;

namespace SchoolService.Repositories.Users;

public interface IUserRepository : IDisposable
{
    Task<Id> CreateUser(User user, Cancel cancellationToken);
    Task<User> GetUser(int id, Cancel cancel);
    Task<IEnumerable<User>> GetUsers(Cancel cancel);
    Task<IEnumerable<StudyGroup>> GetStudyGroups(int userId, Cancel none);
}