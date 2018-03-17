using SqlReflect.Attributes;

namespace SqlReflectTest.Model
{
    [Table("Shipper")]
    public struct Shipper
    {
        [PK]
        public int ShipperID { get; set; }
        public string CompanyName { get; set; }
        public string Phone { get; set; }
    }
}