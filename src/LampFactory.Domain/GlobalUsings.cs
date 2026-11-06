// The Domain project is intentionally dependency-free. These global usings let
// entities reference siblings across bounded contexts (e.g. a PurchaseVoucher
// referencing Warehouse, or ProductionDocument referencing Product) without each
// file repeating the same import block.
//
// The QualityControl context is deliberately absent: its aggregate shares the
// name of its namespace, so importing it globally would make every `QualityControl`
// reference ambiguous. Files there import it explicitly.
global using LampFactory.Domain.Common;
global using LampFactory.Domain.Enums;
global using LampFactory.Domain.IdentityAccess;
global using LampFactory.Domain.Inventory;
global using LampFactory.Domain.Operators;
global using LampFactory.Domain.Production;
global using LampFactory.Domain.Purchasing;
global using LampFactory.Domain.Sales;
global using LampFactory.Domain.Vouchers;
