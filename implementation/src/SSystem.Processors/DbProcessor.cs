using System;
using System.Data;
using System.Data.Common;


namespace SSystem.Processors
{
    /// <summary>
    /// 支持带数据库操作的处理器
    /// </summary>
    public abstract class DbProcessor : Processor
    {
        private DbProviderFactory m_cacheFactory;
        public int ExecuteTimeoutBySecond { get; set; }

        public int ExecuteNonQuery(IDbCommand icom)
        {
            if (icom.Connection == null)
            {
                icom.Connection = GetConnection();
            }
            icom.CommandTimeout = ExecuteTimeoutBySecond;
            return icom.ExecuteNonQuery();
        }

        public object ExecuteScalar(IDbCommand icom)
        {
            if (icom.Connection == null)
            {
                icom.Connection = GetConnection();
            }
            icom.CommandTimeout = ExecuteTimeoutBySecond;
            return icom.ExecuteScalar();
        }

        public DataSet GetDataSet(IDbCommand icom, string providerName)
        {
            if (icom.Connection == null)
            {
                icom.Connection = GetConnection();
            }
            icom.CommandTimeout = ExecuteTimeoutBySecond;

            DbProviderFactory dbfactory = GetDbProviderFactory(icom.Connection.ConnectionString, providerName);
            DbDataAdapter dbd = null;
            try
            {
                dbd = GetDbDataAdapter(icom, dbfactory) as DbDataAdapter;
            }
            catch
            {
                dbd = dbfactory.CreateDataAdapter();
            }

            if (dbd == null)
            {
                throw new NullReferenceException("无法生成DataAdapter");
            }
            dbd.SelectCommand = icom as DbCommand;
            DataSet ds = new DataSet();

            dbd.FillSchema(ds, SchemaType.Source);
            dbd.Fill(ds);
            return ds;
        }

        public IDataReader GetDataReader(IDbCommand iCom)
        {
            if (iCom.Connection == null)
            {
                iCom.Connection = GetConnection();
            }
            IDbConnection iCon = iCom.Connection;
            bool manualopen = false;

            if (iCon.State == ConnectionState.Closed)
            {
                iCon.Open();
                manualopen = true;
            }

            try
            {
                IDataReader read = null;
                read = iCom.ExecuteReader(CommandBehavior.CloseConnection);
                return read;
            }
            catch
            {

                if (manualopen)
                {
                    iCon.Close();
                }
                throw;
            }
        }

        public DbProviderFactory GetDbProviderFactory(string connectionString, string providerName)
        {
            m_cacheFactory = m_cacheFactory ?? DbProviderFactories.GetFactory(providerName);

            return m_cacheFactory;
        }

        public IDbDataAdapter GetDbDataAdapter(IDbCommand iCom, DbProviderFactory df)
        {
            DbDataAdapter da = df.CreateDataAdapter();
            da.SelectCommand = iCom as DbCommand;
            DbCommandBuilder builder = df.CreateCommandBuilder();
            builder.DataAdapter = da;

            da.InsertCommand = builder.GetInsertCommand();

            da.UpdateCommand = builder.GetUpdateCommand();

            da.DeleteCommand = builder.GetDeleteCommand();

            da.SelectCommand = iCom as DbCommand;

            return da as IDbDataAdapter;

        }

        public IDbConnection GetConnection()
        {
            throw new NotImplementedException();
        }
    }
}
