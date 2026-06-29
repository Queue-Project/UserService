using MessagePack;
using QUserService.Contracts.Requests.UserRequests;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Request.Tests.UserRequestsTests;

public class GetUserByEmployeeIdRequestTests
{
    [Fact]
    public void UserRequest_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new GetUserByEmployeeIdRequest
        {
            RequestId = Guid.NewGuid(),
            EmployeeId = 1,
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<GetUserByEmployeeIdRequest>(bytes);


        deserializedRequest.RequestId.ShouldBe(originalRequest.RequestId);
        deserializedRequest.EmployeeId.ShouldBe(originalRequest.EmployeeId);
    }
}