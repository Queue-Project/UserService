using MessagePack;
using QUserService.Contracts.Responses.CustomerResponses;
using Shouldly;

namespace UserService.UnitTest.UserService.Contracts.Tests.Response.Tests.CustomerResponseTests;

public class CustomerResponseTests
{
    [Fact]
    public void UserResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalRequest = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            DateOfBirth = new DateTime(2006, 06, 06),
            Address = "Test Address",
            Country = "Test Country",
            City = "Test City",
            ErrorMessage = null,
            IsValid = true,
            CreatedAt = DateTime.UtcNow
        };

        var bytes = MessagePackSerializer.Serialize(originalRequest);
        var deserializedRequest = MessagePackSerializer.Deserialize<CustomerResponse>(bytes);


        deserializedRequest.Id.ShouldBe(originalRequest.Id);
        deserializedRequest.FirstName.ShouldBe(originalRequest.FirstName);
        deserializedRequest.LastName.ShouldBe(originalRequest.LastName);
        deserializedRequest.PhoneNumber.ShouldBe(originalRequest.PhoneNumber);
        deserializedRequest.DateOfBirth.ShouldBe(originalRequest.DateOfBirth);
        deserializedRequest.Address.ShouldBe(originalRequest.Address);
        deserializedRequest.Country.ShouldBe(originalRequest.Country);
        deserializedRequest.City.ShouldBe(originalRequest.City);
        deserializedRequest.ErrorMessage.ShouldBe(originalRequest.ErrorMessage);
        deserializedRequest.IsValid.ShouldBe(originalRequest.IsValid);
        deserializedRequest.CreatedAt.ShouldBe(originalRequest.CreatedAt);
    }
}