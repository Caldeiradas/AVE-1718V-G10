using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlReflect
{
    class NotPrimitiveColumn : AbstractColumn
    {
        ReflectDataMapper notPrimitiveValue;
        public NotPrimitiveColumn(PropertyInfo Pi, ReflectDataMapper rdm) : base(Pi)
        {
            notPrimitiveValue = rdm;
            this.Name = Pi.Name;
        }
        public override object GetValue( SqlDataReader dr)
        {
            return notPrimitiveValue.GetById(dr[GetFKName()]);
        }
        public string GetFKName()
        {
            return notPrimitiveValue.PrimaryKey.GetName();
        }
        
    }
}
