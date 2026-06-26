using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Data.SqlClient;

namespace Cybersecurity_chatbot_p2
{
    public partial class MainWindow : Window
    {
        private ArrayList reply = new ArrayList();
        private ArrayList ignore = new ArrayList();
        private user_name check_name = new user_name();

        private string username = string.Empty;
        private string lastTopic = string.Empty;
        private Random random = new Random();
        private int userId = -1;
        private DatabaseHelper dbHelper;

        // Quiz variables
        private List<QuizQuestion> quizQuestions;
        private int currentQuestionIndex = 0;
        private int quizScore = 0;
        private bool quizInProgress = false;

        public MainWindow()
        {
            InitializeComponent();
            new respond(reply, ignore);
            new voice_greeting();
            dbHelper = new DatabaseHelper();
            InitializeQuizQuestions();

            if (!dbHelper.TestConnection())
            {
                MessageBox.Show("Database connection failed. Some features will be limited.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void proceed(object sender, RoutedEventArgs e)
        {
            home_grid.Visibility = Visibility.Hidden;
            username_grid.Visibility = Visibility.Visible;
        }

        private void submit_name(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernames_input.Text))
            {
                errorMsg.Text = "Please enter a username to continue!";
                errorMsg.Visibility = Visibility.Visible;
                return;
            }

            username = check_name.submit_name(usernames_input, chats);
            userId = dbHelper.GetOrCreateUser(username);

            if (userId > 0)
            {
                LogActivity("LOGIN", $"User {username} logged in");
                AddChatMessage("ChatBot", $"Welcome back, {username}!\n\nI'm your Cybersecurity Assistant. I can help you with:\n- Cybersecurity tips and advice\n- Task management (Add tasks with reminders)\n- Take a cybersecurity quiz\n- View your activity logs\n\nTry these commands:\n- 'Add task - [description]' - Create a task\n- 'My tasks' - View your tasks\n- 'Start quiz' - Test your knowledge\n- 'Show logs' - View your activity", Brushes.LightBlue);
            }

            username_grid.Visibility = Visibility.Hidden;
            chat_grid.Visibility = Visibility.Visible;
        }

        private void send(object sender, RoutedEventArgs e)
        {
            string rawQuestion = question.Text.Trim();

            if (string.IsNullOrWhiteSpace(rawQuestion))
            {
                AddChatMessage("ChatBot", "Please enter a question about cybersecurity.", Brushes.LightBlue);
                question.Clear();
                return;
            }

            AddChatMessage(username, rawQuestion, Brushes.LightGreen);

            if (userId > 0)
            {
                LogActivity("USER_MESSAGE", rawQuestion);
            }

            if (HandleTaskCommands(rawQuestion))
            {
                question.Clear();
                return;
            }

            if (HandleQuizCommands(rawQuestion))
            {
                question.Clear();
                return;
            }

            if (HandleLogCommands(rawQuestion))
            {
                question.Clear();
                return;
            }

            if (rawQuestion.ToLower().Contains("help") || rawQuestion.ToLower().Contains("commands"))
            {
                ShowHelp();
                question.Clear();
                return;
            }

            string followUpResponse = HandleFollowUp(rawQuestion);
            if (followUpResponse != null)
            {
                AddChatMessage("ChatBot", followUpResponse, Brushes.LightBlue);
                if (userId > 0) LogActivity("BOT_RESPONSE", followUpResponse);
                question.Clear();
                return;
            }

            string sentiment = DetectSentiment(rawQuestion);
            string cleanedQuestion = RemoveSpecialCharacters(rawQuestion);
            bool isOnlySentiment = IsOnlySentimentExpression(cleanedQuestion);

            if (isOnlySentiment && sentiment != "neutral")
            {
                string sentimentResponse = GetSentimentResponse(sentiment);
                if (!string.IsNullOrEmpty(sentimentResponse))
                {
                    AddChatMessage("ChatBot", sentimentResponse, Brushes.LightBlue);
                    if (userId > 0) LogActivity("SENTIMENT_RESPONSE", sentimentResponse);
                }
                question.Clear();
                return;
            }

            string cybersecurityResponse = GetCybersecurityResponse(cleanedQuestion);
            if (!string.IsNullOrEmpty(cybersecurityResponse))
            {
                AddChatMessage("ChatBot", cybersecurityResponse, Brushes.LightBlue);
                if (userId > 0) LogActivity("CYBERSECURITY_RESPONSE", cybersecurityResponse);
            }

            question.Clear();
        }

        // ============================================
        // TAB CONTROL
        // ============================================

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = sender as TabControl;
            if (tabControl == null) return;

            chatTabContent.Visibility = Visibility.Collapsed;
            taskTabContent.Visibility = Visibility.Collapsed;
            quizTabContent.Visibility = Visibility.Collapsed;
            logsTabContent.Visibility = Visibility.Collapsed;

            switch (tabControl.SelectedIndex)
            {
                case 0:
                    chatTabContent.Visibility = Visibility.Visible;
                    break;
                case 1:
                    taskTabContent.Visibility = Visibility.Visible;
                    RefreshTasks();
                    break;
                case 2:
                    quizTabContent.Visibility = Visibility.Visible;
                    break;
                case 3:
                    logsTabContent.Visibility = Visibility.Visible;
                    RefreshLogs();
                    break;
            }
        }

