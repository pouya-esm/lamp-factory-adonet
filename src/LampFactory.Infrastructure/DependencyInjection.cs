using LampFactory.Application.Abstractions.Security;
using LampFactory.Application.Features.Audit;
using LampFactory.Application.Features.IdentityAccess.Authentication;
using LampFactory.Application.Features.IdentityAccess.Permissions;
using LampFactory.Application.Features.IdentityAccess.Resources;
using LampFactory.Application.Features.IdentityAccess.Roles;
using LampFactory.Application.Features.IdentityAccess.SysActions;
using LampFactory.Application.Features.IdentityAccess.Users;
using LampFactory.Application.Features.Inventory.Products;
using LampFactory.Application.Features.Inventory.Warehouses;
using LampFactory.Application.Features.Production.BillOfMaterials;
using LampFactory.Application.Features.Production.LineStops;
using LampFactory.Application.Features.Production.Operators;
using LampFactory.Application.Features.Production.ProductionDocuments;
using LampFactory.Application.Features.Production.ProductionLines;
using LampFactory.Application.Features.Production.ProductionPlans;
using LampFactory.Application.Features.Purchasing.PurchaseVouchers;
using LampFactory.Application.Features.Purchasing.RequestVouchers;
using LampFactory.Application.Features.QualityControl;
using LampFactory.Application.Features.Reports;
using LampFactory.Application.Features.Sales;
using LampFactory.Application.Features.Vouchers.ChargeVouchers;
using FluentValidation;
using LampFactory.Application.Abstractions.Persistence.Audit;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Authentication;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Permissions;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Resources;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Roles;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.SysActions;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.Users;
using LampFactory.Application.Abstractions.Persistence.Inventory.Products;
using LampFactory.Application.Abstractions.Persistence.Inventory.Warehouses;
using LampFactory.Application.Abstractions.Persistence.Production.BillOfMaterials;
using LampFactory.Application.Abstractions.Persistence.Production.LineStops;
using LampFactory.Application.Abstractions.Persistence.Production.Operators;
using LampFactory.Application.Abstractions.Persistence.Production.ProductionDocuments;
using LampFactory.Application.Abstractions.Persistence.Production.ProductionLines;
using LampFactory.Application.Abstractions.Persistence.Production.ProductionPlans;
using LampFactory.Application.Abstractions.Persistence.Purchasing.PurchaseVouchers;
using LampFactory.Application.Abstractions.Persistence.Purchasing.RequestVouchers;
using LampFactory.Application.Abstractions.Persistence.QualityControl;
using LampFactory.Application.Abstractions.Persistence.Reports;
using LampFactory.Application.Abstractions.Persistence.Sales;
using LampFactory.Application.Abstractions.Persistence.Vouchers.ChargeVouchers;
using LampFactory.Infrastructure.Persistence;
using LampFactory.Infrastructure.Persistence.Repositories.Audit;
using LampFactory.Infrastructure.Persistence.Repositories.IdentityAccess;
using LampFactory.Infrastructure.Persistence.Repositories.Inventory;
using LampFactory.Infrastructure.Persistence.Repositories.Production;
using LampFactory.Infrastructure.Persistence.Repositories.Purchasing;
using LampFactory.Infrastructure.Persistence.Repositories.QualityControl;
using LampFactory.Infrastructure.Persistence.Repositories.Sales;
using LampFactory.Infrastructure.Persistence.Repositories.Vouchers;
using LampFactory.Infrastructure.Reports;
using LampFactory.Infrastructure.Security;
using LampFactory.Infrastructure.Services.Audit;
using LampFactory.Infrastructure.Services.IdentityAccess.Authentication;
using LampFactory.Infrastructure.Services.IdentityAccess.Permissions;
using LampFactory.Infrastructure.Services.IdentityAccess.Resources;
using LampFactory.Infrastructure.Services.IdentityAccess.Roles;
using LampFactory.Infrastructure.Services.IdentityAccess.SysActions;
using LampFactory.Infrastructure.Services.IdentityAccess.Users;
using LampFactory.Infrastructure.Services.Inventory.Products;
using LampFactory.Infrastructure.Services.Inventory.Warehouses;
using LampFactory.Infrastructure.Services.Production.BillOfMaterials;
using LampFactory.Infrastructure.Services.Production.LineStops;
using LampFactory.Infrastructure.Services.Production.Operators;
using LampFactory.Infrastructure.Services.Production.ProductionDocuments;
using LampFactory.Infrastructure.Services.Production.ProductionLines;
using LampFactory.Infrastructure.Services.Production.ProductionPlans;
using LampFactory.Infrastructure.Services.Purchasing.PurchaseVouchers;
using LampFactory.Infrastructure.Services.Purchasing.RequestVouchers;
using LampFactory.Infrastructure.Services.QualityControl;
using LampFactory.Infrastructure.Services.Reports;
using LampFactory.Infrastructure.Services.Sales;
using LampFactory.Infrastructure.Services.Vouchers.ChargeVouchers;
using Microsoft.Extensions.DependencyInjection;

