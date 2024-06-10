namespace CrossWeb.Models
{
    public class CrosswordGenerationResponse
    {
        public GenerationResult Result { get; set; }

        public string[] Grid { get; set; }
    }
}
