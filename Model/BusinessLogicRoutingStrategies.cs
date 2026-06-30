namespace FWAPPA.Model;

public partial class BusinessLogic
{
    
    private bool isRoutingStrategiesHeaderVisible;
    public bool IsRoutingStrategiesHeaderVisible
    {
        get => isRoutingStrategiesHeaderVisible;
        set
        {
            if (isRoutingStrategiesHeaderVisible != value)
            {
                isRoutingStrategiesHeaderVisible = value;
                OnPropertyChanged(nameof(IsRoutingStrategiesHeaderVisible));
            }
        }
    }
    
    /// <summary>
    /// Finds a route to visit all airports within a specific range from a starting airport.
    /// The route will end at the starting airport.
    /// </summary>
    /// <param name="source">The starting airport.</param>
    /// <param name="maxMiles">How many miles from the source the airports in the route should be.</param>
    /// <param name="unvisitedOnly">If true, then visited airports will not be in the route.</param>
    /// <returns></returns>
    public Route? GetRoute(WisconsinAirport source, int maxMiles, bool unvisitedOnly)
    {
        // We need to force the start to be at the beginning, so we remove it
        // and possibly already visited airports
        List<WisconsinAirport> excluded = [source];
        if (unvisitedOnly)
        {
            IEnumerable<WisconsinAirport> allAirports = GetAllWisconsinAirports();
            IEnumerable<string?> visitedAirportIds = visitedAirports.Select(airport => airport.Id);
            excluded.AddRange(
                allAirports.Where(airport => visitedAirportIds.Contains(airport.Id))
            );
        }

        // Convert the airports to RoutePoints
        CalculateNearbyAirports(source, maxMiles);
        List<RoutePoint> routePoints = NearbyAirports
            .Except(excluded, new WisconsinAirportEqualityComparer())
            .Prepend(source)
            .Select(x => new RoutePoint(x))
            .ToList();

        // Can't have a route with 0 or 1 airports
        if (routePoints.Count < 2)
        {
            IsRoutingStrategiesHeaderVisible = false;
            return null;
        }
        IsRoutingStrategiesHeaderVisible = true;
        
        return Route.GenerateTravelingSalesmanRoute(routePoints);
    }
}