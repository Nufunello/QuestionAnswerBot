using System;
using System.Collections.Generic;

namespace QuestionAnswerBot
{
    public class UserProgress
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public long MessageId { get; set; }
        public ICollection<UserAnswer> Answers { get; set; }
    }
}
