namespace CreatioChallengeBack.Dtos
{
    public class CreateAccountRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Web { get; set; }
        public string TypeId { get; set; } = string.Empty;
    }
}
