namespace GerberParser.Abstracts.NetList;

public abstract class Netlist
{
    protected int numLayers;
    protected PhysicalNetlist connectedNetlist;
    protected PhysicalNetlist clearanceNetlist;
    protected List<string> builderViolations = new List<string>();
    protected Dictionary<string, LogicalNet> logicalNets = new Dictionary<string, LogicalNet>();

    public abstract List<string> PerformDRC(int annularRing);

    public abstract PhysicalNetlist GetPhysicalNetlist();

    public abstract Dictionary<string, LogicalNet> GetLogicalNetlist();
}
