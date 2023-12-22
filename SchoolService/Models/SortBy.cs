using System.Text.Json.Serialization;

namespace SchoolService.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortBy
{
    None,
    Oldest,
    Newest
}
