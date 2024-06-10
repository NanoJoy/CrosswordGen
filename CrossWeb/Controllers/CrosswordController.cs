using CrossWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Crossword;
using System.Reflection.PortableExecutable;
using System.Text;

namespace CrossWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CrosswordController : ControllerBase
    {
        private readonly ILogger<CrosswordController> _logger;

        public CrosswordController(ILogger<CrosswordController> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "DetermineCrosswordGrid")]
        public CrosswordGenerationResponse Post([FromBody] CrosswordGenerationRequest request)
        {
            var filter = new WordFilter(@"D:\Projects\crossword\crosswordlist.txt");
            var generator = new CrosswordGenerator(filter, request.Grid.Length, request.Grid[0].Length, request.WordTryOrder);
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

                    if (c == '#' || c >= 'A' || c <= 'Z')
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
