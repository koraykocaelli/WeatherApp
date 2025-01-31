namespace WeatherApp.Helpers
{
    public static class IconHelper
    {
        public static string GetLocalIcon(string icon)
        {
            return icon switch
            {
                "01d" or "01n" => "sunny.jpg",
                "02d" or "02n" => "partly_cloudy.jpg",
                "03d" or "03n" or "04d" or "04n" => "cloudy.jpg",
                "09d" or "09n" or "10d" or "10n" => "rainy.jpg",
                "11d" or "11n" => "stormy.jpg",
                "13d" or "13n" => "snowy.jpg",
                "50d" or "50n" => "foggy.jpg",
                _ => "unknown.jpg"
            };
        }
    }
}
