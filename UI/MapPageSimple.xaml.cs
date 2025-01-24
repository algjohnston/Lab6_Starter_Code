using Mapsui;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.UI.Maui;

namespace FWAPPA.UI;

public partial class MapPageSimple : ContentPage
{
	public MapPageSimple()
	{
		MapControl.UseGPU = false;

		InitializeComponent();

		InitializeMap();
	}

	public void InitializeMap()
	{
		Mapsui.Map map = new();
		map.Layers.Add(OpenStreetMap.CreateTileLayer());
		MRect wisconsinBox = new MRect(
			SphericalMercator.FromLonLat(-93.1, 41.7).x,
			SphericalMercator.FromLonLat(-93.1, 41.7).y,
			SphericalMercator.FromLonLat(-86, 47.3).x,
			SphericalMercator.FromLonLat(-86, 47.3).y
		);


		map.Navigator.ZoomToBox(wisconsinBox);
		MyMapControl.Map = map;

	}
	protected override void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);
	}

}
