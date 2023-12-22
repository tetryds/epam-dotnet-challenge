using System.Net.Http.Json;
using SchoolService.Models;
using SchoolServiceTests.Exceptions;
using static SchoolService.Databases.SchoolDb;

namespace SchoolServiceTests.Clients;

public class UserClient(HttpClient client)
{
    private const string BaseUri = "/user";
    private const string StudyGroupsUri = $"{BaseUri}/{{0}}/studygroups";

    public async Task<Id> CreateUser(User user)
    {
        var response = await client.PostAsJsonAsync(BaseUri, user);
        if (!response.IsSuccessStatusCode)
            throw new StudyGroupApiException(response);

        return await response.Content.ReadFromJsonAsync<Id>();
    }

    public async Task<IEnumerable<User>?> GetUsers()
    {
        return await client.GetFromJsonAsync<IEnumerable<User>>(BaseUri);
    }

    public async Task<IEnumerable<StudyGroup>?> GetStudyGroups(int userId)
    {
        var studyGroupsUri = string.Format(StudyGroupsUri, userId);
        return await client.GetFromJsonAsync<IEnumerable<StudyGroup>>(studyGroupsUri);
    }
}