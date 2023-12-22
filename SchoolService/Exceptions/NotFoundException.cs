namespace SchoolService.Exceptions;

[Serializable]
internal class NotFoundException(string entityName, int expected) : Exception
{
    public string entityName = entityName;
    public int expected = expected;
}