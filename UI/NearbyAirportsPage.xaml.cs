using CommunityToolkit.Maui.Core.Platform;
using FWAPPA.Model;

namespace FWAPPA.UI;

/// <summary>
/// Written by Hiba Seraj
/// Alexander Johnston forked and cloned the project, and added the navigation.
/// A page that displays nearby airports.
/// </summary>
public partial class NearbyAirportsPage : ContentPage
{
    private readonly IBusinessLogic businessLogic = MauiProgram.BusinessLogic;

    public NearbyAirportsPage()
    {
        InitializeComponent();
        BindingContext = businessLogic;
    }

    /// <summary>
    /// Update Nearby airport when the user click on the button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnSearchNearbyAirportBtn(object sender, EventArgs e)
    {
        AirportEntry.HideKeyboardAsync(CancellationToken.None);
        DistanceEntry.HideKeyboardAsync(CancellationToken.None);
        string airportId = AirportEntry.Text;
        string distanceMileText = DistanceEntry.Text;

        if (airportId == null)
        {
            await DisplayAlert("", "Please enter a valid airport name", "OK");
            return;
        }

        WisconsinAirport airport = businessLogic.SelectAirportByCode(airportId);
        bool isValidDistance = int.TryParse(distanceMileText, out int distanceMile);
        if (airport == null)
        {
            await DisplayAlert("Error", "Airport not found", "OK");
            return;
        }

        if (!isValidDistance)
        {
            await DisplayAlert("Error", "Distance is invalid", "OK");
            return;
        }

        if (distanceMile < 0)
        {
            await DisplayAlert("Error", "Distance must be greater than 0", "OK");
            return;
        }

        businessLogic.CalculateNearbyAirports(airport, distanceMile);
        
        // May be needed to get the correct distances
        AirportList.ItemsSource = null;
        AirportList.ItemsSource = businessLogic.NearbyAirports;
    }
}