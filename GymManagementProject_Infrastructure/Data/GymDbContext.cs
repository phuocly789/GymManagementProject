using System;
using System.Collections.Generic;
using GymManagementProject_Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagementProject_Infrastructure.Data;

public partial class GymDbContext : DbContext
{
    public GymDbContext() { }

    public GymDbContext(DbContextOptions<GymDbContext> options)
        : base(options) { }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Checkin> Checkins { get; set; }

    public virtual DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceItem> InvoiceItems { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<MemberProfile> MemberProfiles { get; set; }

    public virtual DbSet<MembershipPlan> MembershipPlans { get; set; }

    public virtual DbSet<MessageTemplate> MessageTemplates { get; set; }

    public virtual DbSet<NotificationLog> NotificationLogs { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<PtContract> PtContracts { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Tenant> Tenants { get; set; }

    public virtual DbSet<Trainer> Trainers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserBranchAccess> UserBranchAccesses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        =>
        optionsBuilder.UseNpgsql(
            "Host=aws-1-ap-southeast-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.fglhmkydcnuacvzzljtu;Password=lmp@123"
        );

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone" })
            .HasPostgresEnum(
                "auth",
                "oauth_authorization_status",
                new[] { "pending", "approved", "denied", "expired" }
            )
            .HasPostgresEnum("auth", "oauth_client_type", new[] { "public", "confidential" })
            .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
            .HasPostgresEnum("auth", "oauth_response_type", new[] { "code" })
            .HasPostgresEnum(
                "auth",
                "one_time_token_type",
                new[]
                {
                    "confirmation_token",
                    "reauthentication_token",
                    "recovery_token",
                    "email_change_token_new",
                    "email_change_token_current",
                    "phone_change_token",
                }
            )
            .HasPostgresEnum("booking_status_type", new[] { "scheduled", "done", "cancelled" })
            .HasPostgresEnum("checkin_method", new[] { "QR", "CARD", "FACE" })
            .HasPostgresEnum("invoice_status_type", new[] { "unpaid", "paid", "void", "refund" })
            .HasPostgresEnum("notification_status", new[] { "sent", "failed" })
            .HasPostgresEnum("payment_method", new[] { "Cash", "Card", "BankTransfer", "QR" })
            .HasPostgresEnum(
                "realtime",
                "action",
                new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" }
            )
            .HasPostgresEnum(
                "realtime",
                "equality_op",
                new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in" }
            )
            .HasPostgresEnum("status_type", new[] { "active", "inactive", "suspended", "expired" })
            .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS", "VECTOR" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("graphql", "pg_graphql")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("uuid-ossp")
            .HasPostgresExtension("vault", "supabase_vault");

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("audit_logs_pkey");

            entity.ToTable("audit_logs", "audit");

            entity.HasIndex(e => e.CreatedAt, "idx_audit_logs_created_at");

            entity.HasIndex(e => e.RecordId, "idx_audit_logs_record_id");

            entity.HasIndex(e => e.TenantId, "idx_audit_logs_tenant_id");

            entity.HasIndex(e => e.UserId, "idx_audit_logs_user_id");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.Action).HasMaxLength(50).HasColumnName("action");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IpAddress).HasMaxLength(45).HasColumnName("ip_address");
            entity.Property(e => e.NewValues).HasColumnType("jsonb").HasColumnName("new_values");
            entity.Property(e => e.OldValues).HasColumnType("jsonb").HasColumnName("old_values");
            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.TableName).HasMaxLength(100).HasColumnName("table_name");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("bookings_pkey");

            entity.ToTable("bookings", "fitness");

            entity.HasIndex(e => e.MemberId, "idx_bookings_member_id");

            entity.HasIndex(e => e.TrainerId, "idx_bookings_trainer_id");

