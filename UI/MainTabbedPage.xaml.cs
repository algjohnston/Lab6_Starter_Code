namespace FWAPPA.UI;

public partial class MainTabbedPage : TabbedPage
{

	Boolean IsMapTabVisible {get;set;}
	
		public MainTabbedPage()
	{
		BindingContext = this;
		InitializeComponent();
		NavigationPage.SetHasNavigationBar(this, false);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

	}


}
