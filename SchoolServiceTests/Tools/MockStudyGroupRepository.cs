using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolService.Databases;
using SchoolService.Models;
using SchoolService.Repositories;
using SchoolService.Repositories.StudyGroups;
using static SchoolService.Databases.SchoolDb;

namespace SchoolServiceTests.Tools;

public class MockStudyGroupRepository(MockSchoolDb db) : IStudyGroupRepository
{
    public async Task<Id> CreateStudyGroupAsync(StudyGroup studyGroup, CancellationToken cancellationToken)
    {
        //Ensure unique id
        do
        {
            studyGroup.Id = Random.Shared.Next();
        }
        while (!db.StudyGroups.TryAdd(studyGroup.Id, studyGroup));

        db.Users[studyGroup.OwnerId].StudyGroups.Add(studyGroup);

        return studyGroup.Id;
    }

    public void Dispose() { }

    public async Task<IEnumerable<StudyGroup>> GetStudyGroupsAsync(CancellationToken cancellationToken)
    {
        return db.StudyGroups.Values.ToList();
    }

    public async Task<IEnumerable<User>> GetStudyGroupUsersAsync(int studyGroupId, CancellationToken cancellationToken)
    {
        return db.StudyGroups[studyGroupId].Users.ToList();
    }

    public async Task JoinStudyGroupAsync(int studyGroupId, int userId, CancellationToken cancellationToken)
    {
        var studyGroup = db.StudyGroups[studyGroupId];
        var user = db.Users[userId];

        studyGroup.Users.Add(user);
        user.StudyGroups.Add(studyGroup);
    }

    public async Task LeaveStudyGroupAsync(int studyGroupId, int userId, CancellationToken cancellationToken)
    {
        var studyGroup = db.StudyGroups[studyGroupId];
        var user = db.Users[userId];

        if (studyGroup.OwnerId == userId)
        {
            foreach (var guest in studyGroup.Users.ToList())
            {
                studyGroup.Users.Remove(guest);
                guest.StudyGroups.Remove(studyGroup);
            }

            db.StudyGroups.Remove(studyGroupId);
        }
        else
        {
            studyGroup.Users.Remove(user);
            user.StudyGroups.Remove(studyGroup);

        }

    }

    public async Task<IEnumerable<StudyGroup>> SearchStudyGroupsAsync(Subject subject, CancellationToken cancellationToken)
    {
        return db.StudyGroups.Values.Where(s => s.Subject == subject).ToList();
    }
}