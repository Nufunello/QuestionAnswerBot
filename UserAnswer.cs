using System;
using System.Collections.Generic;

namespace QuestionAnswerBot
{
    public class UserAnswer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Question Question { get; set; } 
        public virtual ICollection<Answer> Answers { get; set; }
        public float Score { get; set; }
    }
}
