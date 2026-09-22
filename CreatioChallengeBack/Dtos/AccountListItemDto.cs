namespace CreatioChallengeBack.Dtos
{
    public class AccountListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        // Resolved lookup value for Type
        public string? TypeName { get; set; }
    }
}
