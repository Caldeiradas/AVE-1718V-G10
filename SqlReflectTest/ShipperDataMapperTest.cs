using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflectTest.DataMappers;

namespace SqlReflectTest
{
    [TestClass]
    public class ShipperDataMapperTest : AbstractShipperDataMapperTest
    {
        public ShipperDataMapperTest() : base(new ShipperDataMapper(NORTHWIND))
        {
        }

        [TestMethod]
        public new void TestShipperGetAll()
        {
            base.TestShipperGetAll();
        }

        [TestMethod]
        public new void TestShipperGetById()
        {
            base.TestShipperGetById();
        }


        [TestMethod]
        public new void TestShipperInsertAndDelete()
        {
            base.TestShipperInsertAndDelete();
        }

        [TestMethod]
        public new void TestShipperUpdate()
        {
            base.TestShipperUpdate();
        }
    }
}
