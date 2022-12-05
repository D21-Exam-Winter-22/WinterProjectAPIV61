using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WinterProjectAPIV61.Models;

public partial class PaymentApidb3Context : DbContext
{
    public PaymentApidb3Context()
    {
    }

    public PaymentApidb3Context(DbContextOptions<PaymentApidb3Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<InPayment> InPayments { get; set; }

    public virtual DbSet<Invite> Invites { get; set; }

    public virtual DbSet<SecurityQuestion> SecurityQuestions { get; set; }

    public virtual DbSet<ShareGroup> ShareGroups { get; set; }

    public virtual DbSet<ShareUser> ShareUsers { get; set; }

    public virtual DbSet<TermsOfService> TermsOfServices { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=51.38.112.206;database=PaymentAPIDB3;user id=sa;password=Alsik22!;trusted_connection=true;TrustServerCertificate=True;integrated security=false;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.ExpenseId).HasName("PK__Expense__1445CFF3149C35F0");

            entity.ToTable("Expense");

            entity.Property(e => e.ExpenseId).HasColumnName("ExpenseID");
            entity.Property(e => e.DatePaid).HasColumnType("datetime");
            entity.Property(e => e.UserGroupId).HasColumnName("UserGroupID");

            entity.HasOne(d => d.UserGroup).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.UserGroupId)
                .HasConstraintName("FK__Expense__UserGro__30F848ED");
        });

        modelBuilder.Entity<InPayment>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__InPaymen__55433A4BC7DEDF14");

            entity.ToTable("InPayment");

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.DatePaid).HasColumnType("datetime");
            entity.Property(e => e.UserGroupId).HasColumnName("UserGroupID");

            entity.HasOne(d => d.UserGroup).WithMany(p => p.InPayments)
                .HasForeignKey(d => d.UserGroupId)
                .HasConstraintName("FK__InPayment__UserG__33D4B598");
        });

        modelBuilder.Entity<Invite>(entity =>
        {
            entity.HasKey(e => e.InviteId).HasName("PK__Invites__AFACE80D62B31235");

            entity.Property(e => e.InviteId).HasColumnName("InviteID");
            entity.Property(e => e.FromUserGroupId).HasColumnName("FromUserGroupID");
            entity.Property(e => e.InviteTime).HasColumnType("datetime");
            entity.Property(e => e.Message).HasColumnName("MESSAGE");
            entity.Property(e => e.ToUserId).HasColumnName("ToUserID");

            entity.HasOne(d => d.FromUserGroup).WithMany(p => p.Invites)
                .HasForeignKey(d => d.FromUserGroupId)
                .HasConstraintName("FK__Invites__FromUse__36B12243");

            entity.HasOne(d => d.ToUser).WithMany(p => p.Invites)
                .HasForeignKey(d => d.ToUserId)
                .HasConstraintName("FK__Invites__ToUserI__37A5467C");
        });

        modelBuilder.Entity<SecurityQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Security__0DC06F8C42FAA0E9");

            entity.ToTable("SecurityQuestion");

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
        });

        modelBuilder.Entity<ShareGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__ShareGro__149AF30A2D31C430");

            entity.ToTable("ShareGroup");

            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.ConclusionDate).HasColumnType("datetime");
            entity.Property(e => e.CreationDate).HasColumnType("datetime");
            entity.Property(e => e.LastActiveDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<ShareUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__ShareUse__1788CCAC535D1012");

            entity.ToTable("ShareUser");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");

            entity.HasOne(d => d.Question).WithMany(p => p.ShareUsers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__ShareUser__Quest__286302EC");
        });

        modelBuilder.Entity<TermsOfService>(entity =>
        {
            entity.HasKey(e => e.ToSid).HasName("PK__TermsOfS__144B507562D816BF");

            entity.ToTable("TermsOfService");

            entity.Property(e => e.ToSid).HasColumnName("ToSID");
            entity.Property(e => e.CreationDate).HasColumnType("datetime");
            entity.Property(e => e.LastModificationDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.UserGroupId).HasName("PK__UserGrou__FA5A61E04D1AB1AE");

            entity.ToTable("UserGroup");

            entity.Property(e => e.UserGroupId).HasColumnName("UserGroupID");
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Group).WithMany(p => p.UserGroups)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__UserGroup__Group__2E1BDC42");

            entity.HasOne(d => d.User).WithMany(p => p.UserGroups)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserGroup__UserI__2D27B809");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
