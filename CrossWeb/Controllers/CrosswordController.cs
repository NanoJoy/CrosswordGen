using CrossWeb.Models;
using CrossWeb.Providers;
using Crossword;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace CrossWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CrosswordController : ControllerBase
    {
        private readonly ILogger<CrosswordController> _logger;

        private readonly IWordFilterProvider _wordFilterProvider;

        public CrosswordController(ILogger<CrosswordController> logger, IWordFilterProvider wordFilterProvider)
        {
            _logger = logger;
            _wordFilterProvider = wordFilterProvider;
        }

        [HttpPost(Name = "DetermineCrosswordGrid")]
        public CrosswordGenerationResponse Post([FromBody] CrosswordGenerationRequest request)
        {
            var generator = new CrosswordGenerator(_wordFilterProvider.Filter, request.Grid.Length, request.Grid[0].Length, request.WordTryOrder);
            var puzzleSpce = GetPuzzleSpec(request.Grid);

            var puzzle = generator.GenerateCrossword(puzzleSpce.ExistingValues);

            if (puzzle == null)
            {
                return new CrosswordGenerationResponse { Result = GenerationResult.NoResults };
            }

            return new CrosswordGenerationResponse { Result = GenerationResult.Success, Grid = CollapseGrid(puzzle) };
        }

        private PuzzleSpecification GetPuzzleSpec(string[] grid)
        {
            var values = new List<SquareValue>();

            var width = grid[0].Length;

            for (int i = 0; i < grid.Length; i++)
            {
                var line = grid[i].ToUpperInvariant();

                for (int j = 0; j < grid[i].Length; j++)
                {
                    var c = line[j];

                    if (c == Constants.BoardSymbols.Black || c >= 'A' || c <= 'Z')
                    {
                        values.Add(new SquareValue(i, j, c));
                    }
                }
            }

            return new PuzzleSpecification(width, grid.Length, values.ToArray());
        }

        private static string[] CollapseGrid(char[][] grid)
        {
            var result = new string[grid.Length];

            for (int i = 0; i < grid.Length; i++)
            {
                var b = new StringBuilder();

                for (int j = 0; j < grid[i].Length; j++)
                {
                    b.Append(grid[i][j]);
                }

                result[i] = b.ToString();
            }

            return result;
        }
    }
}
