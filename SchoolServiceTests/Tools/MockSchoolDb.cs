using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolService.Databases;
using SchoolService.Models;
using SchoolService.Repositories;
using SchoolService.Repositories.StudyGroups;
using static SchoolService.Databases.SchoolDb;

namespace SchoolServiceTests.Tools;

public class MockSchoolDb
{
    public Dictionary<int, User> Users { get; set; } = [];
    public Dictionary<int, StudyGroup> StudyGroups { get; set; } = [];
}