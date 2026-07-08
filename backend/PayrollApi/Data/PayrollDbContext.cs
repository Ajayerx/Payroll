using Microsoft.EntityFrameworkCore;
using PayrollApi.Models.Entities;

namespace PayrollApi.Data;

public class PayrollDbContext : DbContext
{
    public PayrollDbContext(DbContextOptions<PayrollDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<SalaryComponent> SalaryComponents => Set<SalaryComponent>();
    public DbSet<EmployeeSalaryStructure> EmployeeSalaryStructures => Set<EmployeeSalaryStructure>();
    public DbSet<PayrollMonth> PayrollMonths => Set<PayrollMonth>();
    public DbSet<Payroll> Payrolls => Set<Payroll>();
    public DbSet<PayrollDetail> PayrollDetails => Set<PayrollDetail>();
    public DbSet<TaxConfiguration> TaxConfigurations => Set<TaxConfiguration>();
    public DbSet<Deduction> Deductions => Set<Deduction>();
    public DbSet<CompanySetting> CompanySettings => Set<CompanySetting>();
    public DbSet<TaxSlab> TaxSlabs => Set<TaxSlab>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var pk = entityType.FindPrimaryKey();
            if (pk?.Properties.Count == 1 && pk.Properties[0].ClrType == typeof(Guid))
            {
                modelBuilder.Entity(entityType.Name).Property(pk.Properties[0].Name).HasDefaultValueSql("NEWSEQUENTIALID()");
            }
        }

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.Password).IsRequired();
            e.Property(u => u.FirstName).HasMaxLength(100);
            e.Property(u => u.LastName).HasMaxLength(100);
            e.Property(u => u.Role).HasConversion<string>().HasMaxLength(50);
            e.Property(u => u.ResetToken).HasMaxLength(500);
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.HasIndex(emp => emp.EmployeeCode).IsUnique();
            e.HasIndex(emp => emp.Email);
            e.HasIndex(emp => emp.Department);
            e.Property(emp => emp.EmployeeCode).HasMaxLength(20).IsRequired();
            e.Property(emp => emp.FirstName).HasMaxLength(100).IsRequired();
            e.Property(emp => emp.LastName).HasMaxLength(100).IsRequired();
            e.Property(emp => emp.Gender).HasMaxLength(10);
            e.Property(emp => emp.Phone).HasMaxLength(20);
            e.Property(emp => emp.Email).HasMaxLength(256);
            e.Property(emp => emp.Department).HasMaxLength(100);
            e.Property(emp => emp.Designation).HasMaxLength(100);
            e.Property(emp => emp.BankName).HasMaxLength(100);
            e.Property(emp => emp.BankAccount).HasMaxLength(50);
            e.Property(emp => emp.IfscCode).HasMaxLength(20);

            e.HasOne(emp => emp.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(emp => emp.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SalaryComponent>(e =>
        {
            e.HasIndex(sc => sc.Name).IsUnique();
            e.Property(sc => sc.Name).HasMaxLength(100).IsRequired();
            e.Property(sc => sc.Type).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<EmployeeSalaryStructure>(e =>
        {
            e.HasIndex(ess => new { ess.EmployeeId, ess.SalaryComponentId }).IsUnique();

            e.HasOne(ess => ess.Employee)
                .WithMany(emp => emp.SalaryStructures)
                .HasForeignKey(ess => ess.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ess => ess.SalaryComponent)
                .WithMany(sc => sc.EmployeeStructures)
                .HasForeignKey(ess => ess.SalaryComponentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PayrollMonth>(e =>
        {
            e.HasIndex(pm => new { pm.Month, pm.Year }).IsUnique();
            e.Property(pm => pm.Status).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Payroll>(e =>
        {
            e.HasIndex(p => new { p.EmployeeId, p.PayrollMonthId }).IsUnique();
            e.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

            e.HasOne(p => p.Employee)
                .WithMany(emp => emp.Payrolls)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.PayrollMonth)
                .WithMany(pm => pm.Payrolls)
                .HasForeignKey(p => p.PayrollMonthId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PayrollDetail>(e =>
        {
            e.HasOne(pd => pd.Payroll)
                .WithMany(p => p.PayrollDetails)
                .HasForeignKey(pd => pd.PayrollId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(pd => pd.SalaryComponent)
                .WithMany()
                .HasForeignKey(pd => pd.SalaryComponentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaxConfiguration>(e =>
        {
            e.HasOne(tc => tc.Employee)
                .WithMany(emp => emp.TaxConfigurations)
                .HasForeignKey(tc => tc.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Deduction>(e =>
        {
            e.Property(d => d.Type).HasConversion<string>().HasMaxLength(20);

            e.HasOne(d => d.Employee)
                .WithMany(emp => emp.Deductions)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaxSlab>(e =>
        {
            e.Property(ts => ts.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<LeaveType>(e =>
        {
            e.HasIndex(lt => lt.Name).IsUnique();
            e.Property(lt => lt.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<CompanySetting>(e =>
        {
            e.Property(cs => cs.CompanyName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasIndex(rt => rt.Token);
            e.HasIndex(rt => rt.UserId);
            e.Property(rt => rt.Token).HasMaxLength(500).IsRequired();

            e.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasIndex(al => new { al.EntityName, al.EntityId });
            e.Property(al => al.EntityName).HasMaxLength(100).IsRequired();
            e.Property(al => al.Action).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<LeaveRequest>(e =>
        {
            e.HasIndex(lr => lr.EmployeeId);
            e.Property(lr => lr.Status).HasMaxLength(20).IsRequired();
            e.Property(lr => lr.TotalDays).HasPrecision(4, 1);

            e.HasOne(lr => lr.Employee)
                .WithMany()
                .HasForeignKey(lr => lr.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(lr => lr.LeaveType)
                .WithMany()
                .HasForeignKey(lr => lr.LeaveTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasIndex(n => n.UserId);
            e.HasIndex(n => new { n.UserId, n.IsRead });
            e.Property(n => n.Title).HasMaxLength(200).IsRequired();
            e.Property(n => n.Message).HasMaxLength(1000).IsRequired();

            e.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
