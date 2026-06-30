using CsvHelper.Configuration.Attributes;

namespace FWAPPA.Model;

/// <summary>
/// A WIAirport represents a Wisconsin airport, with 
/// </summary>
public class WisconsinAirport
{

    [Index(0)]
    public String Id { get; set; }

    [Index(1)]
    public String Name { get; set; }

    [Index(2)]
    public double Latitude { get; set; }

    [Index(3)]
    public double Longitude { get; set; }

    [Index(4)]
    public String Url { get; set; }

    public double Distance {get;set;}
    
    public Coordinates Coordinates {get;}

    public WisconsinAirport(String id, String name, double latitude, double longitude, string url){
        Id = id;
        Name = name;
        Latitude = latitude;
        Longitude = longitude;
        Url = url;
        Coordinates = new Coordinates(latitude, longitude);
    }

    override
    public String ToString(){
        return $"{Id}, {Name}, {Latitude}, {Longitude}, {Url}";
    }
    
    public override bool Equals(object? obj)
    {
        return obj is WisconsinAirport otherAirport && Id == otherAirport.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}

public class WisconsinAirportEqualityComparer : IEqualityComparer<WisconsinAirport>
{
    public bool Equals(WisconsinAirport? x, WisconsinAirport? y)
    {
        if (x == null || y == null)
        {
            return false;
        }

        return x.Equals(y);
    }

    public int GetHashCode(WisconsinAirport obj)
    {
        return obj.Id!.GetHashCode();
    }
}