using CrossWeb.Validation;

namespace CrossWeb.Models
{
    public class CrosswordGenerationRequest
    {
        [GridValidation]
        public string[] Grid { get; set; }

        public Crossword.WordTryOrder WordTryOrder { get; set; } = Crossword.WordTryOrder.Balanced;
    }
}
 