using System.Runtime.Serialization;

namespace SchoolServiceTests.Exceptions;

[Serializable]
internal class StudyGroupApiException(HttpResponseMessage response) : Exception
{
    public HttpResponseMessage response = response;
}