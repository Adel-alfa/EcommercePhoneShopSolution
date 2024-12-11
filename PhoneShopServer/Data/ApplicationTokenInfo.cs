namespace PhoneShopServer.Data
{
    public class ApplicationTokenInfo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? CreatedDate { get; set; }= DateTime.Now;
        public DateTime? ExpiredDate { get; set; } = DateTime.Now.AddDays(1);
    }
}
