namespace LampFactory.Domain.Operators
{
    public class Operator : AuditableEntity
    {
        public int OperatorID { get; set; }
        public string NationalID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? HiringDate { get; set; }
    }
}
