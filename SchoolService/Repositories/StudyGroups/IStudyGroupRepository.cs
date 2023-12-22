using SchoolService.Models;
using static SchoolService.Databases.SchoolDb;

using Cancel = System.Threading.CancellationToken;

namespace SchoolService.Repositories.StudyGroups;

public interface IStudyGroupRepository : IDisposable
{
    Task<Id> CreateStudyGroupAsync(StudyGroup studyGroup, Cancel cancellationToken);
    Task<IEnumerable<StudyGroup>> GetStudyGroupsAsync(Cancel cancellationToken);
    Task<IEnumerable<User>> GetStudyGroupUsersAsync(int studyGroupId, Cancel cancellationToken);
    Task JoinStudyGroupAsync(int studyGroupId, int userId, Cancel cancellationToken);
    Task LeaveStudyGroupAsync(int studyGroupId, int userId, Cancel cancellationToken);
    Task<IEnumerable<StudyGroup>> SearchStudyGroupsAsync(Subject subject, Cancel cancellationToken);
}