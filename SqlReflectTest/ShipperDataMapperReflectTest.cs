using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflect;
using SqlReflectTest.Model;

namespace SqlReflectTest
{
    [TestClass]
    public class ShipperDataMapperReflectTest : AbstractShipperDataMapperTest
    {
        public ShipperDataMapperReflectTest() : base(new ReflectDataMapper(typeof(Shipper), NORTHWIND))
        {
        }

        [TestMethod]
        public void TestShipperGetAllReflect()
        {
            base.TestShipperGetAll();
        }

        [TestMethod]
        public void TestShipperGetByIdReflect()
        {
            base.TestShipperGetById();
        }
        [TestMethod]
        public void TestShipperInsertAndDeleteReflect()
        {
            base.TestShipperInsertAndDelete();
        }

        [TestMethod]
        public void TestShipperUpdateReflect()
        {
            base.TestShipperUpdate();
        }
    }
}
