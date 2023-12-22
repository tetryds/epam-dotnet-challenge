using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolService.Databases;
using SchoolService.Repositories;
using static SchoolService.Databases.SchoolDb;

namespace SchoolServiceTests.Tools;

public static class DbTools
{
    public static async Task EnsureDbCleared(WebApplicationFactory<Program> factory)
    {
        var scopeFactory = factory.Services.GetService<IServiceScopeFactory>();

        using (var scope = scopeFactory.CreateScope())
        {
            SchoolDb schoolDb = scope.ServiceProvider.GetService<SchoolDb>();
            schoolDb.StudyGroups.RemoveRange(schoolDb.StudyGroups.ToList());
            schoolDb.Users.RemoveRange(schoolDb.Users.ToList());
            await schoolDb.SaveChangesAsync();
        }
    }
}