using SqlReflect.Attributes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace SqlReflect
{
    
    public class ReflectDataMapper : AbstractDataMapper
    {
        

        string TABLE_NAME;
        string PRIMARY_KEY_NAME;
        string COLUMNS_FLAT;
        string[] COLUMNS;
        string SQL_GET_ALL;// =    @"SELECT {0} FROM {1}"; //{0} = PRIMARY_KEY + COLUMNS //{1] = TABLE_NAME
        string SQL_GET_BY_ID;// =  "{0} WHERE {1}=";//{0} = SQL_GET_ALL //{1} = PRIMARY_KEY
        string SQL_INSERT;// =     "INSERT INTO {0} ({1}) OUTPUT INSERTED.{2} VALUES ";//{0} = TABLE_NAME //{1} = COLUMNS //{2} = PRIMARY_KEY
        string SQL_DELETE;// =     "DELETE FROM {0} WHERE {1} = ";//{0} = TABLE_NAME //{1} = PRIMARY_KEY
        string SQL_UPDATE;// =     "UPDATE {0} SET CategoryName={1}, Description={2} WHERE CategoryID = {0}";

        Type DomainObject;

        //Approach number 2:
        //Keep array of Properties
        //And an extra reference for the property that represents PRIMARY_KEY
        //iterate through properties to create the strings
        //iterate through properties in SqlInsert() to get respective values with .GetValue(target);
        PropertyInfo[] attributesOfDomainObject;
        PropertyInfo primaryKeyAttribute;

        public ReflectDataMapper(Type klass, string connStr) : base(connStr)
        {
            //most of the work will be done here
            //construction of the strings is done here
            //should it be done here or should it be done in the respective methods with an IF check to see if the string has been generated before?
            
            DomainObject = klass;
            
            //Get the TableAttribute
            TableAttribute tb = (TableAttribute) klass.GetCustomAttribute(typeof(TableAttribute), false);
            TABLE_NAME = tb.Name;
            
            SQL_UPDATE = "UPDATE " + TABLE_NAME + " SET ";

            //Get all the properties of this type and construct the COLUMN attributes
            PropertyInfo[] properties = klass.GetProperties();
            attributesOfDomainObject = properties;

            //array of COLUMN_NAMES
            COLUMNS = new string[properties.Length];
            //concatenation of COLUMN_NAMES
            COLUMNS_FLAT = "";

            //replace with fori
            int i = 1;
            foreach(PropertyInfo property in properties)
            {
                if (property.IsDefined(typeof(PKAttribute), false)){
                    PRIMARY_KEY_NAME = property.Name;
                    primaryKeyAttribute = property;
                }
                else
                {
                    COLUMNS[i - 1] = property.Name;
                    //form COLUMN names
                    COLUMNS_FLAT += property.Name+",";

                    
                    SQL_UPDATE += property.Name + "={" + i + "},";
                    ++i;
                    //TODO the last one will have an extra ','
                }


            }

            Console.WriteLine("PK = " + PRIMARY_KEY_NAME + "COLUMNS = " + COLUMNS);

            if (SQL_UPDATE[SQL_UPDATE.Length -1] == ',')
            {
                //remove last unnecessary comma ','
                COLUMNS_FLAT = COLUMNS_FLAT.Remove(COLUMNS_FLAT.Length - 1);
                SQL_UPDATE = SQL_UPDATE.Remove(SQL_UPDATE.Length - 1); 
            }
            SQL_UPDATE += " WHERE " + PRIMARY_KEY_NAME + "={0}";
            //SQL_UPDATE is now in the form >UPDATE TABLE_NAME SET Column_1 ={1},..,Column_n ={n} WHERE PRIMARY_KEY = {0}<


            SQL_GET_ALL = "SELECT " + PRIMARY_KEY_NAME + ", " + COLUMNS_FLAT + " FROM " + TABLE_NAME;
            SQL_GET_BY_ID = SQL_GET_ALL + " WHERE " + PRIMARY_KEY_NAME + " = ";
            SQL_INSERT = "INSERT INTO " + TABLE_NAME + "(" + COLUMNS_FLAT + ")" + " OUTPUT INSERTED." + PRIMARY_KEY_NAME + " VALUES ";
            SQL_DELETE = "DELETE FROM " + TABLE_NAME + " WHERE " + PRIMARY_KEY_NAME + " = ";


        }

        protected override object Load(SqlDataReader dr)
        {


            /*
            object[] parameters = new object[COLUMNS.Length+1];
            
            parameters[0] = dr[PRIMARY_KEY_NAME];
            for(int i = 1; i < parameters.Length; ++i)
            {
                parameters[i] = dr[COLUMNS[i-1]];
            }
            return Activator.CreateInstance(DomainObject, parameters);

            //first parameter might not be the PRIMARY_KEY
            */

            //object array that will contain the parameters that the constructor will receive
            //e.g if the object is a Category, parameters will be = {dr["CategoryID"], dr["CategoryName"], dr["Description"]}
            object[] parameters = new object[attributesOfDomainObject.Length];

            for(int i = 0; i < parameters.Length; ++i)
                parameters[i] = dr[attributesOfDomainObject[i].Name];

            //Type dObject = (Type)FormatterServices.GetUninitializedObject(DomainObject); //does not call ctor
            Object dObject = Activator.CreateInstance(DomainObject);
            for (int i = 0; i < attributesOfDomainObject.Length; ++i)
            {
                DomainObject.GetProperty(attributesOfDomainObject[i].Name).SetValue(dObject, parameters[i]);
            }
            return dObject;
            //return Activator.CreateInstance(DomainObject, parameters);
        }

        protected override string SqlGetAll()
        {
            return SQL_GET_ALL;
        }

        protected override string SqlGetById(object id)
        {
            return SQL_GET_BY_ID + id;
        }

        protected override string SqlInsert(object target)
        {
            string values = "";
            //iterate through target's Properties
            for(int i = 0; i < attributesOfDomainObject.Length; ++i)
            {
                PropertyInfo pi = attributesOfDomainObject[i];
                if (!pi.IsDefined(typeof(PKAttribute), false)){
                    values += "'" + attributesOfDomainObject[i].GetValue(target) + "' ,";
                }
                
            }
            if (values[values.Length - 1] == ',')
                //remove last unnecessary comma ','
                values = values.Remove(values.Length - 1);
            return SQL_INSERT + "(" + values + ")";
        }

        protected override string SqlDelete(object target)
        {
            return 
                SQL_DELETE + 
                //ID of target
                primaryKeyAttribute.GetValue(target);
        }

        protected override string SqlUpdate(object target)
        {
            string[] valuesToFormatStringWith = new string[attributesOfDomainObject.Length];
            for(int i = 0; i < valuesToFormatStringWith.Length; ++i)
            {
                valuesToFormatStringWith[i] = "'"+attributesOfDomainObject[i].GetValue(target)+"'";
            }
            return String.Format(SQL_UPDATE, valuesToFormatStringWith);
        }
    }
}
