using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuestionAnswerBot
{
    public class Poll
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public long ChatId { get; set; }
        public string PollId { get; set; }
        public Question Question { get; set; }
        public virtual ICollection<PollAnswer> PollAnswers { get; set; }
    }
}
