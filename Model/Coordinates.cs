namespace FWAPPA.Model;

public class Coordinates(double latitude, double longitude)
{
    public double Latitude { get; } = latitude;

    public double Longitude { get; } = longitude;
}