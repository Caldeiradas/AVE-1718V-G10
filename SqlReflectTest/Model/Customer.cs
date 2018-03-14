using SqlReflect.Attributes;

namespace SqlReflectTest.Model
{
    [Table("Customer")]
    public class Customer
    {
        [PK]
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
    }
}