namespace CreatioChallengeBack.Dtos
{
    // DTO público expuesto por nuestra API. No expone directamente el modelo interno de Creatio.
    public class CreatioContactDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
