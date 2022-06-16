using System;
using System.ComponentModel.DataAnnotations;

namespace QuestionAnswerBot
{
    public class Answer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required(AllowEmptyStrings = false, ErrorMessage = "Питання має бути сформульованим")]
        public string Text { get; set; }
    }
}
