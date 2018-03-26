using System;
using System.Data.SqlClient;
using System.Reflection;

namespace SqlReflect
{
    public abstract class AbstractColumn
    {

        public AbstractColumn(PropertyInfo Pi)
        {
            this.Pi = Pi;
        }
        protected PropertyInfo Pi;
        protected string Name;
        //returns the value of this column
        public abstract object GetValue(SqlDataReader dr);
        public string GetName()
        {
            return Name;
        }
        public object GetPropertyValue(object target)
        {
            return Pi.GetValue(target);
        }

    }
    
}
