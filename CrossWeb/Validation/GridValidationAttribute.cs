using System.ComponentModel.DataAnnotations;

namespace CrossWeb.Validation
{
    public class GridValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || value is not string[])
            {
                return new ValidationResult("GridValidationAttribute is only for use on string arrays.");
            }

            var grid = value as string[];

            if (grid.Length == 0)
            {
                return new ValidationResult("grid must have a height greater than 0");
            }

            if (grid[0].Length == 0)
            {
                return new ValidationResult("grid must have a width greater than 0");
            }

            var width = grid[0].Length;

            foreach (var row in grid)
            {
                if (row == null || row.Length != width)
                {
                    return new ValidationResult($"All rows must match width of first row: {width}");
                }

                foreach (var c in row)
                {
                    if (c != Crossword.Constants.BoardSymbols.Empty 
                        && c !=  Crossword.Constants.BoardSymbols.Black
                        && !(c >= 'a' && c <= 'z')
                        && !(c >= 'A' && c <= 'Z'))
                    {
                        return new ValidationResult($"Character {c} is not valid.");
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}
