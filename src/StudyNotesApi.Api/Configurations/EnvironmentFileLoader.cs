namespace StudyNotesApi.Api.Configurations;

public static class EnvironmentFileLoader
{
    public static string? LoadFromSolutionRoot(string fileName = ".env")
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory is not null)
        {
            var candidatePath = Path.Combine(currentDirectory.FullName, fileName);

            if (File.Exists(candidatePath))
            {
                LoadFile(candidatePath);
                return candidatePath;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }

    private static void LoadFile(string filePath)
    {
        foreach (var rawLine in File.ReadAllLines(filePath))
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
            {
                continue;
            }

            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