        // ============================================
        // TASK 1: TASK ASSISTANT
        // ============================================

        private bool HandleTaskCommands(string message)
        {
            string lowerMsg = message.ToLower();

            if (lowerMsg.Contains("add task") || lowerMsg.Contains("new task") || lowerMsg.Contains("create task"))
            {
                string taskDetails = ExtractTaskDescription(message);

                if (string.IsNullOrWhiteSpace(taskDetails))
                {
                    AddChatMessage("ChatBot",
                        "Please provide task details.\n\nExamples:\n- 'Add task - Review privacy settings'\n- 'Add task for password remind me in 3 days'",
                        Brushes.LightBlue);
                    return true;
                }

                var (taskDescription, reminderDate) = ParseReminder(taskDetails);
                taskDescription = CleanTaskDescription(taskDescription);

                if (string.IsNullOrWhiteSpace(taskDescription))
                {
                    AddChatMessage("ChatBot", "Please provide a valid task description.", Brushes.LightBlue);
                    return true;
                }

                if (userId > 0 && dbHelper.AddTask(userId, taskDescription, taskDescription, reminderDate))
                {
                    string response = $"Task added: '{taskDescription}'";
                    if (reminderDate.HasValue)
                    {
                        response += $"\nReminder set for: {reminderDate.Value:yyyy-MM-dd HH:mm}";
                        LogActivity("REMINDER_SET", $"Reminder for '{taskDescription}' on {reminderDate.Value}");
                    }
                    else
                    {
                        response += "\nNo reminder set. You can add one next time.";
                    }

                    AddChatMessage("ChatBot", response, Brushes.LightBlue);
                    LogActivity("TASK_ADDED", taskDescription);
                    RefreshTasks();
                }
                else
                {
                    AddChatMessage("ChatBot", "Failed to add task. Please try again.", Brushes.LightBlue);
                }
                return true;
            }

            if (lowerMsg.Contains("my tasks") || lowerMsg.Contains("show tasks") ||
                lowerMsg.Contains("view tasks") || lowerMsg.Contains("list tasks"))
            {
                ShowTasksInChat();
                RefreshTasks();
                return true;
            }

            if (lowerMsg.Contains("complete task") || lowerMsg.Contains("finish task") ||
                lowerMsg.Contains("done task") || lowerMsg.Contains("mark as done"))
            {
                int taskId = ExtractTaskId(message);
                if (taskId > 0)
                {
                    if (dbHelper.CompleteTask(taskId))
                    {
                        AddChatMessage("ChatBot", $"Task {taskId} marked as completed! Well done!", Brushes.LightBlue);
                        LogActivity("TASK_COMPLETED", $"Task ID {taskId} completed");
                        RefreshTasks();
                    }
                    else
                    {
                        AddChatMessage("ChatBot", $"Task {taskId} not found or already completed.", Brushes.LightBlue);
                    }
                }
                else
                {
                    AddChatMessage("ChatBot", "Please specify which task to complete.\n\nExample: 'Complete task 3'", Brushes.LightBlue);
                }
                return true;
            }

            if (lowerMsg.Contains("delete task") || lowerMsg.Contains("remove task"))
            {
                int taskId = ExtractTaskId(message);
                if (taskId > 0)
                {
                    var result = MessageBox.Show($"Are you sure you want to delete task #{taskId}?",
                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        if (dbHelper.DeleteTask(taskId))
                        {
                            AddChatMessage("ChatBot", $"Task {taskId} has been deleted.", Brushes.LightBlue);
                            LogActivity("TASK_DELETED", $"Task ID {taskId} deleted");
                            RefreshTasks();
                        }
                        else
                        {
                            AddChatMessage("ChatBot", $"Task {taskId} not found.", Brushes.LightBlue);
                        }
                    }
                }
                else
                {
                    AddChatMessage("ChatBot", "Please specify which task to delete.\n\nExample: 'Delete task 3'", Brushes.LightBlue);
                }
                return true;
            }

            if (lowerMsg.Contains("task stats") || lowerMsg.Contains("task summary") ||
                lowerMsg.Contains("how many tasks") || lowerMsg.Contains("task count"))
            {
                ShowTaskStats();
                return true;
            }

            return false;
        }

