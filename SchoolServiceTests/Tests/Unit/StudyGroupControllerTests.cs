using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SchoolService.Databases;
using SchoolService.Models;
using SchoolService.Models.Dto;
using SchoolService.Repositories;
using SchoolServiceTests.Tools;
using static SchoolService.Controllers.StudyGroupController;
using static SchoolService.Controllers.StudyGroupController.GroupUserOperation;
using static SchoolService.Databases.SchoolDb;

namespace SchoolServiceTests.Tests.Unit;

public class StudyGroupControllerTests
{
    private const int NoOwner = -1;

    private static readonly TestCaseData[] validGroups =
    [
        new TestCaseData(new CreateStudyGroupDto { OwnerId = NoOwner, Subject = Subject.Math, Name = "01234" }).SetDescription("5 chars ok"),
        new TestCaseData(new CreateStudyGroupDto { OwnerId = NoOwner, Subject = Subject.Math, Name = "012345678901234" }).SetDescription("15 chars ok"),
        new TestCaseData(new CreateStudyGroupDto { OwnerId = NoOwner, Subject = Subject.Math, Name = "012345678901234567890123456789" }).SetDescription("30 chars ok"),
        new TestCaseData(new CreateStudyGroupDto { OwnerId = NoOwner, Subject = Subject.Math, Name = "01234" }).SetDescription("Math ok"),
        new TestCaseData(new CreateStudyGroupDto { OwnerId = NoOwner, Subject = Subject.Physics, Name = "01234" }).SetDescription("Physics ok"),
        new TestCaseData(new CreateStudyGroupDto { OwnerId = NoOwner, Subject = Subject.Chemistry, Name = "01234" }).SetDescription("Chemistry ok"),
    ];

