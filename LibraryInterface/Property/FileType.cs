using GerberParser.LibraryInterface.Enums;

namespace GerberParser.LibraryInterface.Property;

public class FileType
{
    public string Stream { get; set; }
    public string Name { get; set; }
    public BoardFileType BoardFileType { get; set; }
}
