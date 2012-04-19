﻿using System;
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

        public List<RfidSession> GetUnshippedTags()
        {
            var sessions = new List<RfidSession>();

            var sessionCmd = new SQLiteCommand(@"SELECT * from reading_sessions where delivery_status <> " + (int)RfidSession.DeliveryStatus.Shipped, connection);

            using (var sessionReader = sessionCmd.ExecuteReader())
            {
                while (sessionReader.Read())
                {
                    var session = new RfidSession()
                        {
                        id = sessionReader.GetInt32(0),
                        time = sessionReader.GetString(1),
                        location = sessionReader.GetString(2),
                        deliveryStatus = (RfidSession.DeliveryStatus)sessionReader.GetInt32(3),
                        readingStatus = (RfidSession.ReadingStatus)sessionReader.GetInt32(4)
                        };

                    var tagCmd = new SQLiteCommand(@"SELECT * from tubes where session_id = " + session.id, connection);
                    using (var tagReader = tagCmd.ExecuteReader())
                    {
                        while (tagReader.Read())
                        {
                            session.tags.Add(tagReader.GetString(1));
                        }
                    }
                    sessions.Add(session);
                }
            }
            return sessions;
        }

        void CreateTables()
        {
            using (var cmd = new SQLiteCommand(connection))
            {
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS tubes (
                    [session_id] integer NOT NULL,
                    [tag] char(32) NOT NULL,
                    PRIMARY KEY(session_id, tag)
                    );";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS reading_sessions (
                    [id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
                    [time_marker] char(23) NOT NULL,
                    [location_id] char(32) DEFAULT NULL,
                    [delivery_status] integer NOT NULL,
                    [reading_status] integer NOT NULL
                    );";
                cmd.ExecuteNonQuery();
            }
        }

        public void Write(RfidSession session)
        {
            var transaction = connection.BeginTransaction();

            //Register a new session
            var cmd = new SQLiteCommand("INSERT INTO reading_sessions (time_marker, location_id, delivery_status, reading_status) VALUES(@time_marker, @location_id, @delivery_status, @reading_status)", connection);
            cmd.Parameters.AddWithValue("@time_marker", session.time);
            cmd.Parameters.AddWithValue("@location_id", session.location);
            cmd.Parameters.AddWithValue("@delivery_status", session.deliveryStatus);
            cmd.Parameters.AddWithValue("@reading_status", session.readingStatus);
            cmd.ExecuteNonQuery();

            //Look up the last session id
            cmd.CommandText = "SELECT last_insert_rowid()";
            var sessionId = Convert.ToInt32(cmd.ExecuteScalar());

            //Prepare for INSERT
            cmd = new SQLiteCommand("INSERT INTO tubes (session_id, tag) VALUES(@session_id, @tag)", connection);
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

        public void SetDeliveryStatus(List<RfidSession> sessions)
        {
            foreach (var session in sessions)
            {
                var cmd = new SQLiteCommand("UPDATE reading_sessions SET delivery_status = @delivery_status where id = @id", connection);
                cmd.Parameters.AddWithValue("@delivery_status", (int)session.deliveryStatus);
                cmd.Parameters.AddWithValue("@id", session.id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}