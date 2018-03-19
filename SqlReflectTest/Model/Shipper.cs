using SqlReflect.Attributes;

namespace SqlReflectTest.Model
{
    [Table("Shippers")]
    public struct Shipper
    {
        [PK]
        public int ShipperID { get; set; }
        public string CompanyName { get; set; }
        public string Phone { get; set; }

        public override string ToString()
        {
            return "" + ShipperID;
        }
    }
}