namespace QUserService.Domain.Models;

public class FavoriteEmployeesEntity : BaseEntity
{
    public int EmployeeId { get; set; }
    public int CustomerId { get; set; }

    public EmployeeEntity Employee { get; set; }
    public CustomerEntity Customer { get; set; }

    public DateTime CreatedAt { get; set; }
}