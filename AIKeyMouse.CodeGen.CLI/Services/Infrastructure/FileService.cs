using AIKeyMouse.CodeGen.CLI.Models.Configuration;

namespace AIKeyMouse.CodeGen.CLI.Services.Infrastructure;

/// <summary>
/// Service for file I/O operations
/// </summary>
public class FileService
{
    private readonly ILogger<FileService> _logger;
    private readonly CliConfig _config;

    public FileService(ILogger<FileService> logger, ConfigurationService configService)
    {
        _logger = logger;
        _config = configService.GetConfiguration();
    }

    /// <summary>
    /// Read text file content
    /// </summary>
    public async Task<string> ReadFileAsync(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        _logger.LogDebug("Reading file: {FilePath}", path);
        return await File.ReadAllTextAsync(path);
    }

    /// <summary>
    /// Write text content to file
    /// </summary>
    public async Task WriteFileAsync(string path, string content, bool createBackup = true)
    {
        // Create directory if it doesn't exist
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger.LogDebug("Created directory: {Directory}", directory);
        }

        // Create backup if file exists and backup is enabled
        if (createBackup && File.Exists(path) && _config.CodeGeneration.CreateBackups)
        {
            await CreateBackupAsync(path);
        }

        _logger.LogDebug("Writing file: {FilePath}", path);
        await File.WriteAllTextAsync(path, content);
        _logger.LogInformation("File written successfully: {FilePath}", path);
    }

    /// <summary>
    /// Create a backup of an existing file
    /// </summary>
    public async Task<string> CreateBackupAsync(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var backupPath = $"{path}.{timestamp}.bak";

        await Task.Run(() => File.Copy(path, backupPath, overwrite: true));
        
        _logger.LogInformation("Backup created: {BackupPath}", backupPath);
        return backupPath;
    }

    /// <summary>
    /// Check if file exists
    /// </summary>
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    /// <summary>
    /// Check if directory exists
    /// </summary>
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    /// <summary>
    /// Create directory if it doesn't exist
    /// </summary>
    public void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            _logger.LogDebug("Created directory: {Directory}", path);
        }
    }

    /// <summary>
    /// Get all files matching a pattern in a directory
    /// </summary>
    public string[] GetFiles(string directory, string searchPattern = "*", bool recursive = false)
    {
        if (!Directory.Exists(directory))
        {
            _logger.LogWarning("Directory not found: {Directory}", directory);
            return Array.Empty<string>();
        }

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return Directory.GetFiles(directory, searchPattern, searchOption);
    }

    /// <summary>
    /// Get file name from path
    /// </summary>
    public string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }

    /// <summary>
    /// Get file name without extension
    /// </summary>
    public string GetFileNameWithoutExtension(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    /// <summary>
    /// Get directory name from path
    /// </summary>
    public string? GetDirectoryName(string path)
    {
        return Path.GetDirectoryName(path);
    }

    /// <summary>
    /// Combine paths
    /// </summary>
    public string CombinePaths(params string[] paths)
    {
        return Path.Combine(paths);
    }

    /// <summary>
    /// Get absolute path
    /// </summary>
    public string GetFullPath(string path)
    {
        return Path.GetFullPath(path);
    }

    /// <summary>
    /// Delete file if it exists
    /// </summary>
    public void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
            _logger.LogDebug("Deleted file: {FilePath}", path);
        }
    }

    /// <summary>
    /// Move/rename file
    /// </summary>
    public void MoveFile(string sourcePath, string destinationPath, bool overwrite = false)
    {
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"Source file not found: {sourcePath}");
        }

        if (File.Exists(destinationPath) && !overwrite)
        {
            throw new IOException($"Destination file already exists: {destinationPath}");
        }

        File.Move(sourcePath, destinationPath, overwrite);
        _logger.LogDebug("Moved file from {SourcePath} to {DestinationPath}", sourcePath, destinationPath);
    }

    /// <summary>
    /// Get file size in bytes
    /// </summary>
    public long GetFileSize(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        return new FileInfo(path).Length;
    }

    /// <summary>
    /// Get file last modified time
    /// </summary>
    public DateTime GetLastWriteTime(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        return File.GetLastWriteTime(path);
    }

    /// <summary>
    /// Read JSON file and deserialize
    /// </summary>
    public async Task<T?> ReadJsonAsync<T>(string path)
    {
        var content = await ReadFileAsync(path);
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        });
    }

    /// <summary>
    /// Serialize and write JSON file
    /// </summary>
    public async Task WriteJsonAsync<T>(string path, T data, bool createBackup = true)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var content = JsonSerializer.Serialize(data, options);
        await WriteFileAsync(path, content, createBackup);
    }

    /// <summary>
    /// Find files matching a glob pattern (e.g., "Pages/**/*Page.cs")
    /// </summary>
    public async Task<List<string>> FindFilesAsync(string pattern)
    {
        _logger.LogDebug("Searching for files matching: {Pattern}", pattern);
        
        // Parse the pattern to determine base directory and search pattern
        var parts = pattern.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        var baseDir = parts.Length > 0 ? parts[0] : ".";
        var searchPattern = parts.Length > 1 ? string.Join(Path.DirectorySeparatorChar, parts.Skip(1)) : "*";
        
        var results = new List<string>();
        
        // Check if base directory exists
        if (!Directory.Exists(baseDir))
        {
            _logger.LogDebug("Base directory not found: {BaseDir}\", baseDir);
            return results;
        }
        
        // Perform search
        try
        {
            var allFiles = Directory.GetFiles(baseDir, "*.*", SearchOption.AllDirectories);
            foreach (var file in allFiles)
            {
                var relativePath = Path.GetRelativePath(baseDir, file);
                if (MatchesPattern(relativePath, searchPattern))
                {
                    results.Add(file);
                }
            }
            
            _logger.LogDebug("Found {Count} files matching pattern", results.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error searching for files with pattern: {Pattern}", pattern);
        }
        
        return await Task.FromResult(results);
    }
    
    private bool MatchesPattern(string path, string pattern)
    {
        // Simple pattern matching - supports ** for any subdirectory
        var patternRegex = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*\\*", ".*")
            .Replace("\\*", "[^/\\\\]*")
            .Replace("\\?", ".")
            + "$";
            
        return System.Text.RegularExpressions.Regex.IsMatch(
            path.Replace('\\', '/'),
            patternRegex.Replace('\\', '/'),
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}
