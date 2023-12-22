namespace SchoolService.Models;

public class Id
{
    public required int Value { get; set; }

    public static implicit operator int(Id id) { return id.Value; }
    public static implicit operator Id(int id) { return new Id { Value = id }; }
}
