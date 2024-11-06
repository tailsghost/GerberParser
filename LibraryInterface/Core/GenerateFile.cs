using GerberParser.LibraryInterface.Contracts;
using GerberParser.LibraryInterface.Enums;
using GerberParser.LibraryInterface.Property;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using System.Text;

namespace GerberParser.LibraryInterface.Core;

public class GenerateFile : IGenerateFile
{
    private List<FileType> board = new();

    private Dictionary<string, List<string>> data = new();

    public Dictionary<string, List<string>> getZipFiles(IFormFile file)
    {
        if (file.FileName.ToLower().EndsWith(".zip"))
        {
            using (var zipStream = file.OpenReadStream())
            {
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {

                        if (isValidFile(entry.Name) && !entry.FullName.EndsWith("/"))
                        {

                            using (var ms = new MemoryStream())
                            {
                                using (var entryStream = entry.Open())
                                {
                                    entryStream.CopyTo(ms);
                                }

                                ms.Seek(0, SeekOrigin.Begin);

                                var reader = new StreamReader(ms, Encoding.UTF8);

                                var context = reader.ReadToEnd();


                                var type = FindFileType(ms);

                                if (BoardFileType.Unsupported == type)
                                {
                                    reader.Close();
                                    context = "";
                                    continue;

                                }

                                board.Add(new FileType { BoardFileType = type, Name = entry.Name, Stream = context });

                            }
                        }
                    }

                    foreach (var entry in board)
                    {

                        if (BoardFileType.Gerber == entry.BoardFileType)
                        {
                            BoardSide Side = BoardSide.Unknown;
                            BoardLayer Layer = BoardLayer.Unknown;
                            DetermineBoardSideAndLayer(entry.Name, out Side, out Layer);

                            if (BoardLayer.Copper == Layer && BoardSide.Top == Side)
                            {
                                data.Add("topCopper", new List<string> { entry.Stream });
                            }
                            else if (BoardLayer.Copper == Layer && BoardSide.Bottom == Side)
                            {
                                data.Add("bottomCopper", new List<string> { entry.Stream });
                            }
                            else if (BoardLayer.SolderMask == Layer && BoardSide.Top == Side)
                            {
                                data.Add("topMask", new List<string> { entry.Stream });
                            }
                            else if (BoardLayer.SolderMask == Layer && BoardSide.Bottom == Side)
                            {
                                data.Add("bottomMask", new List<string> { entry.Stream });

                            }
                            else if (BoardLayer.Silk == Layer && BoardSide.Top == Side)
                            {
                                data.Add("topSilk", new List<string> { entry.Stream });
                            }
                            else if (BoardLayer.Silk == Layer && BoardSide.Bottom == Side)
                            {
                                data.Add("bottomSilk", new List<string> { entry.Stream });
                            }
                            else if (BoardLayer.Mill == Layer && BoardSide.Both == Side)
                            {
                                data.Add("mill", new List<string> { entry.Stream });
                            }
                            else if (BoardLayer.Outline == Layer && BoardSide.Both == Side)
                            {
                                data.Add("outline", new List<string> { entry.Stream });
                            }
                        }
                        else if (BoardFileType.Drill == entry.BoardFileType)
                        {
                            if (data.Keys.Contains("drill"))
                                data["drill"].Add(entry.Stream);
                            else
                            {
                                data.Add("drill", new List<string> { entry.Stream });
                            }
                        }

                    }
                }
            }
        }

