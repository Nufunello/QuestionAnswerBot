using System;

namespace QuestionAnswerBot
{
    public class PollAnswer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Index { get; set; } 
        public Answer Answer { get; set; }
    }
}
