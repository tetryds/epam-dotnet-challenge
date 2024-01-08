using System.Net.Http.Json;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SchoolService.Databases;
using SchoolService.Models.Dto;
using SchoolService.Models;
using SchoolService.Repositories;
using SchoolServiceTests.Clients;
using SchoolServiceTests.Tools;
using static SchoolService.Databases.SchoolDb;

namespace SchoolServiceTests.Tests.Integration;

// Let's ignore these null warning because if a null ref happens we want the test to blow up anyway
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning disable CS8604 // Possible null reference argument.

[TestFixture]
public class EndToEndTests
{
    static WebApplicationFactory<Program> factory;

    [OneTimeSetUp]
    public static void OneTimeSetUp()
    {
        factory = new WebApplicationFactory<Program>();
        DbTools.EnsureDbCleared(factory);
    }

    [OneTimeTearDown]
    public static void OneTimeTearDown()
    {
        factory.Dispose();
    }

    // Create users, create groups, join groups, leave groups, leave groups as owner
    [Test]
    public async Task StudyGroupLifetime()
    {
        HttpClient httpClient = factory.CreateClient();
        var userClient = new UserClient(httpClient);
        var client = new StudyGroupClient(httpClient);

        List<User> users = [new User { Name = "User 1" }, new User { Name = "User 2" }, new User { Name = "User 3" }];


        // Create all users
        foreach (var user in users)
        {
            user.Id = await userClient.CreateUser(user);
        }

        List<CreateStudyGroupDto> groupDtos =
        [
            new CreateStudyGroupDto { Name = "Group 1", Subject = Subject.Math, OwnerId = users[0].Id },
            new CreateStudyGroupDto { Name = "Group 2", Subject = Subject.Chemistry, OwnerId = users[1].Id },
            new CreateStudyGroupDto { Name = "Group 3", Subject = Subject.Math, OwnerId = users[2].Id },
            new CreateStudyGroupDto { Name = "Group 4", Subject = Subject.Physics, OwnerId = users[2].Id },
        ];

        // Create all groups
        foreach (var groupDto in groupDtos)
        {
            await client.CreateStudyGroup(groupDto);
        }

        List<StudyGroup> initialGroups = (await client.GetStudyGroups()).ToList();

        Assert.That(initialGroups, Has.Count.EqualTo(groupDtos.Count));

        // Each group only has its owner
        foreach (var group in initialGroups)
        {
            var groupUsers = (await client.GetStudyGroupUsers(group.Id)).ToList();
            Assert.That(groupUsers, Has.Count.EqualTo(1));
        }

        // Users 2 and 3 join Group 1
        await client.JoinStudyGroup(initialGroups[0].Id, users[1].Id);
        await client.JoinStudyGroup(initialGroups[0].Id, users[2].Id);

        // Verify that group 1 users are User 1 (owner), User 2 and User 3
        List<string> expectedGroup1UserNames = [users[0].Name, users[1].Name, users[2].Name];
        List<User> group1Users = (await client.GetStudyGroupUsers(initialGroups[0].Id)).ToList();

        CollectionAssert.AreEquivalent(expectedGroup1UserNames, group1Users.Select(u => u.Name));

        // User 2 leaves Group 1
        await client.LeaveStudyGroup(initialGroups[0].Id, users[1].Id);

        // Verify that group 1 users are User 1 (owner) and User 3
        expectedGroup1UserNames = [users[0].Name, users[2].Name];
        List<User> newGroup1Users= (await client.GetStudyGroupUsers(initialGroups[0].Id)).ToList();

        CollectionAssert.AreEquivalent(expectedGroup1UserNames, newGroup1Users.Select(u => u.Name));

        // User 3 joins Group 2
        await client.JoinStudyGroup(initialGroups[1].Id, users[2].Id);

        // Verify that Group 2 users are User 2 (owner) and user 3
        List<string> expectedGroup2UserNames = [users[1].Name, users[2].Name];
        List<User> group2Users = (await client.GetStudyGroupUsers(initialGroups[1].Id)).ToList();

        CollectionAssert.AreEquivalent(expectedGroup2UserNames, group2Users.Select(u => u.Name));

        // Owner of Group 2 leaves
        await client.LeaveStudyGroup(initialGroups[1].Id, users[1].Id);

        // Group 2 should be destroyed
        List<StudyGroup> groups = (await client.GetStudyGroups()).ToList();
        Assert.That(groups, Has.Count.EqualTo(groupDtos.Count - 1));
        
        // Fetch all groups and verify that Group 2 was destroyed
        List<string> expectedGroupNames = initialGroups.Select(x => x.Name).Where(n => n != "Group 2").ToList();
        List<string> groupNames = groups.Select(x => x.Name).ToList();

        CollectionAssert.AreEquivalent(expectedGroupNames, groupNames);

        // Filter math groups and verify that they match up
        List<string> expectedMathGroupNames = groups.Where(s => s.Subject == Subject.Math).Select(s => s.Name).ToList();
        List<string> mathGroupNames = (await client.SearchStudyGroups(Subject.Math)).Select(s => s.Name).ToList();

        CollectionAssert.AreEquivalent(expectedMathGroupNames, mathGroupNames);
    }
}

#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8604 // Possible null reference argument.