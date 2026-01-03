using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PerfectFit.Core.Entities;

namespace PerfectFit.Infrastructure.Data.Configurations;

public class AdminAuditLogConfiguration : IEntityTypeConfiguration<AdminAuditLog>
{
    public void Configure(EntityTypeBuilder<AdminAuditLog> builder)
    {
        builder.ToTable("admin_audit_logs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(l => l.AdminUserId)
            .HasColumnName("admin_user_id")
            .IsRequired();

        builder.Property(l => l.Action)
            .HasColumnName("action")
            .IsRequired();

        builder.Property(l => l.TargetUserId)
            .HasColumnName("target_user_id");

        builder.Property(l => l.Details)
            .HasColumnName("details")
            .HasMaxLength(2000);

        builder.Property(l => l.Timestamp)
            .HasColumnName("timestamp")
            .IsRequired();

        // Index on Timestamp for query performance (descending for recent-first queries)
        builder.HasIndex(l => l.Timestamp)
            .IsDescending()
            .HasDatabaseName("ix_admin_audit_logs_timestamp");

        // Index on AdminUserId for filtering by admin
        builder.HasIndex(l => l.AdminUserId)
            .HasDatabaseName("ix_admin_audit_logs_admin_user_id");

        // Index on Action for filtering by action type
        builder.HasIndex(l => l.Action)
            .HasDatabaseName("ix_admin_audit_logs_action");
    }
}
