using System.Globalization;

namespace FWAPPA.Model.NearbyAirports;

/// <summary>
/// Alexander Johnston
/// </summary>

public class AirportToMilesConverter: IValueConverter
{

    private static readonly Dictionary<string, int> _idToMiles = new();
    
    public static void ConvertAll(Dictionary<string, int> idToMiles)
    {
        _idToMiles.Clear();
        foreach (var (id, miles) in idToMiles)
        {
            _idToMiles[id] = miles;
        }
    }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is WisconsinAirport airport)
        {
            // It is possible for Convert to be called before ConvertAll, so the guard is necessary.
            bool found = _idToMiles.TryGetValue(airport.Id, out var airportMiles);
            if (found)
            {
                return airportMiles;
            }
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}