    [TestCaseSource(nameof(validGroups))]
    public async Task StudyGroupCanBeCreated(CreateStudyGroupDto group)
    {
        var mockDb = new MockSchoolDb();
        var user = new User { Id = 1 };
        mockDb.Users.Add(user.Id, user);

        var controller = new StudyGroupController(new MockStudyGroupRepository(mockDb), new MockUserRepository(mockDb));
        group.OwnerId = user.Id;
        var result = await controller.CreateStudyGroup(group);

        StudyGroup savedStudyGroup = mockDb.StudyGroups.Single().Value;
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf(typeof(OkObjectResult)));
            Assert.That(savedStudyGroup.Name, Is.EqualTo(group.Name));
            Assert.That(savedStudyGroup.Subject, Is.EqualTo(group.Subject));
            Assert.That(savedStudyGroup.OwnerId, Is.EqualTo(group.OwnerId));
            Assert.That(mockDb.StudyGroups, Has.Count.EqualTo(1));
        });
    }

    private static readonly TestCaseData[] inValidGroups =
    [
        new TestCaseData(new CreateStudyGroupDto { OwnerId = NoOwner, Subject = Subject.Math, Name = "" }).SetDescription("0 chars not ok"),
        new TestCaseData(new CreateStudyGroupDto { OwnerId = NoOwner, Subject = Subject.Math, Name = "0" }).SetDescription("1 char not ok"),
        new TestCaseData(new CreateStudyGroupDto { OwnerId = NoOwner, Subject = Subject.Math, Name = "01" }).SetDescription("2 chars not ok"),
        new TestCaseData(new CreateStudyGroupDto { OwnerId = NoOwner, Subject = Subject.Math, Name = "012" }).SetDescription("3 chars not ok"),
        new TestCaseData(new CreateStudyGroupDto { OwnerId = NoOwner, Subject = Subject.Math, Name = "0123" }).SetDescription("4 chars not ok"),
        new TestCaseData(new CreateStudyGroupDto { OwnerId = NoOwner, Subject = Subject.Math, Name = "0123456789012345678901234567890" }).SetDescription("31 chars not ok"),
        new TestCaseData(new CreateStudyGroupDto { OwnerId = NoOwner, Subject = Subject.Math, Name = "01234567890123456789012345678901" }).SetDescription("32 chars not ok"),
        new TestCaseData(new CreateStudyGroupDto { OwnerId = NoOwner, Subject = (Subject)3, Name = "01234" }).SetDescription("Only Math, Chemistry and Physics are valid"),
    ];

    [TestCaseSource(nameof(inValidGroups))]
    public void InvalidStudyGroupsCannotBeCreated(CreateStudyGroupDto group)
    {
        var mockDb = new MockSchoolDb();
        var user = new User { Id = 1 };
        mockDb.Users.Add(user.Id, user);
        var controller = new StudyGroupController(new MockStudyGroupRepository(mockDb), new MockUserRepository(mockDb));
        group.OwnerId = user.Id;
        Assert.ThrowsAsync<ValidationException>(() => controller.CreateStudyGroup(group));

        Assert.That(mockDb.StudyGroups, Has.Count.EqualTo(0));
    }

    [Test]
    public void UserCanCreateOneGroupForEachSubject()
    {
        var group = new CreateStudyGroupDto { OwnerId = NoOwner, Subject = Subject.Math, Name = "01234" };

        var mockDb = new MockSchoolDb();
        var user = new User { Id = 1 };
        mockDb.Users.Add(user.Id, user);
        var controller = new StudyGroupController(new MockStudyGroupRepository(mockDb), new MockUserRepository(mockDb));
        group.OwnerId = user.Id;

        foreach (var subject in Enum.GetValues<Subject>())
        {
            group.Subject = subject;
            Assert.DoesNotThrowAsync(() => controller.CreateStudyGroup(group));
            Assert.ThrowsAsync<ValidationException>(() => controller.CreateStudyGroup(group));
        }

        Assert.That(mockDb.StudyGroups, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task StudyGroupsAreProperlySorted()
    {
        var mockDb = new MockSchoolDb();
        var user = new User { Id = 1 };

        var group1 = new StudyGroup { Id = 1, OwnerId = NoOwner, Subject = Subject.Math, Name = "01234", CreateDate = DateTime.Now.AddDays(-1.0d) };
        var group2 = new StudyGroup { Id = 2, OwnerId = NoOwner, Subject = Subject.Math, Name = "01234", CreateDate = DateTime.Now };

        mockDb.Users.Add(user.Id, user);
        mockDb.StudyGroups.Add(group1.Id, group1);
        mockDb.StudyGroups.Add(group2.Id, group2);

        var controller = new StudyGroupController(new MockStudyGroupRepository(mockDb), new MockUserRepository(mockDb));

        // Newest First
        var newestFirst = await controller.GetStudyGroups(SortBy.Newest);
        IEnumerable<StudyGroup> studyGroups = FetchGroups(newestFirst);

        Assert.That(studyGroups.ElementAt(0), Is.EqualTo(group2));
        Assert.That(studyGroups.ElementAt(1), Is.EqualTo(group1));

        // Oldest First
        var oldestFirst = await controller.GetStudyGroups(SortBy.Oldest);
        studyGroups = FetchGroups(oldestFirst);

        Assert.That(studyGroups.ElementAt(0), Is.EqualTo(group1));
        Assert.That(studyGroups.ElementAt(1), Is.EqualTo(group2));
    }

    [Test]
    public async Task UserCanLeaveGroup()
    {
        var mockDb = new MockSchoolDb();
        var ownerUser = new User { Id = 1 };
        var guestUser = new User { Id = 2 };

        var group = new StudyGroup { Id = 1, OwnerId = ownerUser.Id, Subject = Subject.Math, Name = "01234", CreateDate = DateTime.Now };
        group.Users.Add(ownerUser);

        mockDb.Users.Add(ownerUser.Id, ownerUser);
        mockDb.Users.Add(guestUser.Id, guestUser);
        mockDb.StudyGroups.Add(group.Id, group);

        Assert.NotNull(group.Users);

        var controller = new StudyGroupController(new MockStudyGroupRepository(mockDb), new MockUserRepository(mockDb));

        await controller.ModifyGroupUser(group.Id, new GroupUserOperation(guestUser.Id, GroupOperation.Join));

        Assert.That(group.Users.Count, Is.EqualTo(2));

        await controller.ModifyGroupUser(group.Id, new GroupUserOperation(guestUser.Id, GroupOperation.Leave));

        Assert.That(group.Users.Count, Is.EqualTo(1));
        Assert.That(group.Users[0], Is.EqualTo(ownerUser));
    }

    [Test]
    public async Task OwnerLeavingDestroysGroup()
    {
        var mockDb = new MockSchoolDb();
        var owner = new User { Id = 1 };
        var guest = new User { Id = 2 };

        var group = new StudyGroup { Id = 1, OwnerId = owner.Id, Subject = Subject.Math, Name = "01234", CreateDate = DateTime.Now };
        group.Users.Add(owner);

        mockDb.Users.Add(owner.Id, owner);
        mockDb.Users.Add(guest.Id, guest);
        mockDb.StudyGroups.Add(group.Id, group);

        Assert.NotNull(group.Users);

        var controller = new StudyGroupController(new MockStudyGroupRepository(mockDb), new MockUserRepository(mockDb));

        await controller.ModifyGroupUser(group.Id, new GroupUserOperation(guest.Id, GroupOperation.Join));

        Assert.That(group.Users.Count, Is.EqualTo(2));

        await controller.ModifyGroupUser(group.Id, new GroupUserOperation(owner.Id, GroupOperation.Leave));

        var studyGroups = FetchGroups(await controller.GetStudyGroups());

        Assert.That(studyGroups.Count(), Is.EqualTo(0));
    }

    private static IEnumerable<StudyGroup> FetchGroups(IActionResult newestFirst)
    {
        var okResult = newestFirst as OkObjectResult;
        Assert.NotNull(okResult);
        var studyGroups = okResult.Value as IEnumerable<StudyGroup>;
        Assert.NotNull(studyGroups);
        return studyGroups;
    }
}