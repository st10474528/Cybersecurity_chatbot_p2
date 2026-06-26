using System.Collections.Generic;

namespace Cybersecurity_chatbot_p2
{
    public class QuizQuestion
    {
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string Explanation { get; set; }

        public QuizQuestion(string text, List<string> options, int correctAnswerIndex, string explanation = "")
        {
            Text = text;
            Options = options;
            CorrectAnswerIndex = correctAnswerIndex;
            Explanation = explanation;
        }
    }
}