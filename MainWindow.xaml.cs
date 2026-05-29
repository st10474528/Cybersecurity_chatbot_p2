using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Cybersecurity_chatbot_p2
{
    public partial class MainWindow : Window
    {
        private ArrayList reply = new ArrayList();
        private ArrayList ignore = new ArrayList();
        private user_name check_name = new user_name();

        private string username = string.Empty;
        private string lastTopic = string.Empty;
        private int counting = 0;
        private Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            new respond(reply, ignore);
            new voice_greeting();
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

            string followUpResponse = HandleFollowUp(rawQuestion);
            if (followUpResponse != null)
            {
                AddChatMessage("ChatBot", followUpResponse, Brushes.LightBlue);
                question.Clear();
                return;
            }

            string sentiment = DetectSentiment(rawQuestion);
            string cleanedQuestion = RemoveSpecialCharacters(rawQuestion);
            bool isOnlySentiment = IsOnlySentimentExpression(cleanedQuestion);

            // For sentiment-only messages - show ONE response and STOP
            if (isOnlySentiment && sentiment != "neutral")
            {
                string sentimentResponse = GetSentimentResponse(sentiment);
                if (!string.IsNullOrEmpty(sentimentResponse))
                {
                    AddChatMessage("ChatBot", sentimentResponse, Brushes.LightBlue);
                }
                question.Clear();
                return;
            }

            // For normal questions - show ONE cybersecurity response
            string cybersecurityResponse = GetCybersecurityResponse(cleanedQuestion);
            if (!string.IsNullOrEmpty(cybersecurityResponse))
            {
                AddChatMessage("ChatBot", cybersecurityResponse, Brushes.LightBlue);
            }

            question.Clear();
        }

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
                "I'm here to help with cybersecurity! You can ask me about:\n- Password safety\n- Recognizing scams\n- Phishing emails\n- Online privacy\n- Malware protection\n- Social engineering\n- Two-factor authentication (2FA)\n- VPNs and public Wi-Fi",
                "I specialize in cybersecurity awareness. Try asking: 'How do I create a strong password?' or 'What is phishing?'",
                "That's not a cybersecurity topic I recognize. Would you like to learn about passwords, scams, privacy, or malware protection?"
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

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                send(sender, e);
            }
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

        private string DetectSentiment(string message)
        {
            string lowerMsg = message.ToLower();

            if (lowerMsg.Contains("confused") || lowerMsg.Contains("confusing"))
                return "confused";

            if (lowerMsg.Contains("worried") || lowerMsg.Contains("scared") || lowerMsg.Contains("afraid") ||
                lowerMsg.Contains("nervous") || lowerMsg.Contains("anxious") || lowerMsg.Contains("concerned"))
                return "worried";

            if (lowerMsg.Contains("frustrated") || lowerMsg.Contains("annoyed") || lowerMsg.Contains("angry") ||
                lowerMsg.Contains("upset"))
                return "frustrated";

            if (lowerMsg.Contains("curious") || lowerMsg.Contains("interested") || lowerMsg.Contains("want to learn") ||
                lowerMsg.Contains("tell me") || lowerMsg.Contains("explain"))
                return "curious";

            if (lowerMsg.Contains("happy") || lowerMsg.Contains("great") || lowerMsg.Contains("awesome") ||
                lowerMsg.Contains("thanks") || lowerMsg.Contains("thank you"))
                return "happy";

            return "neutral";
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
}