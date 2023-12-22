using Microsoft.AspNetCore.Mvc;
using SchoolService.Models;
using SchoolService.Models.Dto;
using SchoolService.Repositories.Users;
using static SchoolService.Databases.SchoolDb;

namespace SchoolService.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(IUserRepository userRepository)
{
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
    {
        var user = new User { Name = createUserDto.Name };
        Id id = await userRepository.CreateUser(user, CancellationToken.None);
        return new OkObjectResult(id);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        return new OkObjectResult(await userRepository.GetUsers(CancellationToken.None));
    }

    [HttpGet("{userId}/studygroups/")]
    public async Task<IActionResult> GetStudyGroups(int userId)
    {
        return new OkObjectResult(await userRepository.GetStudyGroups(userId, CancellationToken.None));
    }
}