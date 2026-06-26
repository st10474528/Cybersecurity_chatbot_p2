using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace Cybersecurity_chatbot_p2
{
    public class DatabaseHelper
    {
        private string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=chatbot_tasks;Integrated Security=True;";

        public DatabaseHelper()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                string masterConnectionString = connectionString.Replace("Database=chatbot_tasks", "Database=master");

                using (SqlConnection conn = new SqlConnection(masterConnectionString))
                {
                    conn.Open();
                    string checkDbQuery = "SELECT COUNT(*) FROM sys.databases WHERE name = 'chatbot_tasks'";
                    SqlCommand checkCmd = new SqlCommand(checkDbQuery, conn);
                    int dbExists = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (dbExists == 0)
                    {
                        string createDbQuery = "CREATE DATABASE chatbot_tasks";
                        SqlCommand createCmd = new SqlCommand(createDbQuery, conn);
                        createCmd.ExecuteNonQuery();
                    }
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    CreateTables(conn);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}\n\nPlease ensure SQL Server LocalDB is installed.",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateTables(SqlConnection conn)
        {
            try
            {
                string[] commands = {
                    @"IF EXISTS (SELECT * FROM sysobjects WHERE name='quiz_scores' AND xtype='U') DROP TABLE quiz_scores;
                      IF EXISTS (SELECT * FROM sysobjects WHERE name='activity_logs' AND xtype='U') DROP TABLE activity_logs;
                      IF EXISTS (SELECT * FROM sysobjects WHERE name='tasks' AND xtype='U') DROP TABLE tasks;
                      IF EXISTS (SELECT * FROM sysobjects WHERE name='users' AND xtype='U') DROP TABLE users;",

                    @"CREATE TABLE users(
                        id INT PRIMARY KEY IDENTITY(1,1),
                        username VARCHAR(50) UNIQUE NOT NULL,
                        created_at DATETIME DEFAULT GETDATE()
                    );",

                    @"CREATE TABLE tasks(
                        id INT PRIMARY KEY IDENTITY(1,1),
                        user_id INT NOT NULL,
                        title VARCHAR(100) NOT NULL,
                        description TEXT,
                        reminder_date DATETIME NULL,
                        is_completed BIT DEFAULT 0,
                        created_at DATETIME DEFAULT GETDATE(),
                        FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
                    );",

                    @"CREATE TABLE activity_logs(
                        id INT PRIMARY KEY IDENTITY(1,1),
                        user_id INT NOT NULL,
                        action_type VARCHAR(50) NOT NULL,
                        description TEXT,
                        timestamp DATETIME DEFAULT GETDATE(),
                        FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
                    );",

                    @"CREATE TABLE quiz_scores(
                        id INT PRIMARY KEY IDENTITY(1,1),
                        user_id INT NOT NULL,
                        score INT NOT NULL,
                        total_questions INT NOT NULL,
                        date_taken DATETIME DEFAULT GETDATE(),
                        FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
                    );",

                    @"CREATE INDEX idx_tasks_user_id ON tasks(user_id);
                      CREATE INDEX idx_tasks_completed ON tasks(is_completed);
                      CREATE INDEX idx_tasks_reminder_date ON tasks(reminder_date);
                      CREATE INDEX idx_logs_user_id ON activity_logs(user_id);
                      CREATE INDEX idx_logs_timestamp ON activity_logs(timestamp);
                      CREATE INDEX idx_quiz_user_id ON quiz_scores(user_id);"
                };

                foreach (string cmd in commands)
                {
                    using (SqlCommand command = new SqlCommand(cmd, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating tables: {ex.Message}");
            }
        }

        public int GetOrCreateUser(string username)
        {
            int userId = -1;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string checkQuery = "SELECT id FROM users WHERE username = @username";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@username", username);
                    object result = checkCmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        userId = Convert.ToInt32(result);
                    }
                    else
                    {
                        string insertQuery = "INSERT INTO users (username) VALUES (@username); SELECT SCOPE_IDENTITY();";
                        SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                        insertCmd.Parameters.AddWithValue("@username", username);
                        userId = Convert.ToInt32(insertCmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error accessing database: {ex.Message}");
            }
            return userId;
        }

        public bool AddTask(int userId, string title, string description, DateTime? reminderDate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO tasks (user_id, title, description, reminder_date) 
                                    VALUES (@userId, @title, @description, @reminderDate)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.Parameters.AddWithValue("@reminderDate", (object)reminderDate ?? DBNull.Value);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding task: {ex.Message}");
                return false;
            }
        }

        public DataTable GetTasks(int userId, bool showCompleted = false)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT id, title, description, reminder_date, is_completed, created_at 
                                    FROM tasks 
                                    WHERE user_id = @userId";
                    if (!showCompleted)
                    {
                        query += " AND is_completed = 0";
                    }
                    query += " ORDER BY is_completed, reminder_date, created_at DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving tasks: {ex.Message}");
            }
            return dt;
        }

        public bool CompleteTask(int taskId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE tasks SET is_completed = 1 WHERE id = @taskId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@taskId", taskId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error completing task: {ex.Message}");
                return false;
            }
        }

        public bool DeleteTask(int taskId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM tasks WHERE id = @taskId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@taskId", taskId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting task: {ex.Message}");
                return false;
            }
        }

        public (int Pending, int Completed, int Total) GetTaskStats(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT 
                                        SUM(CASE WHEN is_completed = 0 THEN 1 ELSE 0 END) AS Pending,
                                        SUM(CASE WHEN is_completed = 1 THEN 1 ELSE 0 END) AS Completed,
                                        COUNT(*) AS Total
                                    FROM tasks 
                                    WHERE user_id = @userId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int pending = reader["Pending"] != DBNull.Value ? Convert.ToInt32(reader["Pending"]) : 0;
                            int completed = reader["Completed"] != DBNull.Value ? Convert.ToInt32(reader["Completed"]) : 0;
                            int total = reader["Total"] != DBNull.Value ? Convert.ToInt32(reader["Total"]) : 0;
                            return (pending, completed, total);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting task stats: {ex.Message}");
            }
            return (0, 0, 0);
        }

        public bool AddActivityLog(int userId, string actionType, string description)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO activity_logs (user_id, action_type, description) VALUES (@userId, @actionType, @description)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@actionType", actionType);
                    cmd.Parameters.AddWithValue("@description", description);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging activity: {ex.Message}");
                return false;
            }
        }

        public DataTable GetActivityLogs(int userId)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT TOP 100 action_type, description, timestamp 
                                    FROM activity_logs 
                                    WHERE user_id = @userId 
                                    ORDER BY timestamp DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving logs: {ex.Message}");
            }
            return dt;
        }

        public bool SaveQuizScore(int userId, int score, int totalQuestions)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO quiz_scores (user_id, score, total_questions) VALUES (@userId, @score, @totalQuestions)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@score", score);
                    cmd.Parameters.AddWithValue("@totalQuestions", totalQuestions);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving quiz score: {ex.Message}");
                return false;
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public string GetDatabasePath()
        {
            return "SQL Server LocalDB: (localdb)\\MSSQLLocalDB - Database: chatbot_tasks";
        }
    }
}