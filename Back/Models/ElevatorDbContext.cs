using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ElevatorSystem.Models;

public partial class ElevatorDbContext : DbContext
{
    public ElevatorDbContext()
    {
    }

    public ElevatorDbContext(DbContextOptions<ElevatorDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Building> Buildings { get; set; }

    public virtual DbSet<Elevator> Elevators { get; set; }

    public virtual DbSet<ElevatorCall> ElevatorCalls { get; set; }

    public virtual DbSet<ElevatorCallAssignment> ElevatorCallAssignments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=localhost;Database=elevatorDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Building>(entity =>
        {
            entity.ToTable("Building");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Buildings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Building_User");
        });

        modelBuilder.Entity<Elevator>(entity =>
        {
            entity.ToTable("Elevator");

            entity.HasIndex(e => e.BuildingId, "FK_Building").IsUnique();

            entity.Property(e => e.BuildingId)
                .HasColumnName("BuildingID");

            entity.Property(e => e.Status)
                .HasConversion<string>();

            entity.Property(e => e.Direction)
                .HasConversion<string>();

            entity.Property(e => e.DoorStatus)
                .HasConversion<string>();

            entity.HasOne(d => d.Building)
                .WithOne(p => p.Elevator)
                .HasForeignKey<Elevator>(d => d.BuildingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Elevator_Building");
        });

        modelBuilder.Entity<ElevatorCall>(entity =>
        {
            entity.ToTable("ElevatorCall");

            entity.Property(e => e.CallTime).HasColumnType("datetime");

            entity.HasOne(d => d.Building).WithMany(p => p.ElevatorCalls)
                .HasForeignKey(d => d.BuildingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ElevatorCall_Building");
        });

        // 🔧 FIXED: Correct ElevatorCallAssignment configuration
        modelBuilder.Entity<ElevatorCallAssignment>(entity =>
        {
            entity.ToTable("ElevatorCallAssignment");

            // CallID is the primary key
            entity.HasKey(e => e.CallID);

            entity.Property(e => e.AssignmentTime).HasColumnType("datetime");

            // Relationship to Elevator
            entity.HasOne(d => d.Elevator)
                .WithMany(p => p.Assignments)
                .HasForeignKey(d => d.ElevatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ElevatorCallAssignment_Elevator");

            // 🔑 CRITICAL FIX: Use ElevatorCallId as the foreign key, not CallID
            entity.HasOne(d => d.ElevatorCall)
                .WithMany(p => p.Assignments)
                .HasForeignKey(d => d.ElevatorCallId) // Use ElevatorCallId column
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ElevatorCallAssignment_ElevatorCall_ElevatorCallId");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}