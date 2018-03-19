using SqlReflect.Attributes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Runtime.Serialization;

namespace SqlReflect
{
    
    public class ReflectDataMapper : AbstractDataMapper
    {
        //Information to connect to database 
        protected static string ConnStr;

        //SQL querie strings
        readonly string SQL_GET_ALL;//E.g =    @"SELECT {0} FROM {1}"; //{0} = PRIMARY_KEY + COLUMNS //{1] = TABLE_NAME
        readonly string SQL_GET_BY_ID;//E.g  =  "{0} WHERE {1}=";//{0} = SQL_GET_ALL //{1} = PRIMARY_KEY
        readonly string SQL_INSERT;//E.g  =     "INSERT INTO {0} ({1}) OUTPUT INSERTED.{2} VALUES ";//{0} = TABLE_NAME //{1} = COLUMNS //{2} = PRIMARY_KEY
        readonly string SQL_DELETE;//E.g  =     "DELETE FROM {0} WHERE {1} = ";//{0} = TABLE_NAME //{1} = PRIMARY_KEY
        readonly string SQL_UPDATE;//E.g  =     "UPDATE {0} SET CategoryName={1}, Description={2} WHERE CategoryID = {0}";

        //Reflection information
        Type DomainObjectType;
        PropertyInfo[] DomainObjectProperties;
        PropertyInfo PK_ProprietyInfo;
        List<ReflectDataMapper> DataMappers = new List<ReflectDataMapper>();
        

        public ReflectDataMapper(Type klass, string connStr) : base(connStr)
        {
            DomainObjectType = klass;
            ConnStr = connStr;
            DomainObjectProperties = klass.GetProperties();
            
            //Get TableAtribute from the custom atributes
            TableAttribute tb = (TableAttribute)klass.GetCustomAttribute(typeof(TableAttribute), false);
            string tableName = tb.Name;

            StringBuilder columnsSB = new StringBuilder();
            StringBuilder updateSB = new StringBuilder();
            String pkName = "";
            int i = 0;

            //Iterate all proprieties to build the string for columns and for the update querie
            foreach (PropertyInfo property in DomainObjectProperties)
            {
                Type propType = property.PropertyType;
                if (property.IsDefined(typeof(PKAttribute), false))
                {
                    PK_ProprietyInfo = property;
                    pkName = property.Name;
                }
                else if(propType.IsPrimitive || propType.Equals(typeof(string)))
                {
                    columnsSB.Append(property.Name).Append(",");
                    updateSB.Append(property.Name).Append("={").Append(++i).Append("},");
                }
                else
                {
                    ReflectDataMapper rdm = new ReflectDataMapper(propType, connStr);
                    DataMappers.Add(rdm);
                    string auxPKName = rdm.PK_ProprietyInfo.Name;
                    columnsSB.Append(auxPKName).Append(",");
                    updateSB.Append(auxPKName).Append("={").Append(++i).Append("},");
                }
            }

            //remove last unnecessary comma ','
            if (columnsSB[columnsSB.Length - 1] == ',')
            {
                columnsSB.Remove(columnsSB.Length - 1,1);
                updateSB.Remove(updateSB.Length - 1, 1);
            }

            //Build all SQL queries Strings
            SQL_GET_ALL = "SELECT " + pkName + "," + columnsSB.ToString() + " FROM " + tableName;
            SQL_GET_BY_ID = SQL_GET_ALL + " WHERE " + pkName + " = ";
            SQL_INSERT = "INSERT INTO " + tableName + "(" + columnsSB.ToString() + ")" + " OUTPUT INSERTED." + pkName + " VALUES ";
            SQL_DELETE = "DELETE FROM " + tableName + " WHERE " + pkName + " = ";
            SQL_UPDATE = "UPDATE " + tableName + " SET " + updateSB.ToString() + " WHERE " + pkName + "={0}";
        }

        protected override object Load(SqlDataReader dr)
        {
            Object domainObject = Activator.CreateInstance(DomainObjectType);

            //iterate through the type's parameters
            foreach (PropertyInfo property in DomainObjectProperties)
            {
                Type propType = property.PropertyType;
                string propName = property.Name;
                object value = null;
                if (propType.IsPrimitive || propType.Equals(typeof(string)))
                {
                    value = dr[propName];
                    if (value is DBNull)
                        value = null;
                }
                else
                {
                    foreach (ReflectDataMapper rdm in DataMappers)
                    {
                        if (propType.Equals(rdm.DomainObjectType))
                        {
                            value = rdm.GetById(dr[rdm.PK_ProprietyInfo.Name]);
                            break;
                        }
                    }
                }
                DomainObjectType.GetProperty(propName).SetValue(domainObject, value);
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
            StringBuilder valueSB = new StringBuilder();
            //iterate through target's Properties
            for(int i = 0; i < DomainObjectProperties.Length; ++i)
            {
                PropertyInfo pi = DomainObjectProperties[i];
                //if this property is not a primary key then add it to 'values'
                if (!pi.IsDefined(typeof(PKAttribute), false)){
                    valueSB.Append("'").Append(DomainObjectProperties[i].GetValue(target)).Append("' ,");
                }  
            }
            //remove last unnecessary comma ','
            if (valueSB[valueSB.Length - 1] == ',')
                    valueSB.Remove(valueSB.Length - 1, 1);
            return SQL_INSERT + "(" + valueSB.ToString() + ")";
        }

        protected override string SqlDelete(object target)
        {
            return SQL_DELETE + PK_ProprietyInfo.GetValue(target);
        }

        protected override string SqlUpdate(object target)
        {
            string[] valuesToFormatStringWith = new string[DomainObjectProperties.Length];
            for(int i = 0; i < valuesToFormatStringWith.Length; ++i)
            {
                valuesToFormatStringWith[i] = "'"+ DomainObjectProperties[i].GetValue(target)+"'";
            }
            return String.Format(SQL_UPDATE, valuesToFormatStringWith);
        }
    }
}
