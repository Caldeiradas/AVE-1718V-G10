using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlReflectTest.Model;
using SqlReflect;
using System.Collections;

namespace SqlReflectTest
{
    public abstract class AbstractRegionDataMapperTest
    {
        protected static readonly string NORTHWIND = @"
                    Server=(LocalDB)\MSSQLLocalDB;
                    Integrated Security=true;
                    AttachDbFileName=" +
                        Environment.CurrentDirectory +
                        "\\data\\NORTHWND.MDF";

        readonly IDataMapper regions;

        public AbstractRegionDataMapperTest(IDataMapper regions)
        {
            this.regions = regions;
        }

        public void TestRegionGetAll()
        {
            IEnumerable res = regions.GetAll();
            int count = 0;
            foreach (object p in res)
            {
                Console.WriteLine(p);
                count++;
            }
            //there are 4 regions
            Assert.AreEqual(4, count);
        }
        public void TestRegionGetById()
        {
            Region c = (Region)regions.GetById(3);
            //Region strings have lots of white spaces that dont show up in the debug menu
            Assert.AreEqual("Southern                                          ", c.RegionDescription);

        }

        public void TestRegionInsertAndDelete()
        {
            //
            // Create and Insert new Region
            // 
            Region c = new Region()
            {
                RegionID = 42,
                RegionDescription = "Northern"
            };
            object id = regions.Insert(c);
            //
            // Get the new category object from database
            //
            Region actual = (Region)regions.GetById(id);
            Assert.AreEqual(c.RegionDescription, actual.RegionDescription);
            //
            // Delete the created category from database
            //
            regions.Delete(actual);
            object res = regions.GetById(id);
            actual = res != null ? (Region)res : default(Region);
            Assert.IsNull(actual.RegionDescription);
        }

        public void TestRegionUpdate()
        {
            Region original = (Region)regions.GetById(3);
            Region modified = new Region()
            {
                RegionID = original.RegionID,
                RegionDescription = "Southern"
            };
            regions.Update(modified);
            Region actual = (Region)regions.GetById(3);
            Assert.AreEqual(modified.RegionDescription, actual.RegionDescription);
            regions.Update(original);
            actual = (Region)regions.GetById(3);
            Assert.AreEqual("Southern", actual.RegionDescription);
        }
    }
}
