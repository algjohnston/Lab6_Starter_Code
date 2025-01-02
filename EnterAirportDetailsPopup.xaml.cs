using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Lab6_Starter.Model;
using Syncfusion.Maui.Calendar;


namespace Lab6_Starter;

public partial class EnterAirportDetailsPopup : Popup
{
    private bool isEdit;
    const string greyStarPath = "Resources/Images/ic_fluent_star_24_filled_grey.svg";
    const string yellowStarPath = "Resources/Images/ic_fluent_star_24_filled_yellow.svg";
    private string id = "";
    private string city = "";
    private DateTime? dateVisited = null; 
    private int rating = 0;
    private string airportToEditId;

    public EnterAirportDetailsPopup (VisitedAirport airport)
    {
        //this.mainCV = mainCV;
        // this.isEdit = isEdit; // what was this about??
        InitializeComponent();
        Console.WriteLine("Popup Opened");
        if (airport != null) // only null if it's an edit
        {
            isEdit = true; // technically we could use whether airportToEditId is null to check this, but this is more clear
            IdLabel.IsVisible = false;
            IdEntry.IsVisible = false;
            airportToEditId = airport.Id;
            IdEntry.Text = airport.Id;
            CityEntry.Text = airport.Name;
            Calendar.View = CalendarView.Month;
           // Calendar.DisplayDate = airport.DateVisited;
            Calendar.SelectedDate = airport.DateVisited;
           // FillStars(airport.Rating);
        }
        else // Default the Calendar to Today's date
        {
            DateTime today = DateTime.Today;
            Calendar.View = CalendarView.Month;
            Calendar.DisplayDate = today;
            Calendar.SelectedDate = today;
        }
    }
    

    void OnCalendarSelectionChanged(object sender, EventArgs e)
    {
        dateVisited = Calendar.SelectedDate ;
    }
    
    //select rating
    private void Star_Clicked(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        {
            int starCount = Convert.ToInt32(button.CommandParameter); // the button we press has a parameter that tells us which one it is
            FillStars(starCount);
        }
    }
    
    private void FillStars (int numStars) {
        starOne.Source = starTwo.Source = starThree.Source = starFour.Source = starFive.Source = greyStarPath;
        var stars = new[] { starOne, starTwo, starThree, starFour, starFive };
        rating = numStars;
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].Source = (i < numStars) ? yellowStarPath : greyStarPath; // sets as many stars to yellow as were clicked
        }
    }

    void Ok_Clicked(object sender, EventArgs e)
    {
        string errorMessage;
        var action = isEdit ? (Action)editAirport : (Action)addAirport;
        action(); // this isn't super necessary, but it looks kinda neat (pretty self-explanatory here too)
    }

    async void addAirport()
    {
        string errorMessage;
        id = IdEntry.Text;
        city = CityEntry.Text;
        AirportAdditionError error = await MauiProgram.BusinessLogic.AddAirport(id, city, dateVisited, rating);
        errorMessage = error.ToString() switch
        {
            "InvalidIdLength" => "Id length is not between 3 and 4",
            "InvalidCityLength" => "City length is not between 1 and 25",
            "InvalidRating" => "Rating is not selected",
            "InvalidDate" => "Date is invalid",
            "DuplicateAirportId" => "Airport id is already used",
            "NoError" => $"Successfully Added Airport {id}",
            _ => error.ToString()
        };
        IToast errorMessageToast = Toast.Make(errorMessage);
        await errorMessageToast.Show();
        if (error.ToString() == "NoError") // switch is prettier, but we pay for it here I suppose
        {
            Close();
        }
    }

    async void editAirport()
    {
        string errorMessage;
        city = CityEntry.Text;
        AirportEditError error = await MauiProgram.BusinessLogic.EditAirport(airportToEditId, city, (DateTime)dateVisited, rating);
        switch (error.ToString())
        {
            case "AirportNotFound":
                errorMessage = "Airport not found";
                break;
            case "NoError":
                errorMessage = $"Successfully Edited Airport {id}";
                Close();
                break;
            default: errorMessage = error.ToString();
                break;
        }
        IToast errorMessageToast = Toast.Make(errorMessage, ToastDuration.Long);
        errorMessageToast.Show();

        //mainCV.SelectedItem = MauiProgram.BusinessLogic.FindAirport(id);
    }
    
    
    void Cancel_Clicked(object sender, EventArgs e)
    {
        Close();
    }


}
