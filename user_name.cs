using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Cybersecurity_chatbot_p2
{
    public class user_name
    {
        public string submit_name(TextBox user_name, ListView chats)
        {
            string filename = "user_names.txt";

            if (!File.Exists(filename))
            {
                File.AppendAllText(filename, "auto_create\n");
            }

            string name = user_name.Text.Trim();
            bool found = CheckName(name);

            if (!found)
            {
                File.AppendAllText(filename, name + "\n");
                AddChatMessage("ChatBot", "Hey " + name + "! Welcome to the Cybersecurity Awareness Chatbot! I'm here to help you stay safe online.", chats);
            }
            else
            {
                AddChatMessage("ChatBot", "Hey " + name + "! Welcome back! How can I help you with cybersecurity today?", chats);
            }

            return name;
        }

        private bool CheckName(string name)
        {
            string filename = "user_names.txt";
            string[] names = File.ReadAllLines(filename);

            foreach (string name_found in names)
            {
                if (name_found.Trim().ToLower() == name.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        private void AddChatMessage(string sender, string message, ListView chats)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Border messageBorder = new Border
                {
                    Margin = new Thickness(0, 2, 0, 2),
                    Padding = new Thickness(5, 3, 5, 3),
                    CornerRadius = new CornerRadius(5),
                    Background = new SolidColorBrush(Color.FromRgb(240, 248, 255)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(173, 216, 230)),
                    BorderThickness = new Thickness(1)
                };

                TextBlock messageText = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(2)
                };

                messageText.Inlines.Add(new Run
                {
                    Text = sender + ": ",
                    Foreground = Brushes.DarkBlue,
                    FontWeight = FontWeights.Bold
                });

                messageText.Inlines.Add(new Run
                {
                    Text = message,
                    Foreground = Brushes.Black
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