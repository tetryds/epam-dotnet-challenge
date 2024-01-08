using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolService.Databases;
using SchoolService.Exceptions;
using SchoolService.Models;
using static SchoolService.Databases.SchoolDb;

using Cancel = System.Threading.CancellationToken;

namespace SchoolService.Repositories.StudyGroups;

public class StudyGroupRepository(SchoolDb db) : IStudyGroupRepository
{
    public async Task<Id> CreateStudyGroupAsync(StudyGroup studyGroup, Cancel cancel)
    {
        ArgumentNullException.ThrowIfNull(studyGroup, nameof(studyGroup));

        var user = await db.Users.FindAsync([studyGroup.OwnerId], cancellationToken: cancel) ?? throw new NotFoundException("user", studyGroup.OwnerId);

        studyGroup.CreateDate = DateTime.UtcNow;

        var entityEntry = await db.StudyGroups.AddAsync(studyGroup, cancel);
        user.OwnedGroups.Add(entityEntry.Entity);
        await db.SaveChangesAsync(cancel);
        return entityEntry.Entity.Id;
    }

    public async Task<IEnumerable<StudyGroup>> GetStudyGroupsAsync(Cancel cancel)
    {
        return await db.StudyGroups.ToListAsync(cancel);
    }

    public async Task<IEnumerable<User>> GetStudyGroupUsersAsync(int studyGroupId, Cancel cancel)
    {
        var studyGroup = await db.StudyGroups.Include(s => s.Users).FirstAsync(s => s.Id == studyGroupId, cancellationToken: cancel);
        return studyGroup.Users ?? throw new NotFoundException("studyGroup", studyGroupId);
    }

    public async Task JoinStudyGroupAsync(int studyGroupId, int userId, Cancel cancel)
    {
        var user = await db.Users.FindAsync([userId], cancellationToken: cancel) ?? throw new NotFoundException("user", userId);

        var studyGroup = await db.StudyGroups.FindAsync([studyGroupId], cancellationToken: cancel) ?? throw new NotFoundException("studyGroup", studyGroupId);

        studyGroup.Users.Add(user);
        user.StudyGroups.Add(studyGroup);

        await db.SaveChangesAsync(cancel);
    }

    public async Task LeaveStudyGroupAsync(int studyGroupId, int userId, Cancel cancel)
    {
        var user = await db.Users.FindAsync([userId], cancellationToken: cancel) ?? throw new NotFoundException("user", userId);

        var studyGroup = await db.StudyGroups.Include(s => s.Users).FirstAsync(s => s.Id == studyGroupId, cancellationToken: cancel) ?? throw new NotFoundException("studyGroup", studyGroupId);

        if (user.Id == studyGroup.OwnerId)
        {
            db.StudyGroups.Remove(studyGroup);
        }
        else
        {
            studyGroup.Users.Remove(user);
            user.StudyGroups.Remove(studyGroup);
        }

        await db.SaveChangesAsync(cancel);
    }

    public async Task<IEnumerable<StudyGroup>> SearchStudyGroupsAsync(Subject subject, Cancel cancel)
    {
        return await db.StudyGroups.Where(s => s.Subject == subject).ToListAsync(cancel);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        db.Dispose();
    }
}