// See https://aka.ms/new-console-template for more information
using Google.OrTools.LinearSolver;



Solver solver = Solver.CreateSolver("SCIP"); 

if (solver == null)
{
    Console.WriteLine("Solver not created.");
    return;
}

// Define cost per instance type.
double costSmall = 36.5;  // Monthly cost of small instance
double costMedium = 36.5;  // Monthly cost of medium instance
double costLarge = 146;  // Monthly cost of large instance

// Define Compute Units (CU) per instance type.
double cuPerSmall = 50;
double cuPerMedium = 150;
double cuPerLarge = 300;

double latencyCriticalityFactor = 0.8; //used in order to virtually increase latency for critical regions so it will demand more resources

// Latency and capacity constraints
double maxLatency = 30.0;  // Maximum allowed latency
double minCUPerRegion = 1000.0;  // Minimum CU required in each region

// Define regions
Region[] regions = new Region[]
{
    new Region { Name = "UsEast", BaseLoad = 4500, BaseLatency = 50, IsCritical = true },
    new Region { Name = "UsWest", BaseLoad = 3200, BaseLatency = 60 },
    new Region { Name = "EuCentral", BaseLoad = 3250, BaseLatency = 58, IsCritical = true  },
    new Region { Name = "Asia", BaseLoad = 5500, BaseLatency = 80 }
};

// Decision variables (number of instances in each region).
foreach (var region in regions)
{
    region.SmallInstances = solver.MakeIntVar(0.0, double.PositiveInfinity, $"small_{region.Name}");
    region.MediumInstances = solver.MakeIntVar(0.0, double.PositiveInfinity, $"medium_{region.Name}");
    region.LargeInstances = solver.MakeIntVar(0.0, double.PositiveInfinity, $"large_{region.Name}");
}

// Traffic routing variables (percentage of traffic routed between regions).
foreach (var srcRegion in regions)
{
    foreach (var dstRegion in regions)
    {
        srcRegion.RoutedTraffic[dstRegion.Name] = solver.MakeNumVar(0.0, 1.0, $"traffic_{srcRegion.Name}_to_{dstRegion.Name}");
    }
}

// Add constraints.
// Constraint 1: Traffic routed from each region must sum to 1.
foreach (var srcRegion in regions)
{
    LinearExpr routedSum = new LinearExpr();
    foreach (var dstRegion in regions)
    {
        routedSum += srcRegion.RoutedTraffic[dstRegion.Name];
    }
    solver.Add(routedSum == 1.0);
}

// Constraint 2: Total cost must be less than $15,000.
LinearExpr totalCost = new LinearExpr();
for (int i = 0; i < regions.Length; i++)
{
    totalCost += regions[i].SmallInstances * costSmall + regions[i].MediumInstances * costMedium + regions[i].LargeInstances * costLarge;
}
solver.Add(totalCost <= 15000.0);


// Constraint 3: Latency and capacity constraints per region.
foreach (var dstRegion in regions)
{
    // Calculate total load for region i.
    foreach (var srcRegion in regions)
    {
        dstRegion.TotalLoad += srcRegion.RoutedTraffic[dstRegion.Name] * srcRegion.BaseLoad;
    }

    // Calculate the total Compute Units (CU) in the region.
    dstRegion.TotalCU = dstRegion.SmallInstances * cuPerSmall + dstRegion.MediumInstances * cuPerMedium +dstRegion.LargeInstances * cuPerLarge;

    // Add minimal capacity constraint.
    solver.Add(dstRegion.TotalCU >= minCUPerRegion);
    

    // Add latency constraint.
    dstRegion.LatencyLimit = dstRegion.TotalCU * maxLatency;
    if (dstRegion.IsCritical)
    {
        dstRegion.LatencyLimit *= latencyCriticalityFactor;
    }
    solver.Add(dstRegion.TotalLoad*dstRegion.BaseLatency <= dstRegion.LatencyLimit);
}

// Objective: Minimize the total cost.
Objective objective = solver.Objective();
for (int i = 0; i < regions.Length; i++)
{
    objective.SetCoefficient(regions[i].SmallInstances, costSmall);
    objective.SetCoefficient(regions[i].MediumInstances, costMedium);
    objective.SetCoefficient(regions[i].LargeInstances, costLarge);
}
objective.SetMinimization();

// Solve the problem.
Solver.ResultStatus resultStatus = solver.Solve();

// Check that the problem has an optimal solution.
if (resultStatus == Solver.ResultStatus.OPTIMAL)
{
    Console.WriteLine("Solution found:");

    double totalCostValue = 0.0;
    foreach (var region in regions)
    {
        double smallInstances = region.SmallInstances.SolutionValue();
        double mediumInstances = region.MediumInstances.SolutionValue();
        double largeInstances = region.LargeInstances.SolutionValue();

        Console.WriteLine($"{region.Name} - Small instances: {smallInstances}, Medium instances: {mediumInstances}, Large instances: {largeInstances}");

        Console.WriteLine($"{region.Name} - TotalLoad: {region.TotalLoad.SolutionValue()} LatencyLimit: {region.LatencyLimit.SolutionValue()}");

        foreach(var routedTraffic in region.RoutedTraffic)
        {
            Console.WriteLine($"{region.Name} - Routed traffic to {routedTraffic.Key}: {routedTraffic.Value.SolutionValue()}");
        }
        totalCostValue += smallInstances * costSmall + mediumInstances * costMedium + largeInstances * costLarge;

        Console.WriteLine();
        Console.WriteLine();
    }

    Console.WriteLine($"Total cost: {totalCostValue}");
}
else
{
    Console.WriteLine("No optimal solution found.");
}
