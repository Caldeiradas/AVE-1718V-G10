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

        Type DomainObjectType;
        List<ReflectDataMapper> rdms = new List<ReflectDataMapper>();

        //Approach number 2:
        //Keep array of Properties
        //And an extra reference for the property that represents PRIMARY_KEY
        //iterate through properties to create the strings
        //iterate through properties in SqlInsert() to get respective values with .GetValue(target);
        PropertyInfo[] attributesOfDomainObject;
        PropertyInfo primaryKeyAttribute;

        protected static string ConnStr;


        public ReflectDataMapper(Type klass, string connStr) : base(connStr)
        {
            //most of the work will be done here
            //construction of the strings is done here
            //should it be done here or should it be done in the respective methods with an IF check to see if the string has been generated before?
            
            DomainObjectType = klass;
            ConnStr = connStr;
            
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
                Type propType = property.PropertyType;
                if (property.IsDefined(typeof(PKAttribute), false))
                {
                    PRIMARY_KEY_NAME = property.Name;
                    primaryKeyAttribute = property;
                }
                else if(propType.IsPrimitive || propType.Equals(typeof(string)))
                {
                    COLUMNS[i - 1] = property.Name;
                    //form COLUMN names
                    COLUMNS_FLAT += property.Name + ",";


                    SQL_UPDATE += property.Name + "={" + i + "},";
                    ++i;
                    //TODO the last one will have an extra ','
                }
                else
                {
                    //public ReflectDataMapper(Type klass, string connStr)
                    ReflectDataMapper rdm = new ReflectDataMapper(propType, connStr);
                    rdms.Add(rdm);
                    COLUMNS[i - 1] = property.Name+"ID";
                    COLUMNS_FLAT += property.Name+"ID" + ",";


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
            //Create an empty instance of the correct Type
            Object domainObject = Activator.CreateInstance(DomainObjectType);

            //iterate through the type's parameters
            for (int i = 0; i < attributesOfDomainObject.Length; ++i)
            {
                //get the type and check if it is primitive or a string
                Type propertyType = attributesOfDomainObject[i].PropertyType;

                //get the name of the attribute
                string propName = attributesOfDomainObject[i].Name;
                if (propertyType.IsPrimitive || propertyType.Equals(typeof(string)))
                {
                   
                    //get the value of the attribute
                    object value = dr[propName];
                    if (value is DBNull) value = null;
                    //set dObject's 'propName' property's value to 'value'
                    DomainObjectType.GetProperty(propName).SetValue(domainObject, value);
                }
                else
                {

                    //generate the foreign_key name of this attribute
                    string fkName = "";
                    PropertyInfo pi = attributesOfDomainObject[i];
                    if (pi.IsDefined(typeof(FKAttribute), false))
                    {
                        FKAttribute fk = (FKAttribute)pi.GetCustomAttribute(typeof(FKAttribute), false);
                        fkName = fk.Name;
                    }
                    else
                    {
                        //throw something
                        fkName = attributesOfDomainObject[i].Name + "ID";
                    }
                    
                    //get the correct instance by the ID
                    object foreignID = dr[fkName];


                    //create a new ReflectdataMapper for this type
                    ReflectDataMapper rdm = new ReflectDataMapper(propertyType, ConnStr);
                    
                    //get the object corresponding to this ID
                    object obj =  rdm.GetById(foreignID);

                    object instance = Activator.CreateInstance(propertyType);
                    DomainObjectType.GetProperty(propName).SetValue(domainObject, obj);


                }

            }
            return domainObject;
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
                //if this property is not a primary key then add it to 'values'
                if (!pi.IsDefined(typeof(PKAttribute), false)){

                    Type t = attributesOfDomainObject[i].PropertyType;
                    object value = attributesOfDomainObject[i].GetValue(target);
                    //if (t.IsPrimitive || t.Equals(typeof(string)))
                        values += "'" + value + "' ,";
                    //else
                        //if ToString is overriden this if else is not needed
                    //    values += "'" + value + "' ,";
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