        return data;
    }

    private bool isValidFile(string filename)
    {
        List<string> unsupported = new List<string>() { "config", "exe", "dll", "png", "zip", "gif", "jpeg", "doc", "docx", "jpg", "bmp", "svg" };
        string[] filesplit = filename.Split('.');
        string ext = filesplit[filesplit.Count() - 1].ToLower();
        foreach (var s in unsupported)
        {
            if (ext == s)
            {

                return false;
            }
        }

        return true;
    }


    private BoardFileType FindFileType(MemoryStream ms)
    {
        try
        {
            ms.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(ms))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Проверка на тип файла
                    if (line.Contains("%FS")) return BoardFileType.Gerber;
                    if (line.Contains("M48")) return BoardFileType.Drill;
                }
            }
        }
        catch
        {
            return BoardFileType.Unsupported;
        }

        return BoardFileType.Unsupported;
    }

    private void DetermineBoardSideAndLayer(string gerberfile, out BoardSide Side, out BoardLayer Layer)
    {
        Side = BoardSide.Unknown;
        Layer = BoardLayer.Unknown;
        string[] filesplit = Path.GetFileName(gerberfile).Split('.');
        string ext = filesplit[filesplit.Count() - 1].ToLower();
        switch (ext)
        {
            case "art": // ORCAD RELATED TYPES
                {

                    switch (Path.GetFileNameWithoutExtension(gerberfile).ToUpper())
                    {
                        case "PMT": Side = BoardSide.Top; Layer = BoardLayer.Paste; break;
                        case "PMB": Side = BoardSide.Bottom; Layer = BoardLayer.Paste; break;
                        case "TOP": Side = BoardSide.Top; Layer = BoardLayer.Copper; break;
                        case "BOTTOM": Side = BoardSide.Bottom; Layer = BoardLayer.Copper; break;
                        case "SMBOT": Side = BoardSide.Bottom; Layer = BoardLayer.SolderMask; break;
                        case "SMTOP": Side = BoardSide.Top; Layer = BoardLayer.SolderMask; break;
                        case "SSBOT": Side = BoardSide.Bottom; Layer = BoardLayer.Silk; break;
                        case "SSTOP": Side = BoardSide.Top; Layer = BoardLayer.Silk; break;

                        case "DRILLING": Side = BoardSide.Both; Layer = BoardLayer.Drill; break;
                    }
                    break;
                }
            case "slices": Side = BoardSide.Both; Layer = BoardLayer.Utility; break;
            case "copper_bottom": Side = BoardSide.Bottom; Layer = BoardLayer.Copper; break;
            case "copper_top": Side = BoardSide.Top; Layer = BoardLayer.Copper; break;
            case "silk_bottom": Side = BoardSide.Bottom; Layer = BoardLayer.Silk; break;
            case "silk_top": Side = BoardSide.Top; Layer = BoardLayer.Silk; break;
            case "paste_bottom": Side = BoardSide.Bottom; Layer = BoardLayer.Paste; break;
            case "paste_top": Side = BoardSide.Top; Layer = BoardLayer.Paste; break;
            case "soldermask_bottom": Side = BoardSide.Bottom; Layer = BoardLayer.SolderMask; break;
            case "soldermask_top": Side = BoardSide.Top; Layer = BoardLayer.SolderMask; break;
            case "drill_both": Side = BoardSide.Both; Layer = BoardLayer.Drill; break;
            case "outline_both": Side = BoardSide.Both; Layer = BoardLayer.Outline; break;
            case "png":
                {
                    Side = BoardSide.Both;
                    Layer = BoardLayer.Silk;
                }
                break;

            case "assemblytop":
                Layer = BoardLayer.Assembly;
                Side = BoardSide.Top;
                break;
            case "assemblybottom":
                Layer = BoardLayer.Assembly;
                Side = BoardSide.Bottom;
                break;
            case "gbr":

                switch (Path.GetFileNameWithoutExtension(gerberfile).ToLower())
                {
                    case "profile":
                    case "boardoutline":
                    case "outline":
                    case "board":
                        Side = BoardSide.Both;
                        Layer = BoardLayer.Outline;
                        break;


                    case "copper_bottom":
                    case "bottom":
                        Side = BoardSide.Bottom;
                        Layer = BoardLayer.Copper;
                        break;

                    case "soldermask_bottom":
                    case "bottommask":
                        Side = BoardSide.Bottom;
                        Layer = BoardLayer.SolderMask;
                        break;

                    case "solderpaste_bottom":
                    case "bottompaste":
                        Side = BoardSide.Bottom;
                        Layer = BoardLayer.Paste;
                        break;

                    case "silkscreen_bottom":
                    case "bottomsilk":
                        Side = BoardSide.Bottom;
                        Layer = BoardLayer.Silk;
                        break;

                    case "copper_top":
                    case "top":
                        Side = BoardSide.Top;
                        Layer = BoardLayer.Copper;
                        break;

                    case "soldermask_top":
                    case "topmask":
                        Side = BoardSide.Top;
                        Layer = BoardLayer.SolderMask;
                        break;

                    case "solderpaste_top":
                    case "toppaste":
                        Side = BoardSide.Top;
                        Layer = BoardLayer.Paste;
                        break;

                    case "silkscreen_top":
                    case "topsilk":
                        Side = BoardSide.Top;
                        Layer = BoardLayer.Silk;
                        break;

                    case "inner1":
                        Side = BoardSide.Internal1;
                        Layer = BoardLayer.Copper;
                        break;

                    case "inner2":
                        Side = BoardSide.Internal2;
                        Layer = BoardLayer.Copper;
                        break;

                    default:
                        {
                            string lcase = gerberfile.ToLower();
                            if (lcase.Contains("board outline")) { Side = BoardSide.Both; Layer = BoardLayer.Outline; };
                            if (lcase.Contains("copper bottom")) { Side = BoardSide.Bottom; Layer = BoardLayer.Copper; };
                            if (lcase.Contains("silkscreen bottom")) { Side = BoardSide.Bottom; Layer = BoardLayer.Silk; };
                            if (lcase.Contains("copper top")) { Side = BoardSide.Top; Layer = BoardLayer.Copper; };
                            if (lcase.Contains("silkscreen top")) { Side = BoardSide.Top; Layer = BoardLayer.Silk; };
                            if (lcase.Contains("solder mask bottom")) { Side = BoardSide.Bottom; Layer = BoardLayer.SolderMask; };
                            if (lcase.Contains("solder mask top")) { Side = BoardSide.Top; Layer = BoardLayer.SolderMask; };
                            if (lcase.Contains("drill-copper top-copper bottom")) { Side = BoardSide.Both; Layer = BoardLayer.Drill; };
                            if (lcase.Contains("outline")) { Side = BoardSide.Both; Layer = BoardLayer.Outline; }
                            if (lcase.Contains("-edge_cuts")) { Side = BoardSide.Both; Layer = BoardLayer.Outline; }
                            if (lcase.Contains("-b_cu")) { Side = BoardSide.Bottom; Layer = BoardLayer.Copper; }
                            if (lcase.Contains("-f_cu")) { Side = BoardSide.Top; Layer = BoardLayer.Copper; }
                            if (lcase.Contains("-b_silks")) { Side = BoardSide.Bottom; Layer = BoardLayer.Silk; }
                            if (lcase.Contains("-f_silks")) { Side = BoardSide.Top; Layer = BoardLayer.Silk; }
                            if (lcase.Contains("-b_mask")) { Side = BoardSide.Bottom; Layer = BoardLayer.SolderMask; }
                            if (lcase.Contains("-f_mask")) { Side = BoardSide.Top; Layer = BoardLayer.SolderMask; }
                            if (lcase.Contains("-b_paste")) { Side = BoardSide.Bottom; Layer = BoardLayer.Paste; }
                            if (lcase.Contains("-f_paste")) { Side = BoardSide.Top; Layer = BoardLayer.Paste; }
                        }
                        break;
                }
                break;

            case "gml":
                Side = BoardSide.Both;
                Layer = BoardLayer.Mill;
                break;

            case "fabrd":
            case "oln":
            case "gko":
            case "gm1":
                Side = BoardSide.Both;
                Layer = BoardLayer.Outline;
                break;

            case "l2":
            case "g1l":
            case "gl1":
            case "g1":
                Side = BoardSide.Internal1;
                Layer = BoardLayer.Copper;
                break;

            case "adtop":
                Side = BoardSide.Top;
                Layer = BoardLayer.Assembly;
                break;

            case "adbottom":
                Side = BoardSide.Bottom;
                Layer = BoardLayer.Assembly;
                break;

            case "notes":
                Side = BoardSide.Both;
                Layer = BoardLayer.Notes;
                break;

            case "l3":
            case "gl2":
            case "g2l":
            case "g2":
                Side = BoardSide.Internal2;
                Layer = BoardLayer.Copper;
                break;

            case "gl3":
            case "g3l":
            case "g3":
                Side = BoardSide.Internal3;
                Layer = BoardLayer.Copper;
                break;

            case "gl4":
            case "g4l":
            case "g4":
                Side = BoardSide.Internal4;
                Layer = BoardLayer.Copper;
                break;

            case "gl5":
            case "g5l":
            case "g5":
                Side = BoardSide.Internal5;
                Layer = BoardLayer.Copper;
                break;
            case "gl6":
            case "g6l":
            case "g6":
                Side = BoardSide.Internal6;
                Layer = BoardLayer.Copper;
                break;
            case "gl7":
            case "g7l":
            case "g7":
                Side = BoardSide.Internal7;
                Layer = BoardLayer.Copper;
                break;
            case "gl8":
            case "g8l":
            case "g8":
                Side = BoardSide.Internal8;
                Layer = BoardLayer.Copper;
                break;

            case "gl9":
            case "g9l":
            case "g9":
                Side = BoardSide.Internal9;
                Layer = BoardLayer.Copper;
                break;
            case "gl10":
            case "g10l":
            case "g10":
                Side = BoardSide.Internal10;
                Layer = BoardLayer.Copper;
                break;
            case "gl11":
            case "g11l":
            case "g11":
                Side = BoardSide.Internal11;
                Layer = BoardLayer.Copper;
                break;
            case "gl12":
            case "g12l":
            case "g12":
                Side = BoardSide.Internal12;
                Layer = BoardLayer.Copper;
                break;
            case "gl13":
            case "g13l":
            case "g13":
                Side = BoardSide.Internal13;
                Layer = BoardLayer.Copper;
                break;
            case "gl14":
            case "g14l":
            case "g14":
                Side = BoardSide.Internal14;
                Layer = BoardLayer.Copper;
                break;
            case "gl15":
            case "g15l":
            case "g15":
                Side = BoardSide.Internal15;
                Layer = BoardLayer.Copper;
                break;
            case "gl16":
            case "g16l":
            case "g16":
                Side = BoardSide.Internal16;
                Layer = BoardLayer.Copper;
                break;
            case "gl17":
            case "g17l":
            case "g17":
                Side = BoardSide.Internal17;
                Layer = BoardLayer.Copper;
                break;
            case "gl18":
            case "g18l":
            case "g18":
                Side = BoardSide.Internal18;
                Layer = BoardLayer.Copper;
                break;
            case "gl19":
            case "g19l":
            case "g19":
                Side = BoardSide.Internal19;
                Layer = BoardLayer.Copper;
                break;
            case "gl20":
            case "g20l":
            case "g20":
                Side = BoardSide.Internal20;
                Layer = BoardLayer.Copper;
                break;

            case "l4":
            case "gbl":
            case "l2m":
                Side = BoardSide.Bottom;
                Layer = BoardLayer.Copper;
                break;

            case "l1":
            case "l1m":
            case "gtl":
                Side = BoardSide.Top;
                Layer = BoardLayer.Copper;
                break;

            case "gbp":
            case "spbottom":
                Side = BoardSide.Bottom;
                Layer = BoardLayer.Paste;
                break;

            case "gtp":
            case "sptop":
                Side = BoardSide.Top;
                Layer = BoardLayer.Paste;
                break;

            case "gbo":
            case "ss2":
            case "ssbottom":
                Side = BoardSide.Bottom;
                Layer = BoardLayer.Silk;
                break;

            case "gto":
            case "ss1":
            case "sstop":
                Side = BoardSide.Top;
                Layer = BoardLayer.Silk;
                break;

            case "gbs":
            case "sm2":
            case "smbottom":
                Side = BoardSide.Bottom;
                Layer = BoardLayer.SolderMask;
                break;

            case "gts":
            case "sm1":
            case "smtop":

                Side = BoardSide.Top;
                Layer = BoardLayer.SolderMask;
                break;
            case "outline":
            case "gb3":
                Side = BoardSide.Both;
                Layer = BoardLayer.Outline;
                break;

            case "gt3":
                Side = BoardSide.Both;
                Layer = BoardLayer.Outline;
                break;

            case "top":
                Side = BoardSide.Top;
                Layer = BoardLayer.Copper;
                break;

            case "bottom":
            case "bot":
                Side = BoardSide.Bottom;
                Layer = BoardLayer.Copper;
                break;

            case "smb":
                Side = BoardSide.Bottom;
                Layer = BoardLayer.SolderMask;
                break;

            case "smt":
                Side = BoardSide.Top;
                Layer = BoardLayer.SolderMask;
                break;

            case "slk":
            case "sst":
                Side = BoardSide.Top;
                Layer = BoardLayer.Silk;
                break;

            case "bsk":
            case "ssb":
                Side = BoardSide.Bottom;
                Layer = BoardLayer.Silk;
                break;

            case "spt":
                Side = BoardSide.Top;
                Layer = BoardLayer.Paste;
                break;

            case "spb":
                Side = BoardSide.Bottom;
                Layer = BoardLayer.Paste;
                break;

            case "drill_top_bottom":
            case "drl":
            case "drill":
            case "drillnpt":
            case "rou":
            case "sco":
                Side = BoardSide.Both;
                Layer = BoardLayer.Drill;
                break;

        }
    }
}
