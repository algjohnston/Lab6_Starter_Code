using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FWAPPA.Model;

public partial class BusinessLogic(IDatabaseSupa db) : IBusinessLogic
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    const int BRONZE_LEVEL = 42;
    const int SILVER_LEVEL = 84;
    const int GOLD_LEVEL = 128;

    private readonly int MAX_RATING = 5;

    private readonly SortableObservableCollection<VisitedAirport> visitedAirports = [];
    public ObservableCollection<VisitedAirport> VisitedAirports => visitedAirports; // this is all the Visited Airports

    private Route currentRoute = new();

    public Route CurrentRoute
    {
        get => currentRoute;
        set
        {
            if (currentRoute != value)
            {
                currentRoute = value;
            }
        }
    }

    private ObservableCollection<WisconsinAirport> WisconsinAirports =>
        GetAllWisconsinAirports(); // this is all 142 or so Wisconsin Airports

    public ObservableCollection<WisconsinAirport> GetAllWisconsinAirports()
    {
        return db.GetAllWisconsinAirports();
    }

    public ObservableCollection<WisconsinAirport> GetWisconsinAirportsWithinDistance(double userLatitude,
        double userLongitude, double maxDistanceKm)
    {
        return db.GetWisconsinAirportsWithinDistance(userLatitude, userLongitude, maxDistanceKm);
    }

    public WisconsinAirport SelectAirportByCode(string airportCode)
    {
        return db.SelectAirportByCode(airportCode.ToUpper());
    }


    public async Task<VisitedAirport?> FindAirport(string id)
    {
        return await db.SelectAirport(id);
    }

    private AirportAdditionError CheckAirportFields(string? id, string? name, DateTime? dateVisited, int rating)
    {
        if (id == null || id.Length < 3 || id.Length > 4)
        {
            return AirportAdditionError.InvalidIdLength;
        }

        if (name == null || name.Length < 3 || name.Length > 25)
        {
            return AirportAdditionError.InvalidCityLength;
        }

        if (rating < 1 || rating > MAX_RATING)
        {
            return AirportAdditionError.InvalidRating;
        }

        if (dateVisited == null)
        {
            return AirportAdditionError.InvalidDate;
        }

        return AirportAdditionError.NoError;
    }


    public async Task<AirportAdditionError> AddAirport(string id, string name, DateTime dateVisited, int rating)
    {
        var result = CheckAirportFields(id, name, dateVisited, rating);
        if (result != AirportAdditionError.NoError)
        {
            return result;
        }

        var potentialDuplicateAirport = await db.SelectAirport(id);
        if (potentialDuplicateAirport != null) // this now is true, because db.selectAirport(id) returns a Task ... oops
        {
            return AirportAdditionError.DuplicateAirportId;
        }

        VisitedAirport airport = new VisitedAirport(
            id,
            name,
            dateVisited,
            rating
        ); // this will never be null, we check in checkAirportFields

        await db.InsertAirport(airport);
        visitedAirports.Add(airport);
        visitedAirports.Sort(visitedAirport => visitedAirport.Id);
        return AirportAdditionError.NoError;
    }


    public async Task<AirportDeletionError> DeleteAirport(string id)
    {
        var entry = await db.SelectAirport(id);

        if (entry != null)
        {
            AirportDeletionError success = await db.DeleteAirport(entry);
            if (success == AirportDeletionError.NoError)
            {
                var airportToRemove = visitedAirports.FirstOrDefault(va => va.Id == id);
                if (airportToRemove != null)
                {
                    visitedAirports.Remove(airportToRemove);
                }

                return AirportDeletionError.NoError;
            }
            else
            {
                return AirportDeletionError.DBDeletionError;
            }
        }
        else
        {
            return AirportDeletionError.AirportNotFound;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="dateVisited"></param>
    /// <param name="rating"></param>
    /// <returns></returns>
    public async Task<AirportEditError> EditAirport(string id, string name, DateTime dateVisited, int rating)
    {
        var fieldCheck = CheckAirportFields(id, name, dateVisited, rating);
        // Id should never be invalid since it is from a previously created instance
        if (fieldCheck != AirportAdditionError.NoError)
        {
            var error = fieldCheck switch
            {
                AirportAdditionError.InvalidCityLength => AirportEditError.InvalidCityLength,
                AirportAdditionError.InvalidRating => AirportEditError.InvalidRating,
                AirportAdditionError.InvalidDate => AirportEditError.InvalidDate,
                _ => AirportEditError.DBEditError
            };
            return error;
        }

        VisitedAirport? editedAirport = await db.SelectAirport(id); // get the airport to edit from the database
        if (editedAirport == null)
        {
            // This should never happen either
            return AirportEditError.AirportNotFound;
        }

        editedAirport.Id = id; // change the airport's fields; id shouldn't change?
        editedAirport.Name = name;
        editedAirport.DateVisited = dateVisited;
        editedAirport.Rating = rating;

        AirportEditError updateAirportStatus = await db.UpdateAirport(editedAirport); // update it in Supabase
        if (updateAirportStatus != AirportEditError.NoError) // updated in Supabase?
        {
            return updateAirportStatus;
        }

        var originalAirport = visitedAirports.FirstOrDefault(va => va.Id == id); // find it locally
        if (originalAirport == null)
        {
            return AirportEditError.AirportNotFound;
        }

        visitedAirports.Remove(originalAirport); // and update the observable collection
        visitedAirports.Add(editedAirport);
        return AirportEditError.NoError;
    }

    public string CalculateStatistics()
    {
        FlyWisconsinLevel nextLevel;
        int numAirportsUntilNextLevel;

        int numAirportsVisited = VisitedAirports.Count;
        if (numAirportsVisited < BRONZE_LEVEL)
        {
            nextLevel = FlyWisconsinLevel.Bronze;
            numAirportsUntilNextLevel = BRONZE_LEVEL - numAirportsVisited;
        }
        else if (numAirportsVisited < SILVER_LEVEL)
        {
            nextLevel = FlyWisconsinLevel.Silver;
            numAirportsUntilNextLevel = SILVER_LEVEL - numAirportsVisited;
        }
        else if (numAirportsVisited < GOLD_LEVEL)
        {
            nextLevel = FlyWisconsinLevel.Gold;
            numAirportsUntilNextLevel = GOLD_LEVEL - numAirportsVisited;
        }
        else
        {
            nextLevel = FlyWisconsinLevel.None;
            numAirportsUntilNextLevel = 0;
        }

        return string.Format("{0} airport{1} visited; {2} airports remaining until achieving {3}",
            numAirportsVisited, numAirportsVisited != 1 ? "s" : "", numAirportsUntilNextLevel, nextLevel);
    }

    public async Task<ObservableCollection<VisitedAirport>> GetVisitedAirports()
    {
        try
        {
            ObservableCollection<VisitedAirport> airports = await db.SelectAllVisitedAirports(); // grab all the airports

            airports = new ObservableCollection<VisitedAirport>(airports.OrderBy(a => a.Id));


            visitedAirports.Clear(); // empty out visitedAirports
            await MainThread.InvokeOnMainThreadAsync(() =>
{
    foreach (var airport in airports) // add each of the fetched airports in turn to visitedAirports
    {
        visitedAirports.Add(airport);
    }
});
            return visitedAirports;
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Error loading visited airports: {ex.Message}");
        }

        return visitedAirports;
    }

    public ObservableCollection<WisconsinAirport> GetWisconsinAirports()
    {
        return db.GetAllWisconsinAirports();
    }
    
    /// <summary>
    /// Uses the Haversine formula to find the great-circle distance between two coordinates.
    /// </summary>
    /// <param name="sourceCoordinates">The source coordinates.</param>
    /// <param name="destinationCoordinates">The destination coordinates.</param>
    /// <returns>The great-circle distance between the given coordinates.</returns>
    public static double GetDistanceBetweenCoordinates(
        Coordinates sourceCoordinates,
        Coordinates destinationCoordinates
    )
    {
        // Haversine formula to find distance between two points
        double sourceLatitudeRadians = sourceCoordinates.Latitude * (Math.PI / 180);
        double destinationLatitudeRadians = destinationCoordinates.Latitude * (Math.PI / 180);
        double latitudeDiffRadians =
            (destinationCoordinates.Latitude - sourceCoordinates.Latitude) * (Math.PI / 180);
        double longitudeDiffRadians =
            (destinationCoordinates.Longitude - sourceCoordinates.Longitude) * (Math.PI / 180);
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