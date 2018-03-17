using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflectTest.Model;
using SqlReflect;
using System.Collections;

namespace SqlReflectTest
{
    public abstract class AbstractShipperDataMapperTest
    {
        protected static readonly string NORTHWIND = @"
                    Server=(LocalDB)\MSSQLLocalDB;
                    Integrated Security=true;
                    AttachDbFileName=" +
                        Environment.CurrentDirectory +
                        "\\data\\NORTHWND.MDF";

        readonly IDataMapper shippers;

        public AbstractShipperDataMapperTest(IDataMapper shippers)
        {
            this.shippers = shippers;
        }

        public void TestShipperGetAll()
        {
            IEnumerable res = shippers.GetAll();
            int count = 0;
            foreach (object p in res)
            {
                Console.WriteLine(p);
                count++;
            }
            Assert.AreEqual(3, count);
        }
        public void TestShipperGetById()
        {
            Shipper c = (Shipper)shippers.GetById(3);
            Assert.AreEqual("Federal Shipping", c.CompanyName);
            Assert.AreEqual("(503) 555-9931", c.Phone);
        }

        public void TestShipperInsertAndDelete()
        {
            //
            // Create and Insert new Shipper
            // 
            Shipper c = new Shipper()
            {
                CompanyName = "New Company",
                Phone = "(555) 123-4567"
            };
            object id = shippers.Insert(c);
            //
            // Get the new category object from database
            //
            Shipper actual = (Shipper)shippers.GetById(id);
            Assert.AreEqual(c.CompanyName, actual.CompanyName);
            Assert.AreEqual(c.Phone, actual.Phone);
            //
            // Delete the created category from database
            //
            shippers.Delete(actual);
            object res = shippers.GetById(id);
            actual = res != null ? (Shipper)res : default(Shipper);
            Assert.IsNull(actual.CompanyName);
            Assert.IsNull(actual.Phone);
        }

        public void TestShipperUpdate()
        {
            Shipper original = (Shipper)shippers.GetById(3);
            Shipper modified = new Shipper()
            {
                ShipperID = original.ShipperID,
                CompanyName = "New Company",
                Phone = "(555) 123-4567"
            };
            shippers.Update(modified);
            Shipper actual = (Shipper)shippers.GetById(3);
            Assert.AreEqual(modified.CompanyName, actual.CompanyName);
            Assert.AreEqual(modified.Phone, actual.Phone);
            shippers.Update(original);
            actual = (Shipper)shippers.GetById(3);
            Assert.AreEqual("Federal Shipping", actual.CompanyName);
            Assert.AreEqual("(503) 555-9931", actual.Phone);
        }
    }
}
