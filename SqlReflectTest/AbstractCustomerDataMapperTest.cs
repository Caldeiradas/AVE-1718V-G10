using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflectTest.Model;
using SqlReflect;
using System.Collections;

namespace SqlReflectTest
{
    public abstract class AbstractCustomerDataMapperTest
    {
        protected static readonly string NORTHWIND = @"
                    Server=(LocalDB)\MSSQLLocalDB;
                    Integrated Security=true;
                    AttachDbFileName=" +
                        Environment.CurrentDirectory +
                        "\\data\\NORTHWND.MDF";

        readonly IDataMapper customers;

        public AbstractCustomerDataMapperTest(IDataMapper customers)
        {
            this.customers = customers;
        }

        public void TestCustomerGetAll()  //TODO
        {
            IEnumerable res = customers.GetAll();
            int count = 0;
            foreach (object p in res)
            {
                Console.WriteLine(p);
                count++;
            }

            Assert.AreEqual(8, count);
        }
        public void TestCustomerGetById() //TODO
        {
            Customer c = (Customer)customers.GetById(3);
            Assert.AreEqual("John", c.CompanyName);
        }

        public void TestCustomerInsertAndDelete()  //TODO
        {
            //
            // Create and Insert new Customer
            // 
            Customer c = new Customer()
            {
                CompanyName = "Jack",
            };
            object id = customers.Insert(c);
            //
            // Get the new Customer object from database
            //
            Customer actual = (Customer)customers.GetById(id);
            Assert.AreEqual(c.CompanyName, actual.CompanyName);
            //
            // Delete the created Customer from database
            //
            customers.Delete(actual);
            object res = customers.GetById(id);
            actual = res != null ? (Customer)res : default(Customer);
            Assert.IsNull(actual.CompanyName);
        }

        public void TestCustomerUpdate()
        {
            Customer original = (Customer)customers.GetById(3);
            Customer modified = new Customer()
            {
                CustomerID = original.CustomerID,
                CompanyName = "Maria",
            };

            //update to modified
            customers.Update(modified);
            Customer actual = (Customer)customers.GetById(3);
            //check current = modified
            Assert.AreEqual(modified.CompanyName, actual.CompanyName);

            //update to original
            customers.Update(original);
            actual = (Customer)customers.GetById(3);
            //check current = original
            Assert.AreEqual("Maria", actual.CompanyName);
        }
    }
}
