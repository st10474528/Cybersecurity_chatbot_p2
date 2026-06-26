-- ============================================
-- CYBERSECURITY CHATBOT DATABASE
-- For SQL Server LocalDB
-- ============================================

-- Create the database
CREATE DATABASE chatbot_tasks;
GO

-- Use the database
USE chatbot_tasks;
GO

-- Users table
CREATE TABLE users (
    id INT PRIMARY KEY IDENTITY(1,1),
    username VARCHAR(50) UNIQUE NOT NULL,
    created_at DATETIME DEFAULT GETDATE()
);
GO

-- Tasks table
CREATE TABLE tasks (
    id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    title VARCHAR(100) NOT NULL,
    description TEXT,
    reminder_date DATETIME NULL,
    is_completed BIT DEFAULT 0,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

-- Activity Logs table
CREATE TABLE activity_logs (
    id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    action_type VARCHAR(50) NOT NULL,
    description TEXT,
    timestamp DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

-- Quiz Scores table
CREATE TABLE quiz_scores (
    id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    score INT NOT NULL,
    total_questions INT NOT NULL,
    date_taken DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO

-- Create indexes for performance
CREATE INDEX idx_tasks_user_id ON tasks(user_id);
CREATE INDEX idx_tasks_completed ON tasks(is_completed);
CREATE INDEX idx_tasks_reminder_date ON tasks(reminder_date);
CREATE INDEX idx_logs_user_id ON activity_logs(user_id);
CREATE INDEX idx_logs_timestamp ON activity_logs(timestamp);
CREATE INDEX idx_quiz_user_id ON quiz_scores(user_id);
GO

-- Show all tables
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' 
AND TABLE_CATALOG = 'chatbot_tasks'
ORDER BY TABLE_NAME;
GO

-- Success message
SELECT 'Database setup complete!' AS Status;
SELECT 'Tables created: users, tasks, activity_logs, quiz_scores' AS Info;
GO