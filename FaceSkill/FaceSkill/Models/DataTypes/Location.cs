namespace FaceSkill.Models.DataTypes
{
    public class Location
    {
        public EdmGeographyPoint Position { get; set; }

        public Address Address { get; set; }
    }
}
