using SqlReflect.Attributes;


namespace SqlReflectTest.Model
{
    [Table("Region")]
    public class Region
    {
        [PK]
        public int RegionID { get; set; }
        public string RegionDescription { get; set; }

    }
}
