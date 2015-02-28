using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SSystem.Processors
{
    public interface IDbOperator
    {
        int ExecuteNonQuery(IDbCommand icom);
        object ExecuteScalar(IDbCommand icom);
        DataSet GetDataSet(IDbCommand icom);

        IDataReader GetDataReader(IDbCommand icom);
    }
}