        private void RefreshTasks()
        {
            if (userId <= 0) return;

            try
            {
                DataTable tasks = dbHelper.GetTasks(userId, true);
                var taskList = new List<TaskItem>();

                foreach (DataRow row in tasks.Rows)
                {
                    int id = Convert.ToInt32(row["id"]);
                    string title = row["title"].ToString();
                    bool completed = Convert.ToBoolean(row["is_completed"]);
                    DateTime? reminder = row["reminder_date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["reminder_date"]) : null;

                    taskList.Add(new TaskItem
                    {
                        id = id,
                        title = title,
                        status = completed ? "Done" : "Pending",
                        reminder = reminder.HasValue ? reminder.Value.ToString("yyyy-MM-dd HH:mm") : "No reminder"
                    });
                }

                taskListView.ItemsSource = taskList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing tasks: {ex.Message}");
            }
        }

        private void ShowTasksInChat()
        {
            if (userId <= 0)
            {
                AddChatMessage("ChatBot", "Please log in first to view tasks.", Brushes.LightBlue);
                return;
            }

            DataTable tasks = dbHelper.GetTasks(userId);

            if (tasks.Rows.Count == 0)
            {
                AddChatMessage("ChatBot",
                    "You have no tasks.\n\nTo add one, say:\n'Add task - [your task description]'\n\nExample: Add task - Enable Two-Factor Authentication",
                    Brushes.LightBlue);
                return;
            }

            string taskList = "Your Cybersecurity Tasks:\n";
            taskList += "========================================\n";

            int count = 0;
            foreach (DataRow row in tasks.Rows)
            {
                count++;
                int id = Convert.ToInt32(row["id"]);
                string title = row["title"].ToString();
                bool completed = Convert.ToBoolean(row["is_completed"]);
                DateTime? reminder = row["reminder_date"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["reminder_date"]) : null;
                DateTime created = Convert.ToDateTime(row["created_at"]);

                string status = completed ? "COMPLETED" : "PENDING";
                string statusIcon = completed ? "✅" : "⏳";
                string reminderText = reminder.HasValue ? $"Reminder: {reminder.Value:yyyy-MM-dd HH:mm}" : "No reminder";

                taskList += $"\n{statusIcon} [#{id}] {title}\n";
                taskList += $"   Status: {status}\n";
                taskList += $"   {reminderText}\n";
                taskList += $"   Created: {created:yyyy-MM-dd HH:mm}\n";
            }

            taskList += "\n========================================\n";
            taskList += $"Total: {count} task{(count > 1 ? "s" : "")}\n";
            taskList += "\nCommands:\n";
            taskList += "- 'Complete task [ID]' - Mark as done\n";
            taskList += "- 'Delete task [ID]' - Remove task\n";
            taskList += "- 'Task stats' - View statistics";

            AddChatMessage("ChatBot", taskList, Brushes.LightBlue);
        }

        private void ShowTaskStats()
        {
            if (userId <= 0)
            {
                AddChatMessage("ChatBot", "Please log in first to view task statistics.", Brushes.LightBlue);
                return;
            }

            var (pending, completed, total) = dbHelper.GetTaskStats(userId);

            string stats = "Task Statistics:\n";
            stats += "========================================\n";
            stats += $"Total Tasks: {total}\n";
            stats += $"Pending: {pending}\n";
            stats += $"Completed: {completed}\n";
            stats += $"Completion Rate: {(total > 0 ? (completed * 100 / total) : 0)}%\n";
            stats += "========================================\n";
            stats += "To view all tasks: 'My tasks'";

            AddChatMessage("ChatBot", stats, Brushes.LightBlue);
        }

        // ============================================
        // TASK 2: CYBERSECURITY QUIZ
        // ============================================

        private bool HandleQuizCommands(string message)
        {
            string lowerMsg = message.ToLower();

            if (lowerMsg.Contains("start quiz") || lowerMsg.Contains("take quiz") ||
                lowerMsg.Contains("begin quiz") || lowerMsg.Contains("quiz"))
            {
                if (!quizInProgress)
                {
                    StartQuiz();
                }
                else
                {
                    AddChatMessage("ChatBot", "A quiz is already in progress! Type 'Answer [number]' or 'Next' to continue.", Brushes.LightBlue);
                }
                return true;
            }

            if (quizInProgress && (lowerMsg.Contains("next question") || lowerMsg.Contains("next") || lowerMsg.Contains("skip")))
            {
                if (currentQuestionIndex < quizQuestions.Count)
                {
                    AskQuestion();
                }
                else
                {
                    EndQuiz();
                }
                return true;
            }

            if (quizInProgress && (lowerMsg.StartsWith("answer") || Regex.IsMatch(message, @"^\d+$")))
            {
                int answerIndex = -1;

                if (Regex.IsMatch(message, @"^\d+$"))
                {
                    answerIndex = int.Parse(message) - 1;
                }
                else
                {
                    var match = Regex.Match(message, @"(\d+)");
                    if (match.Success)
                    {
                        answerIndex = int.Parse(match.Groups[1].Value) - 1;
                    }
                }

                if (answerIndex >= 0)
                {
                    SubmitQuizAnswer(answerIndex);
                }
                else
                {
                    AddChatMessage("ChatBot", "Please specify your answer number. Example: 'Answer 2' or just '2'", Brushes.LightBlue);
                }
                return true;
            }

            if (quizInProgress && (lowerMsg.Contains("quiz score") || lowerMsg.Contains("my score") || lowerMsg.Contains("score")))
            {
                AddChatMessage("ChatBot", $"Your current quiz score: {quizScore}/{currentQuestionIndex}", Brushes.LightBlue);
                return true;
            }

            return false;
        }

        private void InitializeQuizQuestions()
        {
            quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion(
                    "What is the most secure type of password?",
                    new List<string> {
                        "A short password with numbers",
                        "A long password with uppercase, lowercase, numbers, and special characters",
                        "Your birthdate",
                        "Your pet's name"
                    },
                    1,
                    "A strong password should be at least 12 characters long and include a mix of uppercase, lowercase, numbers, and special characters."
                ),
                new QuizQuestion(
                    "What should you do if you receive an email asking for your bank details?",
                    new List<string> {
                        "Reply with your details",
                        "Ignore it",
                        "Report it as phishing and delete it",
                        "Forward it to your friends"
                    },
                    2,
                    "Phishing emails should be reported and deleted. Never share personal information via email."
                ),
                new QuizQuestion(
                    "What is Two-Factor Authentication (2FA)?",
                    new List<string> {
                        "A type of password",
                        "A security method requiring two verification steps",
                        "A virus scanner",
                        "A social media feature"
                    },
                    1,
                    "2FA adds an extra layer of security by requiring both your password and a second verification method."
                ),
                new QuizQuestion(
                    "Why should you use a VPN on public Wi-Fi?",
                    new List<string> {
                        "To make your internet faster",
                        "To encrypt your data and protect privacy",
                        "To avoid paying for Wi-Fi",
                        "To download more content"
                    },
                    1,
                    "Public Wi-Fi is often unsecured. A VPN encrypts your data to protect your privacy."
                ),
                new QuizQuestion(
                    "What is social engineering in cybersecurity?",
                    new List<string> {
                        "Building social networks",
                        "Manipulating people to reveal confidential information",
                        "Engineering social media posts",
                        "Creating social apps"
                    },
                    1,
                    "Social engineering tricks people into revealing sensitive information. Always verify who you're talking to."
                ),
                new QuizQuestion(
                    "How often should you update your passwords?",
                    new List<string> {
                        "Never",
                        "Every 3-6 months",
                        "Once a year",
                        "Only when you're hacked"
                    },
                    1,
                    "Regular password updates (every 3-6 months) help protect your accounts from unauthorized access."
                ),
                new QuizQuestion(
                    "What should you check before clicking a link in an email?",
                    new List<string> {
                        "The color of the link",
                        "The actual URL by hovering over it",
                        "The font style",
                        "The size of the image"
                    },
                    1,
                    "Always hover over links to see the actual URL before clicking. This helps identify phishing attempts."
                ),
                new QuizQuestion(
                    "What is ransomware?",
                    new List<string> {
                        "A type of game",
                        "Malware that locks your files and demands payment",
                        "A social media platform",
                        "An antivirus software"
                    },
                    1,
                    "Ransomware is malicious software that encrypts your files and demands payment for decryption."
                ),
                new QuizQuestion(
                    "How can you protect your privacy on social media?",
                    new List<string> {
                        "Post everything publicly",
                        "Review and limit privacy settings",
                        "Share your personal contact info",
                        "Accept all friend requests"
                    },
                    1,
                    "Regularly review your privacy settings and limit what you share publicly on social media."
                ),
                new QuizQuestion(
                    "What is the best way to store your passwords?",
                    new List<string> {
                        "Write them on a sticky note",
                        "Use a password manager",
                        "Save them in a text file on your desktop",
                        "Use the same password for everything"
                    },
                    1,
                    "Password managers securely store and generate strong passwords, making it easy to use unique passwords for each account."
                )
            };
        }

