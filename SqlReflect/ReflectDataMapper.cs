using SqlReflect.Attributes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace SqlReflect
{
    
    public class ReflectDataMapper : AbstractDataMapper
    {
        string TABLE_NAME;
        int PRIMARY_KEY;
        string COLUMNS;
        string SQL_GET_ALL;// = @"SELECT CategoryID, " + COLUMNS + " FROM Categories";
        string SQL_GET_BY_ID;// = SQL_GET_ALL + " WHERE CategoryID=";
        string SQL_INSERT;// = "INSERT INTO Categories (" + COLUMNS + ") OUTPUT INSERTED.CategoryID VALUES ";
        string SQL_DELETE;// = "DELETE FROM Categories WHERE CategoryID = ";
        string SQL_UPDATE;// = "UPDATE Categories SET CategoryName={1}, Description={2} WHERE CategoryID = {0}";
        

        public ReflectDataMapper(Type klass, string connStr) : base(connStr)
        {
            //klass is a representation of an entity
            //has a PK and attributes

            COLUMNS = "";

            TableAttribute tb = (TableAttribute) klass.GetCustomAttribute(typeof(TableAttribute), false);
            TABLE_NAME = tb.Name;

           
            PropertyInfo[] properties = klass.GetProperties();

            foreach(PropertyInfo property in properties)
            {
                if (property.IsDefined(typeof(PKAttribute), false)){
                    PRIMARY_KEY = (int) property.GetValue(klass);
                }
                //form COLUMN names
                COLUMNS += property.Name;

            }

            Console.WriteLine("PK = " + PRIMARY_KEY + "COLUMNS = " + COLUMNS);

        }

        protected override object Load(SqlDataReader dr)
        {
            throw new NotImplementedException();
        }

        protected override string SqlGetAll()
        {
            throw new NotImplementedException();
        }

        protected override string SqlGetById(object id)
        {
            throw new NotImplementedException();
        }

        protected override string SqlInsert(object target)
        {
            throw new NotImplementedException();
        }

        protected override string SqlDelete(object target)
        {
            throw new NotImplementedException();
        }

        protected override string SqlUpdate(object target)
        {
            throw new NotImplementedException();
        }
    }
}
