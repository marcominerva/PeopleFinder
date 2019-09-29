namespace FaceSkill.Models.DataTypes
{
    public class EdmGeographyPoint
    {
        public string Type => "Point";

        public double[] Coordinates { get; private set; }

        public EdmGeographyPoint(double latitude, double longitude)
        {
            Coordinates = new double[] { longitude, latitude };
        }

        public static EdmGeographyPoint Create(double? latitude, double? longitude)
        {
            if (latitude.HasValue && longitude.HasValue)
            {
                return new EdmGeographyPoint(latitude.Value, longitude.Value);
            }

            return null;
        }
    }
}
