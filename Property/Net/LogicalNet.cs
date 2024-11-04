namespace GerberParser.Property.Net;

public class LogicalNet
{
    public string name { get; init; }

    public HashSet<PhysicalNet> connectedNets { get; init; }

    public HashSet<PhysicalNet> clearanceNets { get; init; }

    public LogicalNet(string name)
    {
        this.name = name;
        connectedNets = new HashSet<PhysicalNet>();
        clearanceNets = new HashSet<PhysicalNet>();
    }

    public void AssignPhysical(PhysicalNet connected, PhysicalNet clearance)
    {
        connectedNets.Add(connected);
        clearanceNets.Add(clearance);
    }
}
