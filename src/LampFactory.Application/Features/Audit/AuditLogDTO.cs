
using LampFactory.Application.Features.Audit;

namespace LampFactory.Application.Features.Audit
{
    // Read projection for the admin audit-log view. One row per business record,
    // describing only that record's latest state change — created or last
    // modified — because the schema tracks a single audit stamp per entity
    // rather than a full change history.
    public class AuditLogDTO
    {
        public string EntityName { get; set; } = string.Empty;
        public int EntityID { get; set; }
        public string EntityLabel { get; set; } = string.Empty;
        public string LastAction { get; set; } = string.Empty;
        public DateTime? LastAt { get; set; }
        public int? LastBy { get; set; }
        public string? LastByUsername { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByUsername { get; set; }
    }
}
