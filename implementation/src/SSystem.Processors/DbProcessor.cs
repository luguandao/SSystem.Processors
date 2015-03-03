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
        private DbProviderFactory m_CacheFactory;
        public int ExecuteTimeoutBySecond { get; set; }

        protected DbProcessor()
        {
            ExecuteTimeoutBySecond = 30;
        }

        public virtual int ExecuteNonQuery(IDbCommand icom)
        {
            if (icom.Connection == null)
            {
                icom.Connection = CreateConnection();
            }
            icom.CommandTimeout = ExecuteTimeoutBySecond;
            return icom.ExecuteNonQuery();
        }

        public virtual object ExecuteScalar(IDbCommand icom)
        {
            if (icom.Connection == null)
            {
                icom.Connection = CreateConnection();
            }
            icom.CommandTimeout = ExecuteTimeoutBySecond;
            return icom.ExecuteScalar();
        }

        public virtual DataSet QueryDataSet(IDbCommand icom)
        {
            if (icom.Connection == null)
            {
                icom.Connection = CreateConnection();
            }
            icom.CommandTimeout = ExecuteTimeoutBySecond;
            string[] arr = icom.Connection.GetType()
                .FullName.Split(new char[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            string providerName = string.Join(".", arr, 0, arr.Length - 1);

            return QueryDataSet(icom, providerName);

        }

        public virtual DataSet QueryDataSet(IDbCommand icom, string providerName)
        {
            if (icom.Connection == null)
            {
                icom.Connection = CreateConnection();
            }
            icom.CommandTimeout = ExecuteTimeoutBySecond;

            DbProviderFactory dbfactory = CreateDbProviderFactory(icom.Connection.ConnectionString, providerName);
            DbDataAdapter dbd = null;
            try
            {
                dbd = QueryDbDataAdapter(icom, dbfactory) as DbDataAdapter;
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

        public virtual IDataReader QueryDataReader(IDbCommand iCom)
        {
            if (iCom.Connection == null)
            {
                iCom.Connection = CreateConnection();
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

        public virtual DbProviderFactory CreateDbProviderFactory(string connectionString, string providerName)
        {
            m_CacheFactory = m_CacheFactory ?? DbProviderFactories.GetFactory(providerName);

            return m_CacheFactory;
        }

        public virtual IDbDataAdapter QueryDbDataAdapter(IDbCommand iCom, DbProviderFactory df)
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

        public virtual IDbCommand CreateDbCommand()
        {
            IDbConnection icon = CreateConnection();
            if (icon != null)
                return icon.CreateCommand();
            throw new NotImplementedException("请实现CreateConnection方法");

        }

        public abstract IDbConnection CreateConnection();

        public virtual IDbConnection CreateConnection(string connectionString, string providerName)
        {
            var dbProvider = CreateDbProviderFactory(connectionString, providerName);
            if (dbProvider == null)
                throw new Exception("无法生成DbProviderFactory");
            return dbProvider.CreateConnection();
        }

    }
}
