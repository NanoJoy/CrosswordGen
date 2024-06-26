﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Crossword
{
    class Program
    {
        static void Main(string[] args)
        {
            var wordsFilePath = args[0];

            var specsFilePath = args[1];

            var outputDirectoryPath = args.Length > 2 ? args[2] : null;

            Stream stream = null;

            try
            {

                if (outputDirectoryPath != null)
                {
                    var outputFilePath = Path.Combine(outputDirectoryPath, $"Output_{DateTime.Now:yyyyMMddHHmmss}.txt");
                    stream = File.Create(outputFilePath);
                    Utils.SetOutputFile(stream);
                }

                var wordTryOrderString = args.Length > 3 ? args[3] : null;

                var wordTryOrder = wordTryOrderString == null ? WordTryOrder.Random : Enum.Parse<WordTryOrder>(wordTryOrderString);

                var wordFilter = new WordFilter(wordsFilePath);

                foreach (var spec in ParseSpecs(specsFilePath))
                {

                    var generator = new CrosswordGenerator(wordFilter, spec.Width, spec.Height, wordTryOrder);

                    Console.WriteLine("Input:\n");
                    Utils.WritePuzzle(generator.GetStartPuzzle(spec.ExistingValues));

                    Console.WriteLine("Generating...");

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var puzzle = generator.GenerateCrossword(spec.ExistingValues);

                    stopwatch.Stop();

                    Console.WriteLine($"Finished in {stopwatch.Elapsed}");

                    if (puzzle != null)
                    {
                        Console.WriteLine("Result:\n");
                        Utils.WritePuzzle(puzzle);

                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine("No matching puzzles found.\n");
                    }
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }

        static IEnumerable<PuzzleSpecification> ParseSpecs(string filePath)
        {
            using var specsFile = File.OpenRead(filePath);
            using var reader = new StreamReader(specsFile);

            var numPuzzles = int.Parse(reader.ReadLine());

            for (int p = 0; p < numPuzzles; p++)
            {

                var width = int.Parse(reader.ReadLine());
                var height = int.Parse(reader.ReadLine());
                var values = new List<SquareValue>();

                for (int i = 0; i < height; i++)
                {
                    var line = reader.ReadLine();

                    for (int j = 0; j < width; j++)
                    {
                        var c = line[j];

                        if (c == '#' || c >= 'A' || c <= 'Z')
                        {
                            values.Add(new SquareValue(i, j, c));
                        }
                    }
                }
                yield return new PuzzleSpecification(width, height, values.ToArray());
            }

        }
    }
}
