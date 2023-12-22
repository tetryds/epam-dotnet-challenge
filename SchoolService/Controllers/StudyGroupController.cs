using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using SchoolService.Models;
using SchoolService.Models.Dto;
using SchoolService.Repositories.StudyGroups;
using SchoolService.Repositories.Users;
using static SchoolService.Controllers.StudyGroupController.GroupUserOperation;
using static SchoolService.Databases.SchoolDb;

namespace SchoolService.Controllers;

[ApiController]
[Route("[controller]")]
public class StudyGroupController(IStudyGroupRepository studyGroupRepository, IUserRepository userRepository)
{
    [HttpPost]
    public async Task<IActionResult> CreateStudyGroup(CreateStudyGroupDto createStudyGroupDto)
    {
        User owner = await userRepository.GetUser(createStudyGroupDto.OwnerId, CancellationToken.None);
        if (owner == null)
        {
            var result = new ValidationResult($"No user exists with id {createStudyGroupDto.OwnerId}", ["ownerId"]);
            throw new ValidationException(result, null, createStudyGroupDto);
        }

        var newStudyGroup = new StudyGroup
        {
            Name = createStudyGroupDto.Name,
            Subject = createStudyGroupDto.Subject,
            OwnerId = owner.Id,
        };

        Validator.ValidateObject(newStudyGroup, new ValidationContext(newStudyGroup), true);

        if (owner.StudyGroups.Any(s => s.Subject == newStudyGroup.Subject))
        {
            var result = new ValidationResult($"User already owns a study group of subject {newStudyGroup.Subject}", ["subject"]);
            throw new ValidationException(result, null, owner);
        }

        newStudyGroup.Users.Add(owner);
        Id id = await studyGroupRepository.CreateStudyGroupAsync(newStudyGroup, CancellationToken.None);
        
        return new OkObjectResult(id);
    }

    [HttpGet]
    public async Task<IActionResult> GetStudyGroups(SortBy sortBy = SortBy.None)
    {
        var studyGroups = await studyGroupRepository.GetStudyGroupsAsync(CancellationToken.None);
        if (sortBy == SortBy.Newest)
        {
            studyGroups = studyGroups.OrderByDescending(s => s.CreateDate);
        }
        else if (sortBy == SortBy.Oldest)
        {
            studyGroups = studyGroups.OrderBy(s => s.CreateDate);
        }
        return new OkObjectResult(studyGroups);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchStudyGroups(Subject subject, SortBy sortBy = SortBy.None)
    {
        var studyGroups = await studyGroupRepository.SearchStudyGroupsAsync(subject, CancellationToken.None);
        if (sortBy == SortBy.Newest)
        {
            studyGroups = studyGroups.OrderByDescending(s => s.CreateDate);
        }
        else if (sortBy == SortBy.Oldest)
        {
            studyGroups = studyGroups.OrderBy(s => s.CreateDate);
        }

        return new OkObjectResult(studyGroups);
    }

    [HttpPost("{studyGroupId}/users/")]
    public async Task<IActionResult> ModifyGroupUser(int studyGroupId, GroupUserOperation operation)
    {
        switch (operation.Operation)
        {
            case GroupOperation.Join:
                await studyGroupRepository.JoinStudyGroupAsync(studyGroupId, operation.UserId, CancellationToken.None);
                return new OkResult();
            default:
                await studyGroupRepository.LeaveStudyGroupAsync(studyGroupId, operation.UserId, CancellationToken.None);
                return new OkResult();
        }
    }

    [HttpGet("{studyGroupId}/users/")]
    public async Task<IActionResult> GetGroupUsers(int studyGroupId)
    {
        var studyGroupUsers = await studyGroupRepository.GetStudyGroupUsersAsync(studyGroupId, CancellationToken.None);
        return new OkObjectResult(studyGroupUsers);
    }

    public class GroupUserOperation(int userId, GroupOperation operation)
    {
        public int UserId { get; set; } = userId;
        public GroupOperation Operation { get; set; } = operation;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum GroupOperation { Join, Leave }
    }
}