        private void StartQuiz()
        {
            if (quizQuestions.Count == 0)
            {
                AddChatMessage("ChatBot", "No quiz questions available. Please try again later.", Brushes.LightBlue);
                return;
            }

            quizInProgress = true;
            currentQuestionIndex = 0;
            quizScore = 0;
            quizStatus.Text = "Quiz in progress...";
            quizProgress.Value = 0;
            quizResults.Items.Clear();

            AddChatMessage("ChatBot", "Welcome to the Cybersecurity Quiz!\n\nYou will be asked 10 questions. For each question, type 'Answer [number]' or just the number.\nType 'Next' or 'Skip' to skip a question.\n\nLet's begin!", Brushes.LightBlue);
            AskQuestion();

            if (userId > 0)
                LogActivity("QUIZ_STARTED", "Started cybersecurity quiz");
        }

        private void AskQuestion()
        {
            if (currentQuestionIndex >= quizQuestions.Count)
            {
                EndQuiz();
                return;
            }

            var question = quizQuestions[currentQuestionIndex];
            string questionText = $"Question {currentQuestionIndex + 1}/{quizQuestions.Count}\n";
            questionText += $"{question.Text}\n\n";

            for (int i = 0; i < question.Options.Count; i++)
            {
                questionText += $"  {i + 1}. {question.Options[i]}\n";
            }

            questionText += $"\nType 'Answer [number]' or just the number, or 'Next' to skip.";
            AddChatMessage("ChatBot", questionText, Brushes.LightBlue);

            quizStatus.Text = $"Question {currentQuestionIndex + 1}/{quizQuestions.Count}";
            quizProgress.Value = ((double)currentQuestionIndex / quizQuestions.Count) * 100;
        }

