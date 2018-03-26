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

        //SQL query strings
        readonly string SQL_GET_ALL;//E.g =    @"SELECT {0} FROM {1}"; //{0} = PRIMARY_KEY + COLUMNS //{1] = TABLE_NAME
        readonly string SQL_GET_BY_ID;//E.g  =  "{0} WHERE {1}=";//{0} = SQL_GET_ALL //{1} = PRIMARY_KEY
        readonly string SQL_INSERT;//E.g  =     "INSERT INTO {0} ({1}) OUTPUT INSERTED.{2} VALUES ";//{0} = TABLE_NAME //{1} = COLUMNS //{2} = PRIMARY_KEY
        readonly string SQL_DELETE;//E.g  =     "DELETE FROM {0} WHERE {1} = ";//{0} = TABLE_NAME //{1} = PRIMARY_KEY
        readonly string SQL_UPDATE;//E.g  =     "UPDATE {0} SET CategoryName={1}, Description={2} WHERE CategoryID = {0}";

        //Reflection information
        Type DomainObjectType;
        
        AbstractColumn[] ColumnsOfDomain;
        public AbstractColumn PrimaryKey;

        public ReflectDataMapper(Type klass, string connStr) : base(connStr)
        {
            DomainObjectType = klass;
            ConnStr = connStr;
            PropertyInfo[] DomainObjectProperties = klass.GetProperties();
            
            //Get TableAtribute from the custom atributes
            TableAttribute tb = (TableAttribute)klass.GetCustomAttribute(typeof(TableAttribute), false);
            string tableName = tb.Name;

            StringBuilder columnsSB = new StringBuilder();
            StringBuilder updateSB = new StringBuilder();
            String pkName = "";
            int i = 0;
            //-1 because the primary key is not here
            ColumnsOfDomain = new AbstractColumn[DomainObjectProperties.Length -1];
            

            //Iterate all proprieties to build the string for columns and for the update query
            foreach (PropertyInfo property in DomainObjectProperties)
            {
                //AbstractColumn currentColumn; 

                Type propType = property.PropertyType;
                if (property.IsDefined(typeof(PKAttribute), false))
                {
                    PrimaryKey = new PrimitiveColumn(property);
                    pkName = PrimaryKey.GetName();
                }
                else if(propType.IsPrimitive || propType.Equals(typeof(string)))
                {
                    PrimitiveColumn currentColumn = new PrimitiveColumn(property);
                    ColumnsOfDomain[i] = currentColumn;
                    ConstructSQLQuery(currentColumn.GetName(), ++i, columnsSB, updateSB);

                }
                else
                {
                    ReflectDataMapper rdm = new ReflectDataMapper(propType, connStr);
                    NotPrimitiveColumn currentColumn = new NotPrimitiveColumn(property, rdm);
                    ColumnsOfDomain[i] = currentColumn;
                    ConstructSQLQuery(currentColumn.GetFKName(), ++i, columnsSB, updateSB);
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
            foreach (AbstractColumn currentColumn in ColumnsOfDomain)
            {
                object value = currentColumn.GetValue(dr);
                DomainObjectType.
                    GetProperty(currentColumn.GetName()).
                    SetValue(domainObject, value);
            }
            object pkValue = PrimaryKey.GetValue(dr);
            DomainObjectType.GetProperty(PrimaryKey.GetName()).SetValue(domainObject, pkValue);
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
            foreach(AbstractColumn column in ColumnsOfDomain) 
                valueSB.Append("'").Append(column.GetPropertyValue(target)).Append("' ,");
            
            //remove last unnecessary comma ','
            if (valueSB[valueSB.Length - 1] == ',')
                    valueSB.Remove(valueSB.Length - 1, 1);
            return SQL_INSERT + "(" + valueSB.ToString() + ")";
        }

        protected override string SqlDelete(object target)
        {
            return SQL_DELETE + PrimaryKey.GetPropertyValue(target);
        }

        protected override string SqlUpdate(object target)
        {
            string[] valuesToFormatStringWith = new string[ColumnsOfDomain.Length+1];
            valuesToFormatStringWith[0] = "" + PrimaryKey.GetPropertyValue(target);
            for(int i = 1; i < valuesToFormatStringWith.Length; ++i)
            {
                valuesToFormatStringWith[i] = "'"+ ColumnsOfDomain[i-1].GetPropertyValue(target)+"'";
            }
            return String.Format(SQL_UPDATE, valuesToFormatStringWith);
        }

        private static void ConstructSQLQuery(string name, int index, StringBuilder columnsSB, StringBuilder updateSB)
        {
            columnsSB.Append(name).Append(",");
            updateSB.Append(name).Append("={").Append(index).Append("},");
        }
    }
}