            entity.HasIndex(e => new { e.StartAt, e.EndAt }, "idx_fitness_bookings_time");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.EndAt).HasColumnName("end_at");
            entity.Property(e => e.MemberId).HasColumnName("member_id");
            entity.Property(e => e.StartAt).HasColumnName("start_at");
            entity.Property(e => e.TrainerId).HasColumnName("trainer_id");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");

            entity
                .HasOne(d => d.Branch)
                .WithMany(p => p.Bookings)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("bookings_branch_id_fkey");

            entity
                .HasOne(d => d.Member)
                .WithMany(p => p.Bookings)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("bookings_member_id_fkey");

            entity
                .HasOne(d => d.Trainer)
                .WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TrainerId)
                .HasConstraintName("bookings_trainer_id_fkey");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("branches_pkey");

            entity.ToTable("branches", "core");

            entity.HasIndex(e => e.TenantId, "idx_branches_tenant_id");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.Phone).HasMaxLength(20).HasColumnName("phone");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");

            entity
                .HasOne(d => d.Tenant)
                .WithMany(p => p.Branches)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("branches_tenant_id_fkey");
        });

        modelBuilder.Entity<Checkin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("checkins_pkey");

            entity.ToTable("checkins", "operations");

            entity.HasIndex(e => e.CheckinAt, "idx_operations_checkins_at");

            entity.HasIndex(e => new { e.BranchId, e.CheckinAt }, "idx_operations_checkins_br_at");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity
                .Property(e => e.CheckinAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("checkin_at");
            entity.Property(e => e.DeviceId).HasMaxLength(100).HasColumnName("device_id");
            entity.Property(e => e.MemberId).HasColumnName("member_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");

            entity
                .HasOne(d => d.Branch)
                .WithMany(p => p.Checkins)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("checkins_branch_id_fkey");

            entity
                .HasOne(d => d.Member)
                .WithMany(p => p.Checkins)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("checkins_member_id_fkey");

            entity
                .HasOne(d => d.Tenant)
                .WithMany(p => p.Checkins)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("checkins_tenant_id_fkey");
        });

        modelBuilder.Entity<EmailVerificationToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("email_verification_tokens_pkey");

            entity.ToTable("email_verification_tokens", "iam");

            entity.HasIndex(e => e.ExpiresAt, "idx_email_verification_expires_at");

            entity
                .HasIndex(e => e.TokenHash, "idx_email_verification_token_hash")
                .HasFilter("(used_at IS NULL)");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedByIp).HasMaxLength(45).HasColumnName("created_by_ip");
            entity.Property(e => e.Email).HasMaxLength(255).HasColumnName("email");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.MemberId).HasColumnName("member_id");
            entity.Property(e => e.Purpose).HasMaxLength(50).HasColumnName("purpose");
            entity.Property(e => e.SentCount).HasDefaultValue(1).HasColumnName("sent_count");
            entity.Property(e => e.TokenHash).HasMaxLength(128).HasColumnName("token_hash");
            entity
                .Property(e => e.TokenType)
                .HasMaxLength(20)
                .HasDefaultValueSql("'verification'::character varying")
                .HasColumnName("token_type");
            entity.Property(e => e.UsedAt).HasColumnName("used_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity
                .HasOne(d => d.Member)
                .WithMany(p => p.EmailVerificationTokens)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("email_verification_tokens_member_id_fkey");

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.EmailVerificationTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("email_verification_tokens_user_id_fkey");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("invoices_pkey");

            entity.ToTable("invoices", "sales");

            entity.HasIndex(e => e.BranchId, "idx_invoices_branch_id");

            entity.HasIndex(e => e.MemberId, "idx_invoices_member_id");

            entity.HasIndex(e => e.CreatedAt, "idx_sales_invoices_created_at");

            entity.HasIndex(e => e.InvoiceNumber, "invoices_invoice_number_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50).HasColumnName("invoice_number");
            entity.Property(e => e.MemberId).HasColumnName("member_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.TotalAmount).HasPrecision(15, 2).HasColumnName("total_amount");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");

            entity
                .HasOne(d => d.Branch)
                .WithMany(p => p.Invoices)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoices_branch_id_fkey");

            entity
                .HasOne(d => d.Member)
                .WithMany(p => p.Invoices)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoices_member_id_fkey");

            entity
                .HasOne(d => d.Tenant)
                .WithMany(p => p.Invoices)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoices_tenant_id_fkey");
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("invoice_items_pkey");

            entity.ToTable("invoice_items", "sales");

            entity.HasIndex(e => e.InvoiceId, "idx_invoice_items_invoice_id");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.ItemType).HasMaxLength(50).HasColumnName("item_type");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.Quantity).HasDefaultValue(1).HasColumnName("quantity");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.TotalPrice).HasPrecision(15, 2).HasColumnName("total_price");
            entity.Property(e => e.UnitPrice).HasPrecision(15, 2).HasColumnName("unit_price");

            entity
                .HasOne(d => d.Invoice)
                .WithMany(p => p.InvoiceItems)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("invoice_items_invoice_id_fkey");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("members_pkey");

            entity.ToTable("members", "customers");

            entity.HasIndex(e => e.HomeBranchId, "idx_members_home_branch_id");

            entity.HasIndex(e => e.TenantId, "idx_members_tenant_id");

            entity
                .HasIndex(
                    e => new { e.TenantId, e.MemberCode },
                    "members_tenant_id_member_code_key"
                )
                .IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.HomeBranchId).HasColumnName("home_branch_id");
            entity.Property(e => e.MemberCode).HasMaxLength(50).HasColumnName("member_code");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");

            entity
                .HasOne(d => d.HomeBranch)
                .WithMany(p => p.Members)
                .HasForeignKey(d => d.HomeBranchId)
                .HasConstraintName("members_home_branch_id_fkey");

            entity
                .HasOne(d => d.Tenant)
                .WithMany(p => p.Members)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("members_tenant_id_fkey");
        });

        modelBuilder.Entity<MemberProfile>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("member_profiles_pkey");

            entity.ToTable("member_profiles", "customers");

            entity.HasIndex(e => e.PhoneHash, "idx_member_profiles_phone_hash");

            entity.Property(e => e.MemberId).ValueGeneratedNever().HasColumnName("member_id");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.EmailEnc).HasColumnName("email_enc");
            entity
                .Property(e => e.EmailVerified)
                .HasDefaultValue(false)
                .HasColumnName("email_verified");
            entity.Property(e => e.EmailVerifiedAt).HasColumnName("email_verified_at");
            entity.Property(e => e.FullNameEnc).HasColumnName("full_name_enc");
            entity.Property(e => e.Gender).HasMaxLength(10).HasColumnName("gender");
            entity.Property(e => e.PhoneEnc).HasColumnName("phone_enc");
            entity.Property(e => e.PhoneHash).HasMaxLength(128).HasColumnName("phone_hash");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");

            entity
                .HasOne(d => d.Member)
                .WithOne(p => p.MemberProfile)
                .HasForeignKey<MemberProfile>(d => d.MemberId)
                .HasConstraintName("member_profiles_member_id_fkey");
        });

        modelBuilder.Entity<MembershipPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("membership_plans_pkey");

            entity.ToTable("membership_plans", "products");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.Price).HasPrecision(15, 2).HasColumnName("price");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.TotalSessions).HasColumnName("total_sessions");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");

            entity
                .HasOne(d => d.Tenant)
                .WithMany(p => p.MembershipPlans)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("membership_plans_tenant_id_fkey");
        });

        modelBuilder.Entity<MessageTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("message_templates_pkey");

            entity.ToTable("message_templates", "comms");

            entity
                .HasIndex(e => new { e.TenantId, e.Code }, "message_templates_tenant_id_code_key")
                .IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.BodyTemplate).HasColumnName("body_template");
            entity.Property(e => e.Code).HasMaxLength(100).HasColumnName("code");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Subject).HasColumnName("subject");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");

            entity
                .HasOne(d => d.Tenant)
                .WithMany(p => p.MessageTemplates)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("message_templates_tenant_id_fkey");
        });

        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notification_logs_pkey");

            entity.ToTable("notification_logs", "comms");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.Channel).HasMaxLength(20).HasColumnName("channel");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.RecipientId).HasColumnName("recipient_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity
                .HasOne(d => d.Tenant)
                .WithMany(p => p.NotificationLogs)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("notification_logs_tenant_id_fkey");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payments_pkey");

            entity.ToTable("payments", "sales");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.Amount).HasPrecision(15, 2).HasColumnName("amount");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.TransactionId).HasMaxLength(100).HasColumnName("transaction_id");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");

            entity
                .HasOne(d => d.Invoice)
                .WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("payments_invoice_id_fkey");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("permissions_pkey");

            entity.ToTable("permissions", "iam");

            entity
                .HasIndex(e => new { e.TenantId, e.Code }, "permissions_tenant_id_code_key")
                .IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.Code).HasMaxLength(100).HasColumnName("code");
            entity.Property(e => e.Module).HasMaxLength(50).HasColumnName("module");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity
                .HasOne(d => d.Tenant)
                .WithMany(p => p.Permissions)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("permissions_tenant_id_fkey");
        });

        modelBuilder.Entity<PtContract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pt_contracts_pkey");

            entity.ToTable("pt_contracts", "fitness");

            entity.HasIndex(e => e.MemberId, "idx_pt_contracts_member_id");

            entity.HasIndex(e => e.TrainerId, "idx_pt_contracts_trainer_id");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.MemberId).HasColumnName("member_id");
            entity.Property(e => e.TotalSessions).HasColumnName("total_sessions");
            entity.Property(e => e.TrainerId).HasColumnName("trainer_id");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UsedSessions).HasDefaultValue(0).HasColumnName("used_sessions");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");

            entity
                .HasOne(d => d.Member)
                .WithMany(p => p.PtContracts)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("pt_contracts_member_id_fkey");

            entity
                .HasOne(d => d.Trainer)
                .WithMany(p => p.PtContracts)
                .HasForeignKey(d => d.TrainerId)
                .HasConstraintName("pt_contracts_trainer_id_fkey");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("refresh_tokens_pkey");

            entity.ToTable("refresh_tokens", "iam");

            entity.HasIndex(e => e.ExpiresAt, "idx_refresh_tokens_expires_at");

            entity.HasIndex(e => e.Token, "idx_refresh_tokens_token");

            entity
                .HasIndex(e => e.UserId, "idx_refresh_tokens_user_id")
                .HasFilter("(revoked_at IS NULL)");

            entity.HasIndex(e => e.Token, "refresh_tokens_token_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedByIp).HasMaxLength(45).HasColumnName("created_by_ip");
            entity.Property(e => e.DeviceInfo).HasColumnName("device_info");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.LastUsedAt).HasColumnName("last_used_at");
            entity.Property(e => e.LastUsedIp).HasMaxLength(45).HasColumnName("last_used_ip");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.Token).HasMaxLength(512).HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("refresh_tokens_user_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles", "iam");

            entity.HasIndex(e => new { e.TenantId, e.Code }, "roles_tenant_id_code_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.Code).HasMaxLength(50).HasColumnName("code");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");

            entity
                .HasOne(d => d.Tenant)
                .WithMany(p => p.Roles)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("roles_tenant_id_fkey");

            entity
                .HasMany(d => d.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r =>
                        r.HasOne<Permission>()
                            .WithMany()
                            .HasForeignKey("PermissionId")
                            .HasConstraintName("role_permissions_permission_id_fkey"),
                    l =>
                        l.HasOne<Role>()
                            .WithMany()
                            .HasForeignKey("RoleId")
                            .HasConstraintName("role_permissions_role_id_fkey"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId").HasName("role_permissions_pkey");
                        j.ToTable("role_permissions", "iam");
                        j.IndexerProperty<Guid>("RoleId").HasColumnName("role_id");
                        j.IndexerProperty<Guid>("PermissionId").HasColumnName("permission_id");
                    }
                );
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tenants_pkey");

            entity.ToTable("tenants", "core");

            entity.HasIndex(e => e.Code, "tenants_code_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.Code).HasMaxLength(50).HasColumnName("code");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");
        });

        modelBuilder.Entity<Trainer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("trainers_pkey");

            entity.ToTable("trainers", "fitness");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Specialties).HasColumnName("specialties");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.Trainers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("trainers_user_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users", "iam");

            entity
                .HasIndex(e => e.FullName, "idx_users_full_name_gin")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" });

            entity.HasIndex(e => e.TenantId, "idx_users_tenant_id");

            entity
                .HasIndex(e => new { e.TenantId, e.Email }, "users_tenant_id_email_key")
                .IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()").HasColumnName("id");
            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Email).HasMaxLength(255).HasColumnName("email");
            entity
                .Property(e => e.EmailVerified)
                .HasDefaultValue(false)
                .HasColumnName("email_verified");
            entity.Property(e => e.EmailVerifiedAt).HasColumnName("email_verified_at");
            entity.Property(e => e.FullName).HasMaxLength(255).HasColumnName("full_name");
            entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity
                .Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");
            entity.Property(e => e.Version).HasDefaultValue(1).HasColumnName("version");

            entity
                .HasOne(d => d.Tenant)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_tenant_id_fkey");

            entity
                .HasMany(d => d.Roles)
                .WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r =>
                        r.HasOne<Role>()
                            .WithMany()
                            .HasForeignKey("RoleId")
                            .HasConstraintName("user_roles_role_id_fkey"),
                    l =>
                        l.HasOne<User>()
                            .WithMany()
                            .HasForeignKey("UserId")
                            .HasConstraintName("user_roles_user_id_fkey"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("user_roles_pkey");
                        j.ToTable("user_roles", "iam");
                        j.HasIndex(new[] { "UserId" }, "idx_user_roles_user_id");
                        j.IndexerProperty<Guid>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<Guid>("RoleId").HasColumnName("role_id");
                    }
                );
        });

        modelBuilder.Entity<UserBranchAccess>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.BranchId }).HasName("user_branch_access_pkey");

            entity.ToTable("user_branch_access", "iam");

            entity.HasIndex(e => e.UserId, "idx_user_branch_access_user_id");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.IsPrimary).HasDefaultValue(false).HasColumnName("is_primary");

            entity
                .HasOne(d => d.Branch)
                .WithMany(p => p.UserBranchAccesses)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("user_branch_access_branch_id_fkey");

            entity
                .HasOne(d => d.User)
                .WithMany(p => p.UserBranchAccesses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_branch_access_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
