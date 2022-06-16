using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace QuestionAnswerBot
{
    public class Question
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required(AllowEmptyStrings = false, ErrorMessage = "Питання має бути сформульованим")]
        public string Text { get; set; }
        [MinLength(1, ErrorMessage = "Має бути хоча б одна правильна відповідь")]
        public virtual ICollection<Answer> RightAnswers { get; set; }
        [MinLength(1, ErrorMessage = "Має бути хоча б одна неправильна відповідь")]
        public virtual ICollection<Answer> WrongAnswers { get; set; }
    }
}
