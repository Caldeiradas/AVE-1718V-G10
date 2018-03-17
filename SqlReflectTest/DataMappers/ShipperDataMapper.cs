using SqlReflect;
using SqlReflectTest.Model;
using System;
using System.Data.SqlClient;

namespace SqlReflectTest.DataMappers
{
    class ShipperDataMapper : AbstractDataMapper
    {
        const string COLUMNS = "CompanyName, Phone";
        const string SQL_GET_ALL = @"SELECT ShipperID, " + COLUMNS + " FROM Shippers";
        const string SQL_GET_BY_ID = SQL_GET_ALL + " WHERE ShipperID=";
        const string SQL_INSERT = "INSERT INTO Shippers (" + COLUMNS + ") OUTPUT INSERTED.ShipperID VALUES ";
        const string SQL_DELETE = "DELETE FROM Shippers WHERE ShipperID = ";
        const string SQL_UPDATE = "UPDATE Shippers SET CompanyName={1}, Phone={2} WHERE ShipperID = {0}";

        public ShipperDataMapper(string connStr) : base(connStr)
        {
        }

        protected override string SqlGetAll()
        {
            return SQL_GET_ALL;
        }
        protected override string SqlGetById(object id)
        {
            return SQL_GET_BY_ID + id;
        }

        protected override object Load(SqlDataReader dr)
        {
            Shipper c = new Shipper
            {
                ShipperID = (int)dr["ShipperID"],
                CompanyName = (string)dr["CompanyName"],
                Phone = (string)dr["Phone"]
            };
            return c;
        }

        protected override string SqlInsert(object target)
        {
            Shipper c = (Shipper)target;
            string values = "'" + c.CompanyName + "' , '" + c.Phone + "'";
            return SQL_INSERT + "(" + values + ")";
        }

        protected override string SqlUpdate(object target)
        {
            Shipper c = (Shipper)target;
            return String.Format(SQL_UPDATE,
                c.ShipperID,
                "'" + c.CompanyName + "'",
                "'" + c.Phone + "'");
        }

        protected override string SqlDelete(object target)
        {
            Shipper c = (Shipper)target;
            return SQL_DELETE + c.ShipperID;
        }
    }
}
