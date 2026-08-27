namespace Common;

public static class DotEnv
{
    public static void Load(string? filePath = null)
    {
        filePath ??= Path.Combine(Directory.GetCurrentDirectory(), ".env");

        if (!File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                continue;

            var parts = trimmed.Split('=', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
