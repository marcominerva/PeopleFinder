namespace Search42.Core.Models
{
    public class Position
    {
        public double? Latitude { get; }

        public double? Longitude { get; }

        public Position(double[] coordinates)
        {
            Latitude = coordinates?[1];
            Longitude = coordinates?[0];
        }
    }
}
