using GerberParser.Constants;
using GerberParser.Core.PCB;

namespace GerberParser.LibraryInterface.Core;

public class LoadPCB
{
    public CircuitBoard Start(Dictionary<string, List<string>> files)
    {
        var outline = files["outline"].FirstOrDefault();
        var drill = files["drill"];

        string drillNonplated = "\0";
        string mill = "\0";
        double platingThickness = 0.5 * COPPER_OZ.Value;

        var board = new CircuitBoard(outline, drill, drillNonplated, mill, platingThickness);

        if (files.ContainsKey("bottomMask"))
        {
            var bottomMask = files["bottomMask"].First();
            var bottomSilk = files["bottomSilk"].First();
            board.Add_Mask_Layer(bottomMask, bottomSilk);
        }

        if (files.ContainsKey("bottomCopper"))
        {
            var bottomCopper = files["bottomCopper"].First();
            board.Add_Copper_Layer(bottomCopper, COPPER_OZ.Value);
        }

        board.Add_Substrate_Layer(1.5);

        if (files.ContainsKey("topCopper"))
        {
            var topCopper = files["topCopper"].First();
            board.Add_Copper_Layer(topCopper, COPPER_OZ.Value);
        }

        if (files.ContainsKey("topMask"))
        {
            var topMask = files["topMask"].First();
            var topSilk = files["topSilk"].First();
            board.Add_Mask_Layer(topMask, topSilk);
        }

        board.Add_surface_finish();
        files.Clear();

        return board;
    }
}
