// The ADO.NET repositories map between Domain entities and SQL rows, so they
// reach across every bounded context. These imports keep each repository's own
// import block limited to the feature it belongs to.
//
// The current-user contract is an Application abstraction; every repository
// resolves the audit actor through it rather than through a concrete type.
//
// QualityControl is imported per-file for the same reason as in Domain: the
// aggregate shares its namespace name, so a global import would make every
// `QualityControl` reference ambiguous.
global using LampFactory.Application.Abstractions.Security;
global using LampFactory.Domain.Common;
global using LampFactory.Domain.Enums;
global using LampFactory.Domain.IdentityAccess;
global using LampFactory.Domain.Inventory;
global using LampFactory.Domain.Operators;
global using LampFactory.Domain.Production;
global using LampFactory.Domain.Purchasing;
global using LampFactory.Domain.Sales;
global using LampFactory.Domain.Vouchers;
