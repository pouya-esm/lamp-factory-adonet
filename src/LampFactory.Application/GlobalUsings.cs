// Application services and validators reference Domain entities and the shared
// envelope across feature folders. These imports keep each file's import block
// limited to the feature it actually owns.
//
// QualityControl is imported per-file for the same reason as in Domain: the
// aggregate shares its namespace name.
global using LampFactory.Application.Common.Models;
global using LampFactory.Domain.Common;
global using LampFactory.Domain.Enums;
global using LampFactory.Domain.IdentityAccess;
global using LampFactory.Domain.Inventory;
global using LampFactory.Domain.Operators;
global using LampFactory.Domain.Production;
global using LampFactory.Domain.Purchasing;
global using LampFactory.Domain.Sales;
global using LampFactory.Domain.Vouchers;
