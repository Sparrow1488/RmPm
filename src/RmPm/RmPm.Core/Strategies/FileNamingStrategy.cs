namespace RmPm.Core.Strategies;

/// <summary>
/// Стратегия именования файлов
/// </summary>
public abstract class FileNamingStrategy
{
    public abstract string[] SelectFiles(string[] filePaths);
    public abstract string GetNewName(string extension, string[] exists);
}

/// <summary>
/// Последовательная стратегия именования. Пример: example.txt, example1.txt, example2.txt etc.
/// </summary>
public sealed class ConsistentNamingStrategy : FileNamingStrategy
{
    private readonly Func<FileIndex, bool> _selectionFilter;

    public ConsistentNamingStrategy(string sharedFileName, Func<FileIndex, bool>? selectionFilter = null)
    {
        _selectionFilter = selectionFilter ?? (_ => true);
        SharedFileName = sharedFileName;
    }
    
    private string SharedFileName { get; }
    
    public override string[] SelectFiles(string[] filePaths)
    {
        var files = filePaths.Where(x => Path.GetFileName(x).StartsWith(SharedFileName)).ToArray();
        return GetIndex(files, _selectionFilter).Select(x => x.FullPath).ToArray();
    }

    private FileIndex[] GetIndex(string[] files, Func<FileIndex, bool>? filter = default)
    {
        return files.Select(BuildIndex).Where(filter ?? (_ => true)).ToArray();
    }

    private FileIndex BuildIndex(string filePath)
    {
        var numberString = string.Join("", 
            filePath.Remove(0, SharedFileName.Length)
                .SkipWhile(x => !char.IsDigit(x))
                .TakeWhile(char.IsDigit)
        );
        
        int.TryParse(numberString, out var number);
        
        return new FileIndex(filePath, number == 0 ? null : number);
    }
    
    public override string GetNewName(string extension, string[] exists)
    {
        var files = SelectFiles(exists);
        var indexArr = GetIndex(files);
        var lastNumber = indexArr.Max(x => x.Number);

        if (lastNumber is null or 0)
        {
            return SharedFileName + extension;
        }

        return SharedFileName + ++lastNumber + extension;
    }

    public record FileIndex(string FullPath, int? Number);
}