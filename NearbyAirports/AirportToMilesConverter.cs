﻿using System.Globalization;
using Lab6_Starter.Model;

namespace FWAPPA.NearbyAirports;

/// <summary>
/// Alexander Johnston
/// </summary>
public class AirportToMilesConverter: IValueConverter
{

    private static Dictionary<string, int> _idToMiles = new();
    
    public static void ConvertAll(Dictionary<string, int> idToMiles)
    {
        _idToMiles.Clear();
        foreach (var (id, miles) in idToMiles)
        {
            _idToMiles[id] = miles;
        }
    }
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Airport airport)
        {
            return _idToMiles[airport.Id];
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}