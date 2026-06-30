using System.Collections.ObjectModel;

namespace FWAPPA.Model;

public class Route
{
    // Start and end nodes
    public RoutePoint Start => Edges.FirstOrDefault()?.From;
    public RoutePoint End => Edges.LastOrDefault()?.To;

    public double Length => Edges.Select(edge => edge.Distance).Aggregate((a, b) => a + b);

    // Generate a list of all the points
    public List<RoutePoint> Points
    {
        get
        {
            List<RoutePoint> points = [];
            if (Edges.Count > 0)
            {
                foreach (RouteEdge edge in Edges)
                {
                    points.Add(edge.From);
                }

                points.Add(Edges.Last().To);
            }

            return points;
        }
    }


    // Collection of edges
    public ObservableCollection<RouteEdge> Edges { get; set; }

    public Route()
    {
        Edges = [];
    }

    // Constructor to initialize the Route with start, end, and edges
    public Route(RoutePoint start, RoutePoint end)
    {
        Edges = [new(start, end)];
    }

    // Method to add a point on the end
    public void AddPointOnEnd(RoutePoint point)
    {
        Edges.Add(new RouteEdge(End, point));
    }

    /// <summary>
    /// Uses a solution to the traveling salesman problem to generate a good route.
    /// </summary>
    /// <param name="points">The points to get to on the route, starts and ends with the first in the list.</param>
    /// <returns>A decent route that visits each non-starting points exactly once.</returns>
    public static Route GenerateTravelingSalesmanRoute(List<RoutePoint> points)
    {
        // We need at least 2 points
        if (points.Count < 2)
        {
            return null;
        }

        List<RoutePoint> mutPoints = points.Skip(1).ToList();

        // Get the first step (nearest neighbor) for route object creation
        RoutePoint source = points[0];
        RoutePoint firstStep = mutPoints.MinBy(points[0].DistanceFrom);
        mutPoints.Remove(firstStep);

        Route route = new(source, firstStep);

        // Using nearest neighbor, get the path we should take
        while (mutPoints.Count > 0)
        {
            RoutePoint last = route.End;
            RoutePoint closest = mutPoints.MinBy(last.DistanceFrom);
            route.AddPointOnEnd(closest);
            mutPoints.Remove(closest);
        }

        // Finally, go back to the first point
        route.AddPointOnEnd(source);

        return route;
    }
}

public class RouteEdge
{
    public RoutePoint From { get; set; }
    public RoutePoint To { get; set; }
    public double Distance => To.DistanceFrom(From);

    public RouteEdge(RoutePoint from, RoutePoint to)
    {
        From = from;
        To = to;
    }
}

public class RoutePoint
{
    private const double NM_PER_MI = 0.868976;

    public double X { get; set; }
    public double Y { get; set; }
    public WisconsinAirport Airport { get; set; }

    public RoutePoint(WisconsinAirport airport)
    {
        Airport = airport;
        X = airport.Longitude;
        Y = airport.Latitude;
    }

    public double DistanceFrom(RoutePoint other)
    {
        return BusinessLogic.GetDistanceBetweenCoordinates(
            new Coordinates((float)Y, (float)X),
            new Coordinates((float)other.Y, (float)other.X)) * NM_PER_MI;
    }
}