        private void SubmitQuizAnswer(int answerIndex)
        {
            if (currentQuestionIndex >= quizQuestions.Count)
            {
                EndQuiz();
                return;
            }

            var question = quizQuestions[currentQuestionIndex];

            if (answerIndex >= 0 && answerIndex < question.Options.Count)
            {
                string explanation = !string.IsNullOrEmpty(question.Explanation) ? $" - {question.Explanation}" : "";

                if (answerIndex == question.CorrectAnswerIndex)
                {
                    quizScore++;
                    AddChatMessage("ChatBot", $"Correct! Well done!{explanation}", Brushes.LightGreen);
                    LogActivity("QUIZ_ANSWER", $"Correct answer for Q{currentQuestionIndex + 1}");
                }
                else
                {
                    AddChatMessage("ChatBot", $"Incorrect. The correct answer was: {question.Options[question.CorrectAnswerIndex]}.{explanation}", Brushes.LightPink);
                    LogActivity("QUIZ_ANSWER", $"Incorrect answer for Q{currentQuestionIndex + 1}");
                }
            }
            else
            {
                AddChatMessage("ChatBot", "Invalid answer number. Please choose from the listed options.", Brushes.LightBlue);
                return;
            }

            currentQuestionIndex++;
            AskQuestion();
        }

        private void EndQuiz()
        {
            quizInProgress = false;

            string message = "Quiz Complete!\n";
            message += $"Your score: {quizScore}/{quizQuestions.Count}\n";

            double percentage = (double)quizScore / quizQuestions.Count * 100;
            message += $"Percentage: {percentage:F1}%\n\n";

            if (percentage >= 80)
                message += "Excellent! You're a cybersecurity expert!\nKeep up the great habits!";
            else if (percentage >= 60)
                message += "Good job! You have solid cybersecurity knowledge.\nReview the topics you missed to improve!";
            else if (percentage >= 40)
                message += "Not bad! Keep learning about cybersecurity.\nTry researching the topics you got wrong.";
            else
                message += "Keep learning! Cybersecurity is important.\nReview the basics and try again!";

            AddChatMessage("ChatBot", message, Brushes.LightBlue);
            quizStatus.Text = "Quiz completed!";
            quizProgress.Value = 100;

            if (userId > 0)
            {
                LogActivity("QUIZ_COMPLETED", $"Score: {quizScore}/{quizQuestions.Count}");
                dbHelper.SaveQuizScore(userId, quizScore, quizQuestions.Count);
            }
        }

        // ============================================
        // TASK 3: NLP SIMULATION
        // ============================================

        private string ExtractTaskDescription(string message)
        {
            string lowerMsg = message.ToLower();
            string taskDetails = "";
            int startIndex = -1;

            string[] patterns = { "add task", "new task", "create task", "set task", "make task" };
            foreach (string pattern in patterns)
            {
                if (lowerMsg.Contains(pattern))
                {
                    startIndex = lowerMsg.IndexOf(pattern) + pattern.Length;
                    break;
                }
            }

            if (startIndex > 0 && startIndex < message.Length)
            {
                taskDetails = message.Substring(startIndex).Trim();

                string[] separators = { "-", ":", "for ", "to ", "about " };
                foreach (string sep in separators)
                {
                    if (taskDetails.ToLower().StartsWith(sep))
                    {
                        taskDetails = taskDetails.Substring(sep.Length).Trim();
                        break;
                    }
                }
            }
            return taskDetails;
        }

        private (string Description, DateTime? ReminderDate) ParseReminder(string taskDetails)
        {
            string description = taskDetails;
            DateTime? reminderDate = null;

            var match1 = Regex.Match(taskDetails, @"remind me in (\d+)\s*(day|days|week|weeks)", RegexOptions.IgnoreCase);
            if (match1.Success)
            {
                int number = int.Parse(match1.Groups[1].Value);
                string unit = match1.Groups[2].Value.ToLower();
                reminderDate = unit.StartsWith("day") ? DateTime.Now.AddDays(number) : DateTime.Now.AddDays(number * 7);
                description = taskDetails.Substring(0, match1.Index).Trim();
            }
            else
            {
                var match2 = Regex.Match(taskDetails, @"set a reminder for (\d+)\s*(day|days|week|weeks)", RegexOptions.IgnoreCase);
                if (match2.Success)
                {
                    int number = int.Parse(match2.Groups[1].Value);
                    string unit = match2.Groups[2].Value.ToLower();
                    reminderDate = unit.StartsWith("day") ? DateTime.Now.AddDays(number) : DateTime.Now.AddDays(number * 7);
                    description = taskDetails.Substring(0, match2.Index).Trim();
                }
                else
                {
                    var match3 = Regex.Match(taskDetails, @"in (\d+)\s*(day|days|week|weeks)", RegexOptions.IgnoreCase);
                    if (match3.Success)
                    {
                        int number = int.Parse(match3.Groups[1].Value);
                        string unit = match3.Groups[2].Value.ToLower();
                        reminderDate = unit.StartsWith("day") ? DateTime.Now.AddDays(number) : DateTime.Now.AddDays(number * 7);
                        description = taskDetails.Substring(0, match3.Index).Trim();
                    }
                }
            }

            return (description, reminderDate);
        }

        private string CleanTaskDescription(string description)
        {
            string[] prefixes = { "for ", "to ", "about ", "a ", "an ", "the " };
            foreach (string prefix in prefixes)
            {
                if (description.ToLower().StartsWith(prefix))
                {
                    description = description.Substring(prefix.Length).Trim();
                    break;
                }
            }

            description = description.TrimEnd('.', ',', ';', ':');

            if (description.Length > 0)
            {
                description = char.ToUpper(description[0]) + description.Substring(1);
            }

            return description;
        }

        private int ExtractTaskId(string message)
        {
            var match = Regex.Match(message, @"(\d+)");
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            return -1;
        }

