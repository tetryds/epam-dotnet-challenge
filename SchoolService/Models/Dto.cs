using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using static SchoolService.Databases.SchoolDb;

namespace SchoolService.Models.Dto;

public class CreateUserDto
{
    [Required]
    public required string Name { get; set; }
}

public class CreateStudyGroupDto
{
    [Required]
    [StringLength(maximumLength: 30, MinimumLength = 5)]
    public required string Name { get; set; }
    [Required]
    [EnumDataType(typeof(Subject))]
    public required Subject Subject { get; set; }
    [Required]
    public required int OwnerId { get; set; }
}
