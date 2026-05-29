using System.Collections;

namespace Cybersecurity_chatbot_p2
{
    public class respond
    {
        public respond(ArrayList reply, ArrayList ignore)
        {
            AddIgnoreWords(ignore);
            AddResponses(reply);
        }

        private void AddIgnoreWords(ArrayList ignoring)
        {
            string[] commonWords = new string[]
            {
                "a", "about", "above", "across", "after", "again", "against", "all", "almost", "alone",
                "along", "already", "also", "although", "always", "am", "among", "an", "and", "another",
                "any", "are", "around", "as", "at", "back", "be", "because", "been", "before", "behind",
                "being", "below", "between", "both", "but", "by", "can", "could", "did", "do", "does",
                "doing", "done", "down", "during", "each", "either", "else", "enough", "even", "ever",
                "every", "few", "first", "for", "from", "get", "give", "go", "had", "has", "have",
                "having", "he", "her", "here", "hers", "herself", "him", "himself", "his", "how",
                "however", "i", "if", "in", "into", "is", "it", "its", "itself", "just", "last", "least",
                "less", "let", "like", "likely", "may", "me", "might", "more", "most", "mostly", "must",
                "my", "myself", "never", "no", "nor", "not", "nothing", "now", "of", "off", "often",
                "on", "once", "only", "or", "other", "others", "otherwise", "our", "ours", "ourselves",
                "out", "over", "own", "please", "rather", "same", "see", "seem", "seemed", "seeming",
                "seems", "several", "she", "should", "since", "so", "some", "somehow", "someone",
                "something", "sometime", "sometimes", "somewhere", "still", "such", "take", "than",
                "that", "the", "their", "theirs", "them", "themselves", "then", "there", "therefore",
                "these", "they", "this", "those", "through", "throughout", "thru", "thus", "to", "together",
                "too", "toward", "towards", "under", "unless", "until", "up", "upon", "us", "used",
                "very", "via", "was", "we", "well", "were", "what", "whatever", "when", "where", "wherever",
                "whether", "which", "while", "who", "whoever", "whom", "whose", "why", "will", "with",
                "within", "without", "would", "yes", "yet", "you", "your", "yours", "yourself", "yourselves"
            };

            foreach (string word in commonWords)
            {
                ignoring.Add(word);
            }
        }

