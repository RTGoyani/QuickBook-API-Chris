namespace QuickBookAccountApi.Model
{
    public class QuickbooksTokenModel
    {
        public int Id { get; set; }
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiry { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}
