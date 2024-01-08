using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace SchoolService.Databases;

public class SchoolDb(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<StudyGroup> StudyGroups { get; set; }

    public class User
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public DateTime CreateDate { get; set; }

        [CascadingParameter]
        public List<StudyGroup>? StudyGroups { get; } = [];
        [InverseProperty(nameof(StudyGroup.Owner))]
        public List<StudyGroup>? OwnedGroups { get; } = [];
    }

    public class StudyGroup
    {
        public int Id { get; set; }
        [Required]
        [StringLength(maximumLength: 30, MinimumLength = 5)]
        public string? Name { get; set; }
        [Required]
        [EnumDataType(typeof(Subject))]
        public Subject Subject { get; set; }
        [Required]
        public int OwnerId {  get; set; }
        [ForeignKey(nameof(OwnerId))]
        [JsonIgnore]
        public User? Owner { get; set; }
        public List<User>? Users { get; } = [];
        public DateTime CreateDate { get; set; }

        public override string ToString()
        {
            return $"Id:'{Id}', Name:'{Name}', Subject:'{Subject}', CreateDate:'{CreateDate}', Users:'{string.Join(",", Users?.Select(u => u.Name) ?? [])}'";
        }
    }

    //This allows for HttpClient to deserialize this object from json
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Subject
    {
        Math,
        Chemistry,
        Physics
    }
}