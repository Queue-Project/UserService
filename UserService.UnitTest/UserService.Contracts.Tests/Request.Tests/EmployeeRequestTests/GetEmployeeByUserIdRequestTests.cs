using MessagePack;
using QUserService.Contracts.Requests.EmployeeRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.EmployeeRequestTests;

public class GetEmployeeByUserIdRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new GetEmployeeByUserIdRequest
        {
            RequestId = Guid.NewGuid(),
            UserId = 1,
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<GetEmployeeByUserIdRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.UserId.ShouldBe(originalRequest.UserId);
    }
}