        private void AddResponses(ArrayList add_answers)
        {
            // Greetings
            AddResponse(add_answers, "greeting", "I'm doing well, thanks for asking! How are you doing today?");
            AddResponse(add_answers, "greeting", "I'm great today! How can I help you with cybersecurity?");
            AddResponse(add_answers, "greeting", "Doing good! Ready to learn about online safety?");

            // Passwords
            AddResponse(add_answers, "password", "Use strong, unique passwords for each account. Aim for at least 12 characters with a mix of letters, numbers, and symbols.");
            AddResponse(add_answers, "password", "Never share your passwords with anyone. Consider using a password manager like Bitwarden or LastPass.");
            AddResponse(add_answers, "password", "Enable two-factor authentication (2FA) whenever possible. This adds an extra layer of security.");
            AddResponse(add_answers, "password", "Avoid using personal information like your name, birthdate, or 'password123' in your passwords.");

            // Scams
            AddResponse(add_answers, "scam", "Scammers often create urgency. Slow down and verify before acting on any suspicious message.");
            AddResponse(add_answers, "scam", "Never click links in unsolicited emails or SMS messages. Type the website address directly into your browser.");
            AddResponse(add_answers, "scam", "If something sounds too good to be true, it probably is. Lottery wins and free money are common traps.");
            AddResponse(add_answers, "scam", "In South Africa, report scams to the SAPS Cybercrime Unit or SAFPS.");

            // Phishing
            AddResponse(add_answers, "phish", "Check email sender addresses carefully - scammers use addresses that look almost legitimate.");
            AddResponse(add_answers, "phish", "Hover over links before clicking to see the actual URL. Don't trust shortened links from unknown sources.");
            AddResponse(add_answers, "phish", "Legitimate companies will never ask for your password or OTP via email or phone.");
            AddResponse(add_answers, "phish", "Look for spelling mistakes and generic greetings like 'Dear Customer' - these are red flags.");

            // Privacy
            AddResponse(add_answers, "privacy", "Review your social media privacy settings regularly. Limit what you share publicly.");
            AddResponse(add_answers, "privacy", "Be careful what you post online - once it's on the internet, it's very hard to remove completely.");
            AddResponse(add_answers, "privacy", "Use a VPN when connecting to public Wi-Fi at coffee shops, malls, or airports in South Africa.");
            AddResponse(add_answers, "privacy", "Check app permissions on your phone. Does a flashlight app really need access to your contacts?");

            // Malware
            AddResponse(add_answers, "malware", "Keep your antivirus software updated and run regular scans on your devices.");
            AddResponse(add_answers, "malware", "Don't download software from unknown websites or click on pop-up ads claiming your computer is infected.");
            AddResponse(add_answers, "malware", "Be careful with USB drives from unknown sources - they can contain harmful malware.");
            AddResponse(add_answers, "malware", "Keep your operating system and apps updated to patch security vulnerabilities.");

            // Social Engineering
            AddResponse(add_answers, "social engineering", "Social engineers manipulate trust. Always verify who you're talking to before sharing information.");
            AddResponse(add_answers, "social engineering", "Be skeptical of unsolicited calls claiming to be from your bank. Hang up and call the official number.");
            AddResponse(add_answers, "social engineering", "Never give out your OTP (One-Time Pin) to anyone, even if they claim to be from 'support'.");
            AddResponse(add_answers, "social engineering", "Scammers may pretend to be friends or family in distress. Verify through another channel first.");

            // 2FA
            AddResponse(add_answers, "2fa", "Two-factor authentication adds an extra layer of security to your accounts.");
            AddResponse(add_answers, "2fa", "Always enable 2FA when available - it requires both your password and a code from your phone.");
            AddResponse(add_answers, "2fa", "2FA makes it much harder for hackers to access your accounts even if they have your password.");

            // Ransomware
            AddResponse(add_answers, "ransomware", "Ransomware is malware that locks your files and demands payment. Always backup your important data!");
            AddResponse(add_answers, "ransomware", "Never pay the ransom - there's no guarantee you'll get your files back.");
            AddResponse(add_answers, "ransomware", "Regular backups are your best defense against ransomware attacks.");

            // Identity Theft
            AddResponse(add_answers, "identity theft", "Identity theft happens when someone steals your personal information to commit fraud.");
            AddResponse(add_answers, "identity theft", "Monitor your bank statements regularly for unauthorized transactions.");
            AddResponse(add_answers, "identity theft", "Freeze your credit if you suspect identity theft to prevent new accounts being opened.");

            // VPN / Wi-Fi
            AddResponse(add_answers, "wifi", "Public Wi-Fi is often unencrypted. Avoid accessing sensitive accounts like banking when using it.");
            AddResponse(add_answers, "wifi", "Use a VPN when connecting to public Wi-Fi to encrypt your traffic.");
            AddResponse(add_answers, "vpn", "A VPN helps protect your privacy on public wi-fi by encrypting your internet traffic.");
            AddResponse(add_answers, "vpn", "A VPN hides your IP address and location from websites and hackers.");

            // Purpose
            AddResponse(add_answers, "purpose", "My purpose is to educate you on how to stay safe online and guide your cybersecurity questions.");
            AddResponse(add_answers, "purpose", "I help users understand online safety and digital protection in South Africa.");
            AddResponse(add_answers, "purpose", "I assist with cybersecurity awareness and safety guidance for everyday users.");

            // Cybersecurity definition
            AddResponse(add_answers, "cybersecurity", "Cybersecurity is about protecting systems and networks from digital threats.");
            AddResponse(add_answers, "cybersecurity", "It involves protecting devices and online accounts from attacks.");
            AddResponse(add_answers, "cybersecurity", "It focuses on securing digital information and systems.");

            // Firewall
            AddResponse(add_answers, "firewall", "A firewall controls network traffic based on security rules.");
            AddResponse(add_answers, "firewall", "It helps block unwanted access to your device or network.");
            AddResponse(add_answers, "firewall", "It acts as a protective barrier between trusted and untrusted networks.");

            // ============ SENTIMENT RESPONSES - FIXED ============

            // CONFUSED responses
            AddResponse(add_answers, "confused", "No worries - cybersecurity can be confusing at first. Let me explain it more simply.");
            AddResponse(add_answers, "confused", "I understand this topic can be confusing. Let me rephrase it in an easier way for you.");
            AddResponse(add_answers, "confused", "It's normal to feel confused about cybersecurity. Let me give you a clear, simple explanation.");

            // FRUSTRATED responses 
            AddResponse(add_answers, "frustrated", "I understand you're frustrated. Let's work through your cybersecurity concern together.");
            AddResponse(add_answers, "frustrated", "I know cybersecurity can be frustrating. Take a deep breath, and I'll help you understand this step by step.");
            AddResponse(add_answers, "frustrated", "It's okay to feel frustrated. Let me break this down in a simpler way for you.");

            // WORRIED responses
            AddResponse(add_answers, "worried", "It's completely understandable to feel worried about online threats. Let me share some tips to help you stay safe.");
            AddResponse(add_answers, "worried", "Don't panic - most cybersecurity issues can be prevented with good habits. I'm here to help you.");
            AddResponse(add_answers, "worried", "Your concern is valid. Let me explain how you can protect yourself from online threats.");

            // CURIOUS responses
            AddResponse(add_answers, "curious", "That's great! Being curious about cybersecurity is the first step to staying safe online.");
            AddResponse(add_answers, "curious", "I love that you want to learn more! Let me share some important information with you.");
            AddResponse(add_answers, "curious", "Excellent question! Curiosity helps you stay ahead of cyber threats.");

            // HAPPY responses
            AddResponse(add_answers, "happy", "That's great to hear! Keep up the good cybersecurity practices!");
            AddResponse(add_answers, "happy", "Awesome! Staying positive about online safety helps build good habits.");
            AddResponse(add_answers, "happy", "I'm glad to hear that! Remember to always stay vigilant online.");
        }

        private void AddResponse(ArrayList list, string keyword, string response)
        {
            list.Add(keyword + " " + response);
        }
    }
}