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
        string PRIMARY_KEY_NAME;
        string COLUMNS;
        string SQL_GET_ALL;// =    @"SELECT {0} FROM {1}"; //{0} = PRIMARY_KEY + COLUMNS //{1] = TABLE_NAME
        string SQL_GET_BY_ID;// =  "{0} WHERE {1}=";//{0} = SQL_GET_ALL //{1} = PRIMARY_KEY
        string SQL_INSERT;// =     "INSERT INTO {0} ({1}) OUTPUT INSERTED.{2} VALUES ";//{0} = TABLE_NAME //{1} = COLUMNS //{2} = PRIMARY_KEY
        string SQL_DELETE;// =     "DELETE FROM {0} WHERE {1} = ";//{0} = TABLE_NAME //{1} = PRIMARY_KEY
        string SQL_UPDATE;// =     "UPDATE {0} SET CategoryName={1}, Description={2} WHERE CategoryID = {0}";


        int PRIMARY_KEY_VALUE;
        Type TypeOfObject;
        public ReflectDataMapper(Type klass, string connStr) : base(connStr)
        {
            //klass is a representation of an entity
            //has a PK and attributes
            TypeOfObject = klass;
            COLUMNS = "";
            
            //Get the TableAttribute
            TableAttribute tb = (TableAttribute) klass.GetCustomAttribute(typeof(TableAttribute), false);
            TABLE_NAME = tb.Name;
            

            SQL_UPDATE = "UPDATE " + TABLE_NAME + " SET "; //foreach (Attribute) SQL_UPDATE += attribute.name + "={i}"

            //Get all the properties of this type and construct the COLUMN attributes
            PropertyInfo[] properties = klass.GetProperties();
            int i = 1;
            foreach(PropertyInfo property in properties)
            {
                if (property.IsDefined(typeof(PKAttribute), false)){
                    PRIMARY_KEY_NAME = property.Name;
                    //TODO PRIMARY_KEY might not be an integer
                    //object o = property.GetGetMethod().Invoke(klass, new object[] { });
                    //PRIMARY_KEY = property.GetValue(klass);
                }
                else
                {
                    //form COLUMN names
                    COLUMNS += property.Name+",";

                    //TODO the last one will have an extra ','
                    SQL_UPDATE += property.Name + "={" + i + "},";
                    ++i;
                }
                

            }

            Console.WriteLine("PK = " + PRIMARY_KEY_NAME + "COLUMNS = " + COLUMNS);

            if (SQL_UPDATE[SQL_UPDATE.Length -1] == ',')
            {
                COLUMNS = COLUMNS.Remove(COLUMNS.Length - 1);
                SQL_UPDATE = SQL_UPDATE.Remove(SQL_UPDATE.Length - 1); //remove last unecessary ','
            }
            SQL_UPDATE += " WHERE " + PRIMARY_KEY_NAME + "={0}";
            //SQL_UPDATE is now in the form >UPDATE TABLE_NAME SET Column_1 ={1},..,Column_n ={n} WHERE PRIMARY_KEY = {0}<


            SQL_GET_ALL = "SELECT " + PRIMARY_KEY_NAME + ", " + COLUMNS + " FROM " + TABLE_NAME;
            SQL_GET_BY_ID = SQL_GET_ALL + " WHERE " + PRIMARY_KEY_NAME + " = ";
            SQL_INSERT = "INSERT INTO " + TABLE_NAME + "(" + COLUMNS + ")" + " OUTPUT INSERTED." + PRIMARY_KEY_NAME + " VALUES ";
            SQL_DELETE = "DELETE FROM " + TABLE_NAME + " WHERE " + PRIMARY_KEY_NAME + " = ";


        }

        protected override object Load(SqlDataReader dr)
        {
            return null;
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
