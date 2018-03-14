using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflectTest.DataMappers;

namespace SqlReflectTest
{
    [TestClass]
    public class CustomerDataMapperTest : AbstractCustomerDataMapperTest
    {
        public CustomerDataMapperTest() : base(new CustomerDataMapper(NORTHWIND))
        {
        }

        [TestMethod]
        public new void TestCustomerGetAll()
        {
            base.TestCustomerGetAll();
        }

        [TestMethod]
        public new void TestCustomerGetById()
        {
            base.TestCustomerGetById();
        }


        [TestMethod]
        public new void TestCustomerInsertAndDelete()
        {
            base.TestCustomerInsertAndDelete();
        }

        [TestMethod]
        public new void TestCustomerUpdate()
        {
            base.TestCustomerUpdate();
        }
    }
}
