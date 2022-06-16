using QuestionAnswerBot;
using System.Collections.Generic;
using System.Data.Entity;

namespace OptimizeFactoryProgram.Context
{
    public class QuestionContext : DbContext
    {
        public class Initer<T> : DropCreateDatabaseAlways<QuestionContext>
        {
            protected override void Seed(QuestionContext context)
            {
                var questions = new List<Question>
                {
                    new Question{Text = "Початок другої світової війни:",
                        RightAnswers = new List<Answer>{ new Answer { Text = "1 вересня 1939-го" } },
                        WrongAnswers = new List<Answer>{ new Answer { Text = "22 червня 1941-го" }, new Answer { Text = "23 серпня 1939-го" }, new Answer { Text = "28 червня 1914" } }
                    },
                    new Question{Text = "Які з цих істот існують?",
                        RightAnswers = new List<Answer>{ new Answer { Text = "Тасманійський диявол" }, new Answer { Text = "Манул" } },
                        WrongAnswers = new List<Answer>{ new Answer { Text = "Єдиноріг" }, new Answer { Text = "Хороший росіянин" } }
                    },
                    new Question{Text = "1 + 1?",
                        RightAnswers = new List<Answer>{ new Answer { Text = "2" } },
                        WrongAnswers = new List<Answer>{ new Answer { Text = "1" }, new Answer { Text = "Я гуманітарій" }, new Answer { Text = "3" }}
                    },
                    new Question{Text = "2 + 2?",
                        RightAnswers = new List<Answer>{ new Answer { Text = "4" } },
                        WrongAnswers = new List<Answer>{ new Answer { Text = "5" }, new Answer { Text = "0" }, new Answer { Text = "16" }}
                    },
                    new Question{Text = "Що з цього є філосовськими течіями?",
                        RightAnswers = new List<Answer>{ new Answer { Text = "Екзистенціалізм" }, new Answer { Text = "Метафізика" }, new Answer { Text = "Дуалізм" } },
                        WrongAnswers = new List<Answer>{ new Answer { Text = "Нарцисизм" } }
                    }
                };
                context.Questions.AddRange(questions);
                context.SaveChanges();
            }
        }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollAnswer> PollAnswers { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<UserProgress> UserProgresses { get; set; }
        public QuestionContext()
            : base("Server=localhost;Trusted_Connection=True;Database=QuestionTelegramBot;")
        {
            Database.SetInitializer(new Initer<QuestionContext>());
        }
    }
}
