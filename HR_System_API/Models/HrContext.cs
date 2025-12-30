using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HR_System_API.Models;

public partial class HrContext : DbContext
{
    public HrContext()
    {
    }

    public HrContext(DbContextOptions<HrContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EmployeeInfo> EmployeeInfos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmployeeInfo>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04FF11F08717F");

            entity.ToTable("EmployeeInfo");

            entity.Property(e => e.EmployeeId)
                .HasMaxLength(10)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.Account).HasMaxLength(50);
            entity.Property(e => e.ContactInfo).HasMaxLength(50);
            entity.Property(e => e.EnglishName).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(200);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValue("User");
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
