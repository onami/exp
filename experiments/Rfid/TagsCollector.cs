using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace DL6970.Rfid
{
    /// <summary>
    /// Класс записи и чтения с sqlite-базы
    /// </summary>
    public class RfidTagsCollector
    {
        private readonly SQLiteConnection connection;

        public RfidTagsCollector(string connectionString)
        {
            connectionString = "data source=" + connectionString;

            //SQLite normally creates a new database without throwing any exeption.
            //So, if one wants to know whether a database exists or not,
            //he should add the 'FailIfMissing=True' option.
            connection = new SQLiteConnection(connectionString);
            connection.Open();
            CreateTables();
        }

        /// <summary>
        /// Возвращает список неотправленных на сервер сессий чтения
        /// </summary>
        public List<RfidSession> GetUnshippedTags()
        {
            var sessions = new List<RfidSession>();
            var sessionCmd = new SQLiteCommand(@"SELECT * from reading_sessions where delivery_status <> " + (int)RfidSession.DeliveryStatus.Shipped, connection);

            using (var sessionReader = sessionCmd.ExecuteReader())
            {
                while (sessionReader.Read())
                {
                    var session = new RfidSession
                        {
                            id = sessionReader.GetInt32(0),
                            time = sessionReader.GetString(1),
                            location = sessionReader.GetString(2),
                            deliveryStatus = (RfidSession.DeliveryStatus)sessionReader.GetInt32(3),
                            readingStatus = (RfidSession.ReadingStatus)sessionReader.GetInt32(4),
                            sessionMode = (RfidSession.SessionMode)sessionReader.GetInt32(5)
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

        /// <summary>
        /// Иниализация новой базы
        /// </summary>
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
                    [reading_status] integer NOT NULL,
                    [reading_mode] integer NOT NULL
                    );";
                cmd.ExecuteNonQuery();
            }
        }

        public void Write(RfidSession session)
        {
            if (session.tags.Count == 0)
                return;

            var transaction = connection.BeginTransaction();

            //Register a new session
            var cmd = new SQLiteCommand(@"
            INSERT INTO reading_sessions (time_marker,  location_id,  delivery_status,  reading_status,  reading_mode)
                                  VALUES(@time_marker, @location_id, @delivery_status, @reading_status, @reading_mode)", connection);
            cmd.Parameters.AddWithValue("@time_marker", session.time);
            cmd.Parameters.AddWithValue("@location_id", session.location);
            cmd.Parameters.AddWithValue("@delivery_status", session.deliveryStatus);
            cmd.Parameters.AddWithValue("@reading_status", session.readingStatus);
            cmd.Parameters.AddWithValue("@reading_mode", session.sessionMode);
            cmd.ExecuteNonQuery();

            //Look up the last session id
            cmd.CommandText = "SELECT last_insert_rowid()";
            var sessionId = Convert.ToInt32(cmd.ExecuteScalar());

            //Prepare for INSERT
            cmd = new SQLiteCommand("INSERT INTO tubes (session_id, tag) VALUES(@session_id, @tag)", connection);
            cmd.Parameters.AddWithValue("@session_id", sessionId);
            var tag_ = new SQLiteParameter("@tag");
            cmd.Parameters.Add(tag_);

            //Add new tags
            foreach (var tag in session.tags)
            {
                tag_.Value = tag;
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        /// <summary>
        /// Закрытие соединения
        /// </summary>
        /// <remarks>По каким-то причинам в Win CE закрытие не работает в деструкторе,
        /// поэтому был вынес его в этот метод</remarks>
        public void Close()
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
                connection.Dispose();
            }
        }

        /// <summary>
        /// Обновление состояния по сессиям. Применяется при успешной отправке данных.
        /// </summary>
        /// <param name="sessions"></param>
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