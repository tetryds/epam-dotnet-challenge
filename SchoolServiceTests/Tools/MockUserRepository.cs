using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolService.Databases;
using SchoolService.Models;
using SchoolService.Repositories;
using SchoolService.Repositories.StudyGroups;
using SchoolService.Repositories.Users;
using static SchoolService.Databases.SchoolDb;

namespace SchoolServiceTests.Tools;

public class MockUserRepository(MockSchoolDb db) : IUserRepository
{
    public async Task<Id> CreateUser(User user, CancellationToken cancellationToken)
    {
        //Ensure unique id
        do
        {
            user.Id = Random.Shared.Next();
        }
        while (!db.Users.TryAdd(user.Id, user));

        return user.Id;
    }


    public async Task<IEnumerable<StudyGroup>> GetStudyGroups(int userId, CancellationToken none)
    {
        return db.Users[userId].StudyGroups.ToList();
    }

    public async Task<User> GetUser(int id, CancellationToken cancel)
    {
        return db.Users[id];
    }

    public async Task<IEnumerable<User>> GetUsers(CancellationToken cancel)
    {
        return db.Users.Values.ToList();
    }

    public void Dispose() { }
}