using System;
using System.Collections.Generic;
using DeaneryManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeaneryManagementSystem.Data;

public partial class DeaneryContext : DbContext
{
    public DeaneryContext()
    {
    }

    public DeaneryContext(DbContextOptions<DeaneryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Key> Keys { get; set; }

    public virtual DbSet<Mark> Marks { get; set; }

    public virtual DbSet<Person> People { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        base.OnConfiguring(optionsBuilder);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("groups_pkey");

            entity.ToTable("groups");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Year).HasColumnName("year");
        });

        modelBuilder.Entity<Key>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("keys_pkey");

            entity.ToTable("keys");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AccessKey)
                .HasMaxLength(256)
                .HasColumnName("accessKey");
        });

        modelBuilder.Entity<Mark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marks_pkey");

            entity.ToTable("marks");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.TeacherId).HasColumnName("teacher_id");
            entity.Property(e => e.Term).HasColumnName("term");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Student).WithMany(p => p.MarkStudents)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("fk_marks_people1");

            entity.HasOne(d => d.Subject).WithMany(p => p.Marks)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("fk_marks_subjects");

            entity.HasOne(d => d.Teacher).WithMany(p => p.MarkTeachers)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("fk_marks_people2");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("people_pkey");

            entity.ToTable("people");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(20)
                .HasColumnName("first_name");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.PatherName)
                .HasMaxLength(20)
                .HasColumnName("pather_name");
            entity.Property(e => e.SecondName)
                .HasMaxLength(20)
                .HasColumnName("second_name");
            entity.Property(e => e.Type)
                .HasColumnType("char")
                .HasColumnName("type");

            entity.HasOne(d => d.Group).WithMany(p => p.People)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("fk_people_groups");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subjects_pkey");

            entity.ToTable("subjects");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.HashedPassword)
                .HasMaxLength(256)
                .HasColumnName("hashedPassword");
            entity.Property(e => e.KeyId).HasColumnName("key_Id");
            entity.Property(e => e.PersonId).HasColumnName("person_Id");

            entity.HasOne(d => d.Key).WithMany(p => p.Users)
                .HasForeignKey(d => d.KeyId)
                .HasConstraintName("fk_users_keys");

            entity.HasOne(d => d.Person).WithMany(p => p.Users)
                .HasForeignKey(d => d.PersonId)
                .HasConstraintName("fk_users_people");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
