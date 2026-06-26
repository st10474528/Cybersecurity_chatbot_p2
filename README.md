# Cybersecurity Awareness Chatbot - South Africa

## Project Overview

The Cybersecurity Awareness Chatbot is a WPF desktop application designed to educate South African citizens about online safety and cybersecurity threats. The chatbot engages users in conversation, provides information about various cybersecurity topics, detects user sentiment, and remembers user preferences throughout the conversation.

## Features

### Part 2 Requirements Implemented

| Feature | Description |
|---------|-------------|
| **GUI Interface** | User-friendly WPF application with dark theme colors and proper spacing |
| **Voice Greeting** | Plays a welcome sound when the application starts |
| **User Memory** | Remembers user names and interests across sessions using text file storage |
| **Keyword Recognition** | Recognizes cybersecurity topics: passwords, scams, phishing, privacy, malware, social engineering, 2FA, ransomware, identity theft, VPNs |
| **Random Responses** | Multiple response options for each topic, selected randomly for varied interactions |
| **Sentiment Detection** | Detects user emotions (confused, frustrated, worried, curious, happy) and responds empathetically |
| **Conversation Flow** | Handles follow-up questions like "tell me more" and "another tip" |
| **Error Handling** | Provides helpful fallback messages for unrecognized inputs |
| **Code Organization** | Uses Object-Oriented Programming with separate classes for responses, user management, and voice |

## Project Structure
Cybersecurity_chatbot_p2/
├── MainWindow.xaml # GUI layout (home, username, chat grids)
├── MainWindow.xaml.cs # Main application logic
├── respond.cs # Response database and ignore words
├── user_name.cs # User name storage and recall
├── voice_greeting.cs # Voice greeting functionality
├── logo.png # Application logo (required)
├── greet.wav # Welcome sound file (required)
└── user_names.txt # Auto-generated user database


## Classes Overview

### MainWindow.xaml.cs
- Manages the GUI and user interactions
- Handles sentiment detection
- Processes user input and generates responses
- Manages conversation flow

### respond.cs
- Stores all cybersecurity responses in ArrayLists
- Contains ignore words list for filtering common words
- Provides multiple random responses per topic

### user_name.cs
- Saves user names to `user_names.txt`
- Recalls returning users
- Displays personalized welcome messages

### voice_greeting.cs
- Plays welcome sound on application startup
- Handles file path resolution for audio file

## Setup Instructions

### Prerequisites
- Visual Studio 2022 or later
- .NET 8.0 SDK
- Windows OS (for WPF and SoundPlayer)


### Supported Topics

| Topic | Keywords |
|-------|----------|
| Passwords | password, passwords |
| Scams | scam, scams |
| Phishing | phish, phishing |
| Privacy | privacy |
| Malware | malware |
| Social Engineering | social engineering |
| Two-Factor Authentication | 2fa, 2-factor |
| Ransomware | ransomware |
| Identity Theft | identity theft |
| VPN/Wi-Fi | vpn, wifi |

## Sentiment Responses

The chatbot detects and responds to user emotions:

| Sentiment | Example Response |
|-----------|------------------|
| Confused | "No worries - cybersecurity can be confusing at first. Let me explain it more simply." |
| Frustrated | "I know cybersecurity can be frustrating. Take a deep breath, and I'll help you understand this step by step." |
| Worried | "It's completely understandable to feel worried about online threats. Let me share some tips to help you stay safe." |
| Curious | "That's great! Being curious about cybersecurity is the first step to staying safe online." |
| Happy | "That's great to hear! Keep up the good cybersecurity practices!" |

## Error Handling

If the chatbot doesn't recognize a question, it responds with helpful suggestions:


## Known Issues and Solutions

| Issue | Solution |
|-------|----------|
| Voice greeting doesn't play | Ensure `greet.wav` is in the project root folder |
| Logo not displaying | Ensure `logo.png` is in the project root folder |
| Build errors | Make sure all .cs files are included in the project |
| Duplicate responses | The latest version ensures only one response per message |

## Future Enhancements (Part 3)

The following features are planned for Part 3:
- Interactive cybersecurity game
- Task list for security tips
- Advanced voice recognition
- More comprehensive topic database
- Progress tracking for users

## Technical Details

### Dependencies
- .NET 8.0 Windows
- System.Speech (for voice - optional)
- System.Media (for sound playback)

### Code Patterns Used
- Object-Oriented Programming (OOP)
- Generic Collections (ArrayList, List<string>)
- Regular Expressions for text processing
- Event-driven programming
- File I/O for user data persistence

## Author

- **Project:** Cybersecurity Awareness Chatbot
- **Target Audience:** South African Citizens
- **Purpose:** Educational tool for cybersecurity awareness

## Acknowledgments

- Department of Cybersecurity (project scenario)
- South African cybersecurity awareness initiatives

---

## Version History

| Version | Date | Description |
|---------|------|-------------|
| Part 1 | - | Command-line interface version |
| Part 2 | Current | WPF GUI with sentiment detection and memory |

## License

This project was developed for educational purposes as part of a cybersecurity awareness initiative.

YouTube Video Link: https://youtu.be/UgD94fkw79g
