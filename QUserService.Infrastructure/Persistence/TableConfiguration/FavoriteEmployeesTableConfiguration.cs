using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QUserService.Domain.Models;

namespace QUserService.Infrastructure.Persistence.TableConfiguration;

public class FavoriteEmployeesTableConfiguration: IEntityTypeConfiguration<FavoriteEmployeesEntity>
{
    public void Configure(EntityTypeBuilder<FavoriteEmployeesEntity> builder)
    {
        builder.ToTable("Favorite Employees");
        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.Customer)
            .WithMany(s => s.FavoriteEmployee)
            .HasForeignKey(s => s.CustomerId);

        builder.HasOne(s => s.Employee)
            .WithMany(s => s.FavoriteEmployees)
            .HasForeignKey(s => s.EmployeeId);


    }
}