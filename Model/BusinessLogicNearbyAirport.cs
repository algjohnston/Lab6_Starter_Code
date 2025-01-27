using System.Collections.ObjectModel;
using FWAPPA.Model.NearbyAirports;

namespace FWAPPA.Model;

/// <summary>
/// Jason Wang
/// </summary>
public partial class BusinessLogic
{
    private const int EARTH_RADIUS_IN_METERS = 6378000;
    private const double MILES_PER_METER = 0.00062137;

    public ObservableCollection<WisconsinAirport> NearbyAirports { get; } = [];

    private bool isNearbyAirportsHeaderVisible;

    public bool IsNearbyAirportsHeaderVisible
    {
        get => isNearbyAirportsHeaderVisible;
        set
        {
            if (isNearbyAirportsHeaderVisible != value)
            {
                isNearbyAirportsHeaderVisible = value;
                OnPropertyChanged(nameof(IsNearbyAirportsHeaderVisible));
            }
        }
    }

    /// <summary>
    /// Find all airports within maxMiles of sourceAirport
    /// </summary>
    /// <param name="sourceAirport">The airport whose location to use as reference.</param>
    /// <param name="maxMiles">How far the desired airports are to be from the sourceAirport.</param>
    /// <returns></returns>
    public void CalculateNearbyAirports(WisconsinAirport sourceAirport, int maxMiles)
    {
        Dictionary<string, int> idToMiles = new();
        List<WisconsinAirport> tempNearbyAirports = new();

        ObservableCollection<WisconsinAirport> allAirports = GetWisconsinAirports();

        foreach (WisconsinAirport destinationAirport in allAirports) // why aren't we using a dictionary here??
        {
            WisconsinAirport? destinationAirportCoordinates = WisconsinAirports.FirstOrDefault(
                airport => airport?.Id == destinationAirport.Id && airport.Id != sourceAirport.Id, null
            );

            if (destinationAirportCoordinates != null)
            {
                double distanceInMiles = GetDistanceFromAirportCoordinates(
                    sourceAirport,
                    destinationAirportCoordinates
                );
                if (distanceInMiles < maxMiles)
                {
                    tempNearbyAirports.Add(destinationAirport);
                    idToMiles[destinationAirport.Id] = (int)Math.Round(distanceInMiles);
                }
            }
        }

        AirportToMilesConverter.ConvertAll(idToMiles); // converter now has distances to display

        NearbyAirports.Clear();
        foreach (var airport in tempNearbyAirports)
        {
            NearbyAirports.Add(airport);
        }

        IsNearbyAirportsHeaderVisible = NearbyAirports.Count > 0; // no airports? no headers necessary
    }

    public static double GetDistanceFromAirportCoordinates(
        WisconsinAirport sourceAirport,
        WisconsinAirport destinationAirport
    )
    {
        // Haversine formula to find distance between two points
        double sourceLatitudeRadians = sourceAirport.Latitude * (Math.PI / 180);
        double destinationLatitudeRadians = destinationAirport.Latitude * (Math.PI / 180);
        double latitudeDiffRadians =
            (destinationAirport.Latitude - sourceAirport.Latitude) * (Math.PI / 180);
        double longitudeDiffRadians =
            (destinationAirport.Longitude - sourceAirport.Longitude) * (Math.PI / 180);
        double flatDistance = Math.Pow(Math.Sin(latitudeDiffRadians / 2.0), 2.0) +
                              (Math.Cos(sourceLatitudeRadians) *
                               Math.Cos(destinationLatitudeRadians) *
                               Math.Pow(Math.Sin(longitudeDiffRadians / 2.0), 2.0));
        double angularDistance = 2 * Math.Atan2(Math.Sqrt(flatDistance), Math.Sqrt(1 - flatDistance));
        double distanceInMeters = EARTH_RADIUS_IN_METERS * angularDistance;
        double distanceInMiles = distanceInMeters * MILES_PER_METER;
        return distanceInMiles;
    }
}