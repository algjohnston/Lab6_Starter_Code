using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using FWAPPA.Model;
using Syncfusion.Maui.Calendar;

namespace FWAPPA.UI;

public partial class EnterAirportDetailsPopup : Popup
{
    private readonly bool isEdit;
    private const string GreyStarPath = "ic_fluent_star_24_filled_grey.png";
    private const string YellowStarPath = "ic_fluent_star_24_filled_yellow.png";
    private string id = "";
    private string city = "";
    private DateTime? dateVisited;
    private int rating;
    private readonly string? airportToEditId;

    public EnterAirportDetailsPopup(VisitedAirport? airport)
    {
        InitializeComponent();
        if (airport != null) // only null if it's an edit
        {
            isEdit = true; // technically we could use whether airportToEditId is null to check this, but this is more clear
            IdLabel.IsVisible = false;
            IdEntry.IsVisible = false;
            IdEntry.Text = airport.Id; // This is not visible ???
            airportToEditId = airport.Id;
            CityEntry.Text = airport.Name;
            Calendar.View = CalendarView.Month;
            Calendar.SelectedDate = airport.DateVisited;
            FillStars(airport.Rating);
        }
        else // Default the Calendar to Today's date
        {
            DateTime today = DateTime.Today;
            Calendar.View = CalendarView.Month;
            Calendar.DisplayDate = today;
            Calendar.SelectedDate = today;
        }
    }

    private void OnCalendarSelectionChanged(object sender, EventArgs e)
    {
        DateTime? selectedDate = Calendar.SelectedDate;
        if (selectedDate == null)
        {
            return;
        }

        dateVisited = selectedDate.Value;
    }

    // select rating
    private void Star_Clicked(object sender, EventArgs e)
    {
        if (sender is not ImageButton button)
        {
            return;
        }

        int starCount =
            Convert.ToInt32(button
                .CommandParameter); // the button we press has a parameter that tells us which one it is
        FillStars(starCount);
    }

    private void FillStars(int numStars)
    {
        var stars = new[] { starOne, starTwo, starThree, starFour, starFive };
        rating = numStars;
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].Source =
                (i < numStars) ? YellowStarPath : GreyStarPath; // sets as many stars to yellow as were clicked
        }
    }

    private void Ok_Clicked(object sender, EventArgs e)
    {
        var action = isEdit ? (Action)EditAirport : (Action)AddAirport;
        action(); // this isn't super necessary, but it looks kinda neat (pretty self-explanatory here too)
    }

    private async void AddAirport()
    {
        try
        {
            id = IdEntry.Text;
            city = CityEntry.Text;
            // Date is set in response to calendar events; see OnCalendarSelectionChanged
            if (dateVisited == null)
            {
                IToast toast = Toast.Make("A date need to be selected");
                await toast.Show();
                return;
            }

            // Rating is set in response to clicking a star; see Star_Clicked.
            AirportAdditionError error = await MauiProgram.BusinessLogic.AddAirport(
                id,
                city,
                dateVisited.Value,
                rating
            );
            var errorMessage = error switch
            {
                AirportAdditionError.InvalidIdLength => "Id length is not between 3 and 4",
                AirportAdditionError.InvalidCityLength => "City length is not between 1 and 25",
                AirportAdditionError.InvalidRating => "Rating is not selected",
                AirportAdditionError.InvalidDate => "Date is invalid",
                AirportAdditionError.DuplicateAirportId => "Airport id is already used",
                AirportAdditionError.NoError => $"Successfully Added Airport {id}",
                _ => error.ToString()
            };
            IToast errorMessageToast = Toast.Make(errorMessage);
            await errorMessageToast.Show();
            if (error == AirportAdditionError.NoError) // switch is prettier, but we pay for it here I suppose
            {
                Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in addAirport(): {ex.Message}");
            IToast errorMessageToast = Toast.Make($"Exception: {ex.Message}", ToastDuration.Long);
            await errorMessageToast.Show();
        }
    }

    private async void EditAirport()
    {
        city = CityEntry.Text;
        // Fields should never be null when editing
        AirportEditError error = await MauiProgram.BusinessLogic.EditAirport(
            airportToEditId!,
            city,
            (DateTime)dateVisited!,
            rating
        );

        string errorMessage;
        switch (error)
        {
            case AirportEditError.AirportNotFound:
                errorMessage = "Airport not found";
                break;
            case AirportEditError.InvalidCityLength:
                errorMessage = "City length is not between 3 and 25, exclusive";
                break;
            case AirportEditError.InvalidRating:
                errorMessage = "Rating is not selected";
                break;
            case AirportEditError.InvalidDate:
                errorMessage = "Date is invalid";
                break;
            case AirportEditError.DBEditError:
                errorMessage = "Error when trying to update airport";
                break;
            case AirportEditError.NoError:
                errorMessage = $"Successfully Edited Airport {id}";
                Close();
                break;
            default:
                errorMessage = error.ToString();
                break;
        }

        IToast errorMessageToast = Toast.Make(errorMessage, ToastDuration.Long);
        await errorMessageToast.Show();

        //mainCV.SelectedItem = MauiProgram.BusinessLogic.FindAirport(id);
    }


    private void Cancel_Clicked(object sender, EventArgs e)
    {
        Close();
    }
}