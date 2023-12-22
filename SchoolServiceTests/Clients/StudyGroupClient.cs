using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using SchoolService.Models;
using SchoolService.Models.Dto;
using SchoolServiceTests.Exceptions;
using static SchoolService.Controllers.StudyGroupController;
using static SchoolService.Controllers.StudyGroupController.GroupUserOperation;
using static SchoolService.Databases.SchoolDb;

namespace SchoolServiceTests.Clients;

public class StudyGroupClient(HttpClient client)
{
    private const string BaseUri = "/studygroup";
    private const string SearchUri = $"{BaseUri}/search";
    private const string UserUri = $"{BaseUri}/{{0}}/users";

    public async Task<Id> CreateStudyGroup(CreateStudyGroupDto studyGroup)
    {
        var response = await client.PostAsJsonAsync(BaseUri, studyGroup);
        if (!response.IsSuccessStatusCode)
            throw new StudyGroupApiException(response);

        return await response.Content.ReadFromJsonAsync<Id>();
    }

    public async Task<IEnumerable<StudyGroup>?> GetStudyGroups()
    {
        return await client.GetFromJsonAsync<IEnumerable<StudyGroup>>(BaseUri);
    }

    public async Task<IEnumerable<User>?> GetStudyGroupUsers(int studyGroupId)
    {
        string userUri = string.Format(UserUri, studyGroupId);
        return await client.GetFromJsonAsync<IEnumerable<User>>(userUri);
    }

    public async Task<IEnumerable<StudyGroup>?> SearchStudyGroups(Subject subject)
    {
        string searchUri = $"{SearchUri}?subject={subject}";
        return await client.GetFromJsonAsync<IEnumerable<StudyGroup>>(searchUri);
    }

    public async Task JoinStudyGroup(int studyGroupId, int userId)
    {
        var response = await ModifyGroupUser(studyGroupId, userId, GroupOperation.Join);
        if (!response.IsSuccessStatusCode)
            throw new StudyGroupApiException(response);
    }

    public async Task LeaveStudyGroup(int studyGroupId, int userId)
    {
        var response = await ModifyGroupUser(studyGroupId, userId, GroupOperation.Leave);
        if (!response.IsSuccessStatusCode)
            throw new StudyGroupApiException(response);
    }

    private async Task<HttpResponseMessage> ModifyGroupUser(int studyGroupId, int userId, GroupOperation operation)
    {
        string userUri = string.Format(UserUri, studyGroupId);
        var groupUserOperation = new GroupUserOperation(userId, operation);
        return await client.PostAsJsonAsync(userUri, groupUserOperation);
    }
}