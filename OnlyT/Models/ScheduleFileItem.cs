using System.IO;

namespace OnlyT.Models;

public class ScheduleFileItem
{
    public ScheduleFileItem(string fileName)
    {
        FileName = fileName;
        DisplayName = Path.GetFileNameWithoutExtension(fileName);
    }

    public string FileName { get; }

    public string DisplayName { get; }
}
