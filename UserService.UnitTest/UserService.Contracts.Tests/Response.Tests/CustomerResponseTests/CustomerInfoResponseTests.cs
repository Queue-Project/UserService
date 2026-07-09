using MessagePack;
using QUserService.Contracts.Responses.CustomerResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.CustomerResponseTests;

public class CustomerInfoResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new CustomerInfo
        {
            CustomerId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            CreatedAt = DateTime.UtcNow
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<CustomerInfo>(bytes);


        deserializedRequest.CustomerId.ShouldBe(originalRequest.CustomerId);
        deserializedRequest.FirstName.ShouldBe(originalRequest.FirstName);
        deserializedRequest.LastName.ShouldBe(originalRequest.LastName);
        deserializedRequest.PhoneNumber.ShouldBe(originalRequest.PhoneNumber);
        deserializedRequest.CreatedAt.ShouldBe(originalRequest.CreatedAt);
    }
}