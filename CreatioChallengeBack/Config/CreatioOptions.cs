namespace CreatioChallengeBack.Config
{
    public class CreatioOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = string.Empty; // OAuth token endpoint (configurable)
        public string ClientId { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public int TokenCacheSeconds { get; set; } = 300;
        public string AllowedOrigins { get; set; } = "http://localhost:4200";
    }
}