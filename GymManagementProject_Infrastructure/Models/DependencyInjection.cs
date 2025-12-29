using Microsoft.Extensions.DependencyInjection;

public static class RepositoryServiceRegistration
{
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPtContractRepository, PtContractRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<ICheckinRepository, CheckinRepository>();
        services.AddScoped<IInvoiceItemRepository, InvoiceItemRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserBranchAccessRepository, UserBranchAccessRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IMemberProfileRepository, MemberProfileRepository>();
        services.AddScoped<IMessageTemplateRepository, MessageTemplateRepository>();
        services.AddScoped<IMembershipPlanRepository, MembershipPlanRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITrainerRepository, TrainerRepository>();
        services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();

        return services;
    }
}
