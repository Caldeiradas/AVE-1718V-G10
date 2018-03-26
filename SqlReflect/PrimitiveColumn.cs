using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlReflect
{
    class PrimitiveColumn : AbstractColumn
    {
        public PrimitiveColumn(PropertyInfo Pi) : base(Pi){
            this.Name = Pi.Name;
        }
        public override object GetValue(SqlDataReader dr)
        {
            //get the value for this column from the SqlDataReader
            object value = dr[Name];
            if (value is DBNull)
                value = null;
            return value;
        }
        
    }
}
