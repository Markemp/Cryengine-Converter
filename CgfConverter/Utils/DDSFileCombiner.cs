using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace CgfConverter.Utils;

public class DDSFileCombiner
{
    public static string Combine(string baseFileName, string combinedFileNameIdentifier = "combined")
    {
        if (baseFileName is null)
            return string.Empty;

        string? directory = Path.GetDirectoryName(baseFileName);

        if (directory is null)
            return baseFileName;

        string fileNameWithExtension = Path.GetFileName(baseFileName);

        // Get all files that start with the base file name and have a numeric extension
        List<string> filesToCombine = new() { Path.Combine(directory, fileNameWithExtension) };

        filesToCombine.AddRange(Directory.GetFiles(directory, $"{fileNameWithExtension}*")
                    .Where(path => Path.GetFileName(path).StartsWith($"{fileNameWithExtension}.") &&
                                   int.TryParse(Path.GetExtension(path).TrimStart('.'), out _))
                    .OrderByDescending(path => int.Parse(Path.GetExtension(path).TrimStart('.'))).ToList());


        // If no files to combine, inform the user and exit the program
        if (!filesToCombine.Any())
            throw new Exception("No matching part files found.");

        // Create a new combined file
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameWithExtension);
        string extension = Path.GetExtension(fileNameWithExtension);
        string combinedFileName = $"{fileNameWithoutExtension}.{combinedFileNameIdentifier}{extension}";

        try
        {
            using FileStream combinedFileStream = new(combinedFileName, FileMode.Create);

            // Iterate over each file and append it to the combined file
            foreach (string filePath in filesToCombine)
            {
                using FileStream partFileStream = new(filePath, FileMode.Open);
                partFileStream.CopyTo(combinedFileStream);
            }

            return combinedFileName;
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred: {ex.Message}");
        }
    }

    public static Stream CombineToStream(string baseFileName)
    {
        if (baseFileName is null)
            throw new ArgumentException("Base file name cannot be null or empty.", nameof(baseFileName));

        string? directory = Path.GetDirectoryName(baseFileName);

        if (directory is null)
            throw new ArgumentException("Base file name must contain a directory.", nameof(baseFileName));

        string fileNameWithExtension = Path.GetFileName(baseFileName);

        var files = Directory.GetFiles(directory, $"{fileNameWithExtension}.*");

        // Get all files that start with the base file name and have a numeric extension
        //var filesToCombine = Directory.GetFiles(directory, $"{fileNameWithExtension}*")
        //    .Where(path => Path.GetFileName(path).StartsWith($"{fileNameWithExtension}.") &&
        //                   int.TryParse(Path.GetExtension(path).TrimStart('.'), out _))
        //    .OrderByDescending(path => int.Parse(Path.GetExtension(path).TrimStart('.'))) // Order files by descending order
        //    .ToList();

        // If no files to combine, inform the user and exit
        if (!files.Any())
            throw new Exception("No matching part files found.");

        // Create a new MemoryStream to hold the combined file data
        MemoryStream combinedStream = new();

        try
        {
            // Append the base file to the combined file first, as it contains the header
            using FileStream baseFileStream = new(baseFileName, FileMode.Open);
            baseFileStream.CopyTo(combinedStream);

            // Iterate over each file and append it to the combined MemoryStream
            for (int i = files.Length - 1; i >= 1; i--)
            {
                using FileStream partFileStream = new(files[i], FileMode.Open);
                partFileStream.CopyTo(combinedStream);
            }
            //foreach (string filePath in filesToCombine)
            //{
            //    using FileStream partFileStream = new(filePath, FileMode.Open);
            //    partFileStream.CopyTo(combinedStream);
            //}

            // Reset the position of the MemoryStream to the beginning
            combinedStream.Position = 0;

            return combinedStream;
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred: {ex.Message}");
        }
    }

    public static bool IsSplitDDSIsSplitDDS(string baseFileName)
    {
        // Check if baseFileName is null or empty
        if (string.IsNullOrEmpty(baseFileName))
            throw new ArgumentException("Base file name cannot be null or empty.", nameof(baseFileName));

        string directory = Path.GetDirectoryName(baseFileName);
        string fileNameWithExtension = Path.GetFileName(baseFileName);

        // Check if a file exists that starts with the base file name and ends with '.1'
        return File.Exists(Path.Combine(directory, $"{fileNameWithExtension}.1"));
    }

}
