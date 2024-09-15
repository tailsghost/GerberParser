namespace GerberParser.Abstracts.NetList;

public abstract class LogicalNet
{
    protected string name {  get; init; }

    protected HashSet<PhysicalNet> connectedNets { get; init; }

    protected HashSet<PhysicalNet> clearanceNets { get; init; }

    public LogicalNet(string name)
    {
        this.name = name;
        this.connectedNets = new HashSet<PhysicalNet>();
        this.clearanceNets = new HashSet<PhysicalNet>();
    }

    protected abstract void AssignPhysical(HashSet<PhysicalNet> connected, HashSet<PhysicalNet> clearance);
}
