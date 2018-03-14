using SqlReflect;
using SqlReflectTest.Model;
using System;
using System.Data.SqlClient;

namespace SqlReflectTest.DataMappers
{
    class CustomerDataMapper : AbstractDataMapper
    {
        const string COLUMNS = "CustomerName, Email";
        const string SQL_GET_ALL = @"SELECT CustomerID, " + COLUMNS + " FROM Customers";
        const string SQL_GET_BY_ID = SQL_GET_ALL + " WHERE CustomerID=";
        const string SQL_INSERT = "INSERT INTO Customers (" + COLUMNS + ") OUTPUT INSERTED.CustomerID VALUES ";
        const string SQL_DELETE = "DELETE FROM Customers WHERE CustomerID = ";
        const string SQL_UPDATE = "UPDATE Customers SET CustomerName={1}, Email={2} WHERE CustomerID = {0}";

        public CustomerDataMapper(string connStr) : base(connStr)
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
            Customer c = new Customer
            {
                CustomerID = (int)dr["CustomerID"],
                CustomerName = (string)dr["CustomerName"],
                Email = (string)dr["Email"]
            };
            return c;
        }

        protected override string SqlInsert(object target)
        {
            Customer c = (Customer)target;
            string values = "'" + c.CustomerName + "' , '" + c.Email + "'";
            return SQL_INSERT + "(" + values + ")";
        }

        protected override string SqlUpdate(object target)
        {
            Customer c = (Customer)target;
            return String.Format(SQL_UPDATE,
                c.CustomerID,
                "'" + c.CustomerName + "'",
                "'" + c.Email + "'");
        }

        protected override string SqlDelete(object target)
        {
            Customer c = (Customer)target;
            return SQL_DELETE + c.CustomerID;
        }
    }
}
