using System;
using System.Data.SQLite;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;

namespace rfid
{
    public class RfidTagsCollector
    {
        SQLiteConnection connection;

        public RfidTagsCollector(string connectionString)
        {
            connectionString = "data source=" + connectionString;

            //SQLite normally creates a new database without throwing any exeption.
            //So, if one wants to know whether a database exists or not, one should add the 'FailIfMissing=True' option.
            try
            {
                connection = new SQLiteConnection(connectionString + "; FailIfMissing=True");
                connection.Open();
            }
            catch (SQLiteException e)
            {
                Trace.WriteLine(DateTime.Now.ToString() + "\tThe database wasn't found. A new one was created.");
                connection = new SQLiteConnection(connectionString);
                connection.Open();
            }
            CreateTables();
        }

        void CreateTables()
        {
            using (SQLiteCommand cmd = new SQLiteCommand(connection))
            {
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS rfid_tags (
                    [session_id] integer NOT NULL,
                    [tag] char(32) NOT NULL,
                    PRIMARY KEY(session_id, tag)
                    );";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS sessions (
                    [id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                    [time_marker] char(23) NOT NULL,
                    [location_id] char(32) NOT NULL,
                    [status] integer NOT NULL  
                    );";
                cmd.ExecuteNonQuery();
            }
        }

        public void Write(RfidSession session)
        {
            var transaction = connection.BeginTransaction();

            //Register a new session
            var cmd = new SQLiteCommand("INSERT INTO sessions (time_marker, location_id, status) VALUES(@time_marker, @location_id, @status)", connection);
            cmd.Parameters.AddWithValue("@time_marker", session.time);
            cmd.Parameters.AddWithValue("@location_id", session.location);
            cmd.Parameters.AddWithValue("@status", session.status);
            cmd.ExecuteNonQuery();

            //Look up the last session id
            cmd.CommandText = "SELECT last_insert_rowid()";
            var sessionId = Convert.ToInt32(cmd.ExecuteScalar());

            //Prepare for INSERT
            cmd = new SQLiteCommand("INSERT INTO rfid_tags (session_id, tag) VALUES(@session_id, @tag)", connection);
            cmd.Parameters.AddWithValue("@session_id", sessionId);
            SQLiteParameter tag_ = new SQLiteParameter("@tag");
            cmd.Parameters.Add(tag_);

            //Add new tags
            foreach (string tag in session.tags)
            {
                tag_.Value = tag;
                cmd.ExecuteNonQuery();
            }
            transaction.Commit();
        }

        public void Close()
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }
}