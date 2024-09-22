// See https://aka.ms/new-console-template for more information
using Google.OrTools.LinearSolver;

class Region
{
    public string Name { get; set; }
    public int BaseLoad { get; set; }
    public int BaseLatency { get; set; }
    public bool IsCritical { get; set; } = false;
    public Variable SmallInstances { get; set; }
    public Variable MediumInstances { get; set; }
    public Variable LargeInstances { get; set; }

    public Dictionary<string, Variable> RoutedTraffic { get; set; } = new Dictionary<string, Variable>();

    public LinearExpr TotalLoad { get; set; } = new LinearExpr();
    public LinearExpr TotalCU { get; set; } 
    public LinearExpr LatencyLimit { get; set; }
}