namespace LampFactory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register every FluentValidation validator of the Application assembly.
        // Validators are Application-only contracts, but the single DI front door
        // now lives here: each DTO's validation is discovered without a
        // per-feature registration line, and Application ships no registration
        // file at all.
        services.AddValidatorsFromAssembly(typeof(LampFactory.Application.Common.Results.Result).Assembly);

        // Persistence: the ADO.NET connection factory and every repository
        // implementation behind its application-layer contract.
        services.AddScoped<ApplicationDbContext>();

        // The current-user adapter needs per-request HTTP state, so the
        // accessor is registered beside the only thing that consumes it; both
        // are scoped to the in-flight request.
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // The token service is the only other security implementation that
        // belongs to Infrastructure.
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<ISysActionRepository, SysActionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<IBOMRepository, BOMRepository>();
        services.AddScoped<IProductionPlanRepository, ProductionPlanRepository>();
        services.AddScoped<IProductionDocumentRepository, ProductionDocumentRepository>();
        services.AddScoped<IProductionLineRepository, ProductionLineRepository>();
        services.AddScoped<ILineStopRepository, LineStopRepository>();
        services.AddScoped<IOperatorRepository, OperatorRepository>();

        services.AddScoped<IPurchaseVoucherRepository, PurchaseVoucherRepository>();
        services.AddScoped<IRequestVoucherRepository, RequestVoucherRepository>();
        services.AddScoped<ISalesVoucherRepository, SalesVoucherRepository>();
        services.AddScoped<IChargeVoucherRepository, ChargeVoucherRepository>();
        services.AddScoped<IQualityControlRepository, QualityControlRepository>();

        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Read-side report queries, one contract per report.
        services.AddScoped<IProductionTraceabilityReportQuery, ProductionTraceabilityReportQuery>();
        services.AddScoped<IMaterialConsumptionVarianceReportQuery, MaterialConsumptionVarianceReportQuery>();

        // Feature services: the implementations live beside the data access
        // they orchestrate; each contract stays in Application.
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<ISysActionService, SysActionService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IBOMService, BOMService>();
        services.AddScoped<IProductionPlanService, ProductionPlanService>();
        services.AddScoped<IProductionDocumentService, ProductionDocumentService>();
        services.AddScoped<IProductionLineService, ProductionLineService>();
        services.AddScoped<ILineStopService, LineStopService>();
        services.AddScoped<IOperatorService, OperatorService>();
        services.AddScoped<IPurchaseVoucherService, PurchaseVoucherService>();
        services.AddScoped<IRequestVoucherService, RequestVoucherService>();
        services.AddScoped<ISalesVoucherService, SalesVoucherService>();
        services.AddScoped<IChargeVoucherService, ChargeVoucherService>();
        services.AddScoped<IQualityControlService, QualityControlService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IProductionTraceabilityReportService, ProductionTraceabilityReportService>();
        services.AddScoped<IMaterialConsumptionVarianceReportService, MaterialConsumptionVarianceReportService>();

        return services;
    }
}