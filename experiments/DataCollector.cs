using System;
using System.Data.SQLite;
using System.Data;

namespace DataCollector
{
    public class DataCollector
    {
        string tableName;
        SQLiteConnection connection;
        SQLiteTransaction transaction;
        int queryCnt;
        int queriesAmount;

        public DataCollector()
        {
        }

        public DataCollector(int queriesAmount, string tableName, string connectionString)
        {
            connection = new SQLiteConnection("data source=" + connectionString);
            connection.Open();

            this.tableName = tableName;

            if (isTableExist() == false)
            {
                createTable();
            }

            this.queriesAmount = queriesAmount;
        }

        bool isTableExist()
        {
            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
                command.Parameters.AddWithValue("@tableName", tableName);

                if (command.ExecuteScalar() != null)
                {
                    return true;
                }

                return false;
            }
        }

        void createTable()
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText = @"CREATE TABLE if not exists " + tableName + @" (
                    [id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                    [data] char(32) NOT NULL,
                    [time] char(23) NOT NULL
                    );";
                command.ExecuteNonQuery();
            }
        }

        void doTransaction()
        {
            if (queryCnt == 0)
            {
                transaction = connection.BeginTransaction();
                queryCnt++;
                return;
            }
            else if (queryCnt >= queriesAmount)
            {
                transaction.Commit();
                queryCnt = 0;
                doTransaction();
            }

            queryCnt++;
        }

        public void write(String rfid)
        {
            var comInsert = new SQLiteCommand("INSERT INTO " + tableName + @" (data, time) VALUES(@rfid, @time)", connection);
            comInsert.Parameters.AddWithValue("@rfid", rfid);
            comInsert.Parameters.AddWithValue("@time", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            doTransaction();
            comInsert.ExecuteNonQuery();
        }

        public void close()
        {
            if (queryCnt != 0)
            {
                transaction.Commit();
            }

            transaction.Dispose();

            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
}