namespace FWAPPA.UI;

public partial class WeatherPage : ContentPage
{
    public WeatherPage()
    {
        InitializeComponent(); 
        // There's really only one control that needs to talk to the BusinessLogic layer, and that's the CollectionView
        BindingContext = MauiProgram.BusinessLogic;
        MauiProgram.BusinessLogic.GetClosestAirportWeather();
    }

    // Various event handlers for the buttons on the main page

    void Fetch_Clicked(object sender, EventArgs e)
    {
        string airportId = AiportEntry.Text;
        MauiProgram.BusinessLogic.GetClosestAirportWeather(airportId);
    }

}


