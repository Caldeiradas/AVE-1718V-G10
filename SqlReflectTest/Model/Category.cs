using SqlReflect.Attributes;

namespace SqlReflectTest.Model
{
    [Table("Categories")]
    public struct Category
    {
        [PK]
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }

        public Category(int categoryID, string categoryName, string description)
        {
            CategoryID = categoryID;
            CategoryName = categoryName;
            Description = description;

        }
        public override string ToString()
        {
            return "" + CategoryID;
        }
    }

    
}