        private string DetectSentiment(string message)
        {
            string lowerMsg = message.ToLower();

            string[] confusedWords = { "confused", "confusing", "not understand", "unclear" };
            string[] worriedWords = { "worried", "scared", "afraid", "nervous", "anxious", "concerned", "unsafe" };
            string[] frustratedWords = { "frustrated", "annoyed", "angry", "upset", "tired" };
            string[] curiousWords = { "curious", "interested", "want to learn", "tell me", "explain", "how", "why", "what" };
            string[] happyWords = { "happy", "great", "awesome", "thanks", "thank you", "good", "excellent" };

            foreach (string word in confusedWords) if (lowerMsg.Contains(word)) return "confused";
            foreach (string word in worriedWords) if (lowerMsg.Contains(word)) return "worried";
            foreach (string word in frustratedWords) if (lowerMsg.Contains(word)) return "frustrated";
            foreach (string word in curiousWords) if (lowerMsg.Contains(word)) return "curious";
            foreach (string word in happyWords) if (lowerMsg.Contains(word)) return "happy";

            return "neutral";
        }

        // ============================================
        // TASK 4: ACTIVITY LOG
        // ============================================

        private void LogActivity(string actionType, string description)
        {
            if (userId > 0)
            {
                dbHelper.AddActivityLog(userId, actionType, description);
            }
        }

        private bool HandleLogCommands(string message)
        {
            string lowerMsg = message.ToLower();

            if (lowerMsg.Contains("show logs") || lowerMsg.Contains("view logs") ||
                lowerMsg.Contains("activity log") || lowerMsg.Contains("my activity") ||
                lowerMsg.Contains("what have you done"))
            {
                ShowActivityLogs();
                RefreshLogs();
                return true;
            }

            if (lowerMsg.Contains("clear logs") || lowerMsg.Contains("delete logs"))
            {
                AddChatMessage("ChatBot", "Logs are kept for security and cannot be deleted.", Brushes.LightBlue);
                return true;
            }

            return false;
        }

