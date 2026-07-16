namespace QUserService.Application.Helpers;

public static class DateTimeOffsetExtensions
{
    public static DateOnly ToDateOnly(this DateTimeOffset dto)
    {
        return DateOnly.FromDateTime(dto.DateTime);
    }
}