using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using OptimizeFactoryProgram.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuestionAnswerBot
{
    class Program
    {
        private static TelegramBotClient client;
        private static Dictionary<long, List<Guid>> QuestionsForUser = new Dictionary<long, List<Guid>> { };
        static void Main(string[] args)
        {
            {
                using var context = new QuestionContext();
                if (context.Database.Exists())
                {
                    context.Database.Delete();
                }
                context.Database.Create();
                context.Database.Initialize(true);
            }

            client = new TelegramBotClient("5413021783:AAGP9gFq8nbDRY2OERCEV-RAiXi-Abi9ZW0");
            client.OnMessage += (sender, e) =>
            {
                var handlers = new Dictionary<string, IMessageHandler>
                {
                    { "/start", new StartHandler() }
                };
                try
                {
                    handlers[e.Message.Text].Handle(client, e.Message.Chat.Id);
                }
                catch (KeyNotFoundException ex)
                {
                    client.SendTextMessageAsync(e.Message.Chat.Id, "Невалідна команда");
                }
                catch (ArgumentNullException ex)
                {}
                catch (Exception ex)
                {
                    client.SendTextMessageAsync(e.Message.Chat.Id, "Помилка");
                }
            };
            client.OnCallbackQuery += (sender, e) =>
            {

                var handlers = new Dictionary<string, IMessageHandler>
                {
                    { "startTesting", new StartQuestionsHandler()}
                };
                try
                {
                    handlers[e.CallbackQuery.Data].Handle(client, e.CallbackQuery.Message.Chat.Id);
                }
                catch (KeyNotFoundException ex)
                {
                }

            };
            client.OnUpdate += async (sender, e) =>
            {
                if (e.Update.Poll != null)
                {
                    try
                    {
                        using var context = new QuestionContext();
                        var d = context.Polls.Count();
                        var poll = context.Polls.Include("PollAnswers").Include("Question").Include("PollAnswers.Answer").First(x => x.PollId == e.Update.Poll.Id);
                        var progresses = context.UserProgresses.Include("Answers").Include("Answers.Answers").Include("Answers.Question").First(x => x.MessageId == poll.ChatId); var options = e.Update.Poll.Options;
                        {

                            var userAnswers = new List<Answer>();
                            for (int i = 0; i < options.Length; ++i)
                            {
                                if (options[i].VoterCount > 0)
                                {
                                    userAnswers.Add(poll.PollAnswers.First(x => x.Index == i).Answer);
                                }
                            }
                            var correctAnswers = poll.Question.RightAnswers.Where(x => userAnswers.Any(y => x.Id == y.Id)).Count();
                            var incorrectAnswers = poll.Question.WrongAnswers.Where(x => userAnswers.Any(y => x.Id == y.Id)).Count();

                            float diff = (correctAnswers - incorrectAnswers);
                            progresses.Answers.Add(new UserAnswer { Question = poll.Question, Score = diff == 0 ? 0 : Math.Clamp(diff / poll.Question.RightAnswers.Count, 0, 1), Answers = userAnswers });
                            context.SaveChanges();
                        }

                        if (QuestionsForUser[poll.ChatId].Count > 0)
                        {
                            new QuestionPassingHandler().Handle(context, client, poll.ChatId);
                        }
                        else
                        {
                            string result = "Ваші відповіді";
                            float mark = 0.0F;
                            int index = 0;
                            foreach (var progress in progresses.Answers)
                            {
                                var question = progress.Question;
                                var answers = progress.Answers;

                                var rightAnswers = string.Join("\n\t", question.RightAnswers.Where(x => answers.Contains(x)).Select(x => x.Text + "✅"));
                                var wrongAnswers = string.Join("\n\t", question.WrongAnswers.Where(x => answers.Contains(x)).Select(x => x.Text + "❌"));
                                result += $"\n\n{++index}) {question.Text}\n\t{string.Join("\n\t", (new string[] { rightAnswers, wrongAnswers }).Where(s => !string.IsNullOrEmpty(s)))}";
                                mark += progress.Score;
                            }
                            await client.SendTextMessageAsync(poll.ChatId, $"Бал {mark / progresses.Answers.Count * 100}%\n\n{result}");
                            new StartHandler().Handle(context, client, poll.ChatId);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            };
            client.StartReceiving();
            CreateHostBuilder(args).Build().Run();
        }

        interface IMessageHandler
        {
            void Handle(TelegramBotClient client, long chatId);
            void Handle(QuestionContext context, TelegramBotClient client, long chatId);
        };

        public abstract class BaseMessageHandler : IMessageHandler
        {
            public abstract void Handle(QuestionContext context, TelegramBotClient client, long chatId);

            public void Handle(TelegramBotClient client, long chatId)
            {
                using var context = new QuestionContext();
                Handle(context, client, chatId);
            }
        }

        class StartHandler
            : BaseMessageHandler
        {
            public override void Handle(QuestionContext context, TelegramBotClient client, long chatId)
            {
                context.UserProgresses.Add(new UserProgress { MessageId = chatId });
                context.SaveChanges();
                client.SendTextMessageAsync(chatId, "Почати тестування?", replyMarkup: new InlineKeyboardMarkup (new InlineKeyboardButton { Text = "Так", CallbackData = "startTesting" }));
            }
        }

        class StartQuestionsHandler
            : BaseMessageHandler
        {
            public override void Handle(QuestionContext context, TelegramBotClient client, long chatId)
            {
                if (QuestionsForUser.ContainsKey(chatId))
                {
                    QuestionsForUser.Remove(chatId);
                }
                {
                    Random rnd = new Random();
                    var questions = context.Questions.ToList().OrderBy((item) => rnd.Next()).Select(x => x.Id).ToList();
                    QuestionsForUser.Add(chatId, questions);

                    try
                    {
                        var progress = context.UserProgresses.Include("Answers").First(x => x.MessageId == chatId);
                        progress.Answers.Clear();
                        context.SaveChanges();
                    }
                    catch (Exception)
                    {

                    }
                }
                {

                    new QuestionPassingHandler().Handle(context, client, chatId);

                }
            }
        }

        class QuestionPassingHandler
            : BaseMessageHandler
        {
            private Answer[] RandomizeAnswers(Question question)
            {
                var orderedAnswers = question.RightAnswers.ToList();
                orderedAnswers.AddRange(question.WrongAnswers);

                Random rnd = new Random();
                return orderedAnswers.OrderBy((item) => rnd.Next()).ToArray();
            }
            private Poll CreatePoll(Question question, Answer[] answers)
            {

                var pollAnswers = new List<PollAnswer>();
                for (int i = 0; i < answers.Length; ++i)
                {
                    pollAnswers.Add(new PollAnswer
                    {
                        Answer = answers[i],
                        Index = i
                    });
                }

                return new Poll
                {
                    Question = question,
                    PollAnswers = pollAnswers
                };
            }
            public override void Handle(QuestionContext context, TelegramBotClient client, long chatId)
            {
                var questionsLeft = QuestionsForUser[chatId];
                var toRemove = questionsLeft.Last();
                var question = context.Questions.First(x => x.Id == toRemove);
                questionsLeft.Remove(toRemove);

                var answers = RandomizeAnswers(question);
                var pollDB = CreatePoll(question, answers);

                var poll = client.SendPollAsync(chatId, question.Text, answers.Select(x => x.Text), allowsMultipleAnswers: true);
                pollDB.ChatId = chatId;
                pollDB.PollId = poll.Result.Poll.Id;
                context.Polls.Add(pollDB);
                context.SaveChanges();
                var d = context.Polls.Count();
                var d1 = context.Polls.Count();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