        private void RefreshLogs()
        {
            if (userId <= 0) return;

            try
            {
                DataTable logs = dbHelper.GetActivityLogs(userId);
                var logList = new List<LogItem>();

                foreach (DataRow row in logs.Rows)
                {
                    string action = row["action_type"].ToString();
                    string description = row["description"].ToString();
                    DateTime timestamp = Convert.ToDateTime(row["timestamp"]);

                    logList.Add(new LogItem
                    {
                        action = action,
                        description = description,
                        time = timestamp.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                logListView.ItemsSource = logList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing logs: {ex.Message}");
            }
        }

        private void ShowActivityLogs()
        {
            if (userId <= 0)
            {
                AddChatMessage("ChatBot", "Please log in first to view your activity logs.", Brushes.LightBlue);
                return;
            }

            DataTable logs = dbHelper.GetActivityLogs(userId);
            if (logs.Rows.Count == 0)
            {
                AddChatMessage("ChatBot", "No activity logs found. Start interacting with the chatbot!", Brushes.LightBlue);
                return;
            }

            string logText = "Activity Log (Last 100 actions):\n";
            logText += "========================================\n";

            int count = 0;
            foreach (DataRow row in logs.Rows)
            {
                count++;
                string action = row["action_type"].ToString();
                string description = row["description"].ToString();
                DateTime timestamp = Convert.ToDateTime(row["timestamp"]);

                logText += $"\n{count}. [{timestamp:yyyy-MM-dd HH:mm:ss}]\n";
                logText += $"   Action: {action}\n";
                if (!string.IsNullOrEmpty(description) && description != action)
                {
                    logText += $"   Details: {description}\n";
                }
            }

            logText += "\n========================================\n";
            logText += $"Total activities: {count}";

            AddChatMessage("ChatBot", logText, Brushes.LightBlue);
        }

        // ============================================
        // EVENT HANDLERS
        // ============================================

        private void RefreshTasks(object sender, RoutedEventArgs e)
        {
            RefreshTasks();
        }

        private void MarkTaskCompleted(object sender, RoutedEventArgs e)
        {
            if (taskListView.SelectedItem == null)
            {
                AddChatMessage("ChatBot", "Please select a task from the list first.", Brushes.LightBlue);
                return;
            }

            TaskItem selectedItem = taskListView.SelectedItem as TaskItem;
            if (selectedItem == null) return;

            int taskId = selectedItem.id;

            if (dbHelper.CompleteTask(taskId))
            {
                AddChatMessage("ChatBot", $"Task {taskId} marked as completed! Well done!", Brushes.LightBlue);
                LogActivity("TASK_COMPLETED", $"Task ID {taskId} completed");
                RefreshTasks();
            }
            else
            {
                AddChatMessage("ChatBot", "Failed to complete task. Please try again.", Brushes.LightBlue);
            }
        }

        private void DeleteSelectedTask(object sender, RoutedEventArgs e)
        {
            if (taskListView.SelectedItem == null)
            {
                AddChatMessage("ChatBot", "Please select a task from the list first.", Brushes.LightBlue);
                return;
            }

            TaskItem selectedItem = taskListView.SelectedItem as TaskItem;
            if (selectedItem == null) return;

            int taskId = selectedItem.id;

            var result = MessageBox.Show($"Are you sure you want to delete task #{taskId}?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (dbHelper.DeleteTask(taskId))
                {
                    AddChatMessage("ChatBot", $"Task {taskId} has been deleted.", Brushes.LightBlue);
                    LogActivity("TASK_DELETED", $"Task ID {taskId} deleted");
                    RefreshTasks();
                }
                else
                {
                    AddChatMessage("ChatBot", "Failed to delete task. Please try again.", Brushes.LightBlue);
                }
            }
        }

        private void StartQuizButton(object sender, RoutedEventArgs e)
        {
            StartQuiz();
        }

        private void RefreshLogs(object sender, RoutedEventArgs e)
        {
            RefreshLogs();
        }

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                send(sender, e);
            }
        }

        // ============================================
        // HELPER METHODS
        // ============================================

        private void ShowHelp()
        {
            string helpText = "Available Commands:\n\n";
            helpText += "Cybersecurity Topics:\n";
            helpText += "   - Ask about: passwords, scams, phishing, privacy, malware, social engineering, 2FA, ransomware, VPN, Wi-Fi\n\n";
            helpText += "Task Management:\n";
            helpText += "   - 'Add task - [description]' - Create a new task\n";
            helpText += "   - 'My tasks' - View all your tasks\n";
            helpText += "   - 'Complete task [ID]' - Mark a task as done\n";
            helpText += "   - 'Delete task [ID]' - Remove a task\n";
            helpText += "   - 'Task stats' - View task statistics\n\n";
            helpText += "Quiz:\n";
            helpText += "   - 'Start quiz' - Begin cybersecurity quiz\n";
            helpText += "   - 'Answer [number]' - Submit your answer\n";
            helpText += "   - 'Next' - Skip to next question\n";
            helpText += "   - 'Quiz score' - Check your current score\n\n";
            helpText += "Activity:\n";
            helpText += "   - 'Show logs' - View your activity history\n\n";
            helpText += "General:\n";
            helpText += "   - 'Help' or 'Commands' - Show this help menu\n";
            helpText += "   - 'Tell me more' - Get more info about current topic";

            AddChatMessage("ChatBot", helpText, Brushes.LightBlue);
            LogActivity("HELP", "User requested help");
        }

        // ============================================
        // ORIGINAL CHATBOT METHODS
        // ============================================

        private string GetCybersecurityResponse(string questions)
        {
            if (string.IsNullOrWhiteSpace(questions))
            {
                return null;
            }

            string[] words = questions.ToLower().Split(new char[] { ' ', ',', '.', '?', '!', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> allMatchingResponses = new List<string>();

            foreach (string word in words)
            {
                if (word.Length < 3 || ignore.Contains(word.ToLower()))
                    continue;

                foreach (string answer in reply)
                {
                    string answerLower = answer.ToString().ToLower();

                    if (answerLower.StartsWith(word))
                    {
                        string[] parts = answer.ToString().Split(new char[] { ' ' }, 2);
                        if (parts.Length > 1)
                        {
                            allMatchingResponses.Add(parts[1]);
                            lastTopic = word;
                        }
                    }

                    string[] multiTopics = { "password", "scam", "phish", "privacy", "malware", "social", "ransomware", "identity", "wifi", "vpn", "firewall", "greeting", "purpose", "cybersecurity" };
                    foreach (string topic in multiTopics)
                    {
                        if (word.Contains(topic) || topic.Contains(word))
                        {
                            if (answerLower.StartsWith(topic))
                            {
                                string[] parts = answer.ToString().Split(new char[] { ' ' }, 2);
                                if (parts.Length > 1)
                                {
                                    allMatchingResponses.Add(parts[1]);
                                    lastTopic = topic;
                                }
                            }
                        }
                    }
                }
            }

            allMatchingResponses = allMatchingResponses.Distinct().ToList();

            if (allMatchingResponses.Count > 0)
            {
                Random rand = new Random();
                return allMatchingResponses[rand.Next(allMatchingResponses.Count)];
            }

            string[] fallbackMessages = {
                "I'm here to help with cybersecurity! You can ask me about:\n- Password safety\n- Recognizing scams\n- Phishing emails\n- Online privacy\n- Malware protection\n- Social engineering\n- Two-factor authentication (2FA)\n- VPNs and public Wi-Fi\n\nTry these commands:\n- 'Add task - [description]' - Create a task\n- 'My tasks' - View your tasks\n- 'Start quiz' - Test your knowledge\n- 'Show logs' - View your activity",
                "I specialize in cybersecurity awareness. Try asking: 'How do I create a strong password?' or 'What is phishing?'\n\nYou can also manage tasks, take quizzes, and view logs!",
                "That's not a cybersecurity topic I recognize. Would you like to learn about passwords, scams, privacy, or malware protection? Or try one of my features: tasks, quizzes, or logs."
            };
            Random randomGen = new Random();
            return fallbackMessages[randomGen.Next(fallbackMessages.Length)];
        }

        private bool IsOnlySentimentExpression(string message)
        {
            string[] sentimentWords = { "confused", "frustrated", "worried", "curious", "happy", "sad", "angry", "scared", "afraid", "nervous", "anxious" };
            string[] words = message.ToLower().Split(new char[] { ' ', ',', '.', '?', '!', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length <= 3)
            {
                foreach (string word in words)
                {
                    bool isSentiment = false;
                    foreach (string sentiment in sentimentWords)
                    {
                        if (word == sentiment || word.Contains(sentiment))
                        {
                            isSentiment = true;
                            break;
                        }
                    }
                    if (!isSentiment && word.Length > 2)
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        private string HandleFollowUp(string message)
        {
            string lowerMsg = message.ToLower();

            if (lowerMsg.Contains("tell me more") || lowerMsg.Contains("explain more") || lowerMsg.Contains("more about"))
            {
                if (!string.IsNullOrEmpty(lastTopic))
                {
                    return $"Let me tell you more about {lastTopic}. " + GetResponseForTopic(lastTopic);
                }
                return "What topic would you like to know more about? You can ask about passwords, scams, phishing, privacy, malware, or social engineering.";
            }

            if (lowerMsg.Contains("another tip") || lowerMsg.Contains("another advice") || lowerMsg.Contains("give me another"))
            {
                if (!string.IsNullOrEmpty(lastTopic))
                {
                    return $"Here's another tip about {lastTopic}. " + GetAlternativeResponseForTopic(lastTopic);
                }
                return "What topic would you like another tip about? Try asking about passwords, scams, or phishing.";
            }

            return null;
        }

        private string GetResponseForTopic(string topic)
        {
            foreach (string answer in reply)
            {
                if (answer.ToString().ToLower().StartsWith(topic.ToLower()))
                {
                    string[] parts = answer.ToString().Split(new char[] { ' ' }, 2);
                    if (parts.Length > 1)
                        return parts[1];
                }
            }
            return "I have more information on that topic. Could you be more specific?";
        }

        private string GetAlternativeResponseForTopic(string topic)
        {
            List<string> topicResponses = new List<string>();
            foreach (string answer in reply)
            {
                if (answer.ToString().ToLower().StartsWith(topic.ToLower()))
                {
                    topicResponses.Add(answer.ToString());
                }
            }

            if (topicResponses.Count > 0)
            {
                int index = random.Next(topicResponses.Count);
                string selected = topicResponses[index];
                string[] parts = selected.Split(new char[] { ' ' }, 2);
                return parts.Length > 1 ? parts[1] : "Stay safe online!";
            }
            return "Keep practicing good cybersecurity habits!";
        }

        private string GetSentimentResponse(string sentiment)
        {
            List<string> matchingResponses = new List<string>();

            foreach (string answer in reply)
            {
                string answerLower = answer.ToString().ToLower();

                if (answerLower.StartsWith(sentiment + " "))
                {
                    string[] parts = answer.ToString().Split(new char[] { ' ' }, 2);
                    if (parts.Length > 1)
                    {
                        matchingResponses.Add(parts[1]);
                    }
                }
            }

            if (matchingResponses.Count > 0)
            {
                Random rand = new Random();
                return matchingResponses[rand.Next(matchingResponses.Count)];
            }

            return null;
        }

        private string RemoveSpecialCharacters(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            StringBuilder sanitized = new StringBuilder();

            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '\'' || c == '-')
                {
                    sanitized.Append(c);
                }
                else
                {
                    sanitized.Append(' ');
                }
            }

            string result = sanitized.ToString();
            result = Regex.Replace(result, @"\s+", " ").Trim();
            return result;
        }

        private void AddChatMessage(string name, string message, Brush color)
        {
            Dispatcher.Invoke(() =>
            {
                Border messageBorder = new Border
                {
                    Margin = new Thickness(0, 2, 0, 2),
                    Padding = new Thickness(5, 3, 5, 3),
                    CornerRadius = new CornerRadius(5)
                };

                if (name.ToLower().Contains("chatbot") || name.ToLower().Contains("chat"))
                {
                    messageBorder.Background = new SolidColorBrush(Color.FromRgb(240, 248, 255));
                    messageBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(173, 216, 230));
                }
                else
                {
                    messageBorder.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                    messageBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(211, 211, 211));
                }
                messageBorder.BorderThickness = new Thickness(1);

                TextBlock messageText = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(2),
                    MaxWidth = 500
                };

                Brush nameColor = (name.ToLower().Contains("chatbot") || name.ToLower().Contains("chat")) ?
                                  Brushes.DarkBlue : Brushes.DarkGreen;
                Brush messageColor = Brushes.Black;

                messageText.Inlines.Add(new Run
                {
                    Text = name + ": ",
                    Foreground = nameColor,
                    FontWeight = FontWeights.Bold
                });

                messageText.Inlines.Add(new Run
                {
                    Text = message,
                    Foreground = messageColor
                });

                messageBorder.Child = messageText;
                chats.Items.Add(messageBorder);

                if (chats.Items.Count > 0)
                {
                    chats.ScrollIntoView(chats.Items[chats.Items.Count - 1]);
                }
            });
        }
    }

    // ============================================
    // DATA CLASSES FOR LISTVIEW BINDING
    // ============================================

    public class TaskItem
    {
        public int id { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public string reminder { get; set; }
    }

    public class LogItem
    {
        public string action { get; set; }
        public string description { get; set; }
        public string time { get; set; }
    }
}