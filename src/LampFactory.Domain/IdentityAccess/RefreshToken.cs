namespace LampFactory.Domain.IdentityAccess
{
    public class RefreshToken
    {
        public int TokenID { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public int UserID { get; set; }
    }
}
