using Microsoft.AspNetCore.Http;

namespace GerberParser.LibraryInterface.Contracts;

public interface IGenerateFile
{
    Dictionary<string, List<string>> getZipFiles(IFormFile file);
}
