using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


///텔레그램 dll using
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


namespace WindowsChatbot
{
    class Program
    {
        //봇 생성
        private static readonly TelegramBotClient Bot = new TelegramBotClient("1048903631:AAG4wS_RIYJ2j93YdNh2oD_UJyMclv6Qrzk");
        //임시방편 유저 리스트
        private static List<User> Users = new List<User>();

        //임시방편 유저 리스트
        private static List<EngWord> Words = new List<EngWord>();


        //telegram 사용자의 상태 저장 변수
        private static Dictionary<long, UserState> dicUserState = new Dictionary<long, UserState>();

        //영어단어 저장 변수
        private static Dictionary<long, EngState> engword = new Dictionary<long, EngState>();

        static void Main(string[] args)
        {
            ///임시방편 유저 리스트에 유저 추가
            Users.Add(new User("윤지혜", 24));
            Users.Add(new User("고가영", 21));
            Users.Add(new User("강석천", 23));

            //영어단어 리스트 추가
            string[] lines = System.IO.File.ReadAllLines(@"C:\Users\on092\Desktop\Vocabulary 13000.txt");
            foreach (string show in lines)
            {
                Words.Add(new EngWord("{0}", show));
                Console.WriteLine("{0}", show);
            }
            Words.Add(new EngWord("the", "그, 그럴수록, 더욱더"));
            Words.Add(new EngWord("of", "~의, ~으로부터, ~을"));




            ///봇 이벤트 추가
            Bot.OnMessage += Bot_OnMessage;
            Bot.OnMessageEdited += Bot_OnMessage;
            Bot.OnReceiveError += Bot_OnReceiveError;

            var me = Bot.GetMeAsync().Result;

            Console.Title = me.Username;

            /// Recv Start
            Bot.StartReceiving();
            Console.ReadLine();
            /// Recv Stop
            Bot.StopReceiving();
        }

        /// Recv Error
        private static void Bot_OnReceiveError(object sender, Telegram.Bot.Args.ReceiveErrorEventArgs e)
        {
            Debugger.Break();
        }
        ///사용자로 부터 Message Recv
        private static async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs messageEventArgs)
        {
            /// Message 객체
            var message = messageEventArgs.Message;

            /// 예외처리
            if (message == null || message.Type != Telegram.Bot.Types.Enums.MessageType.Text)
            {
                return;
            }

            /// "/사용자추가" 라는 명령을 받음
            if (message.Text.StartsWith("/사용자추가"))
            {
                dicUserState[message.Chat.Id] = UserState.addUser;
                await Bot.SendTextMessageAsync(message.Chat.Id, @"사용자 이름과 나이를 입력해 주세요. ex)윈도우프로그래밍,28");
            }
            /// "/사용자삭제" 라는 명령을 받음
            else if (message.Text.StartsWith("/사용자삭제"))
            {
                dicUserState[message.Chat.Id] = UserState.deleteUser;
                await Bot.SendTextMessageAsync(message.Chat.Id, "사용자 이름을 입력해 주세요.");
            }

            /// "/사용자목록" 라는 명령을 받음
            else if (message.Text.StartsWith("/사용자목록"))
            {
                dicUserState[message.Chat.Id] = UserState.none;

                string _message = string.Empty;
                Users.ForEach(x => _message += string.Format("이름 : {0}, 나이 : {1}\r\n", x.Name, x.Age));

                await Bot.SendTextMessageAsync(message.Chat.Id, _message);
            }

            /// "/오늘의영어단어 명령
            else if (message.Text.StartsWith("/오늘의영어단어"))
            {
                engword[message.Chat.Id] = EngState.none;

                string _message = string.Empty;
                await Bot.SendTextMessageAsync(message.Chat.Id, "오늘의 영어단어 10개");
                //Words.ForEach(x => _message += string.Format("영어단어 : {0}\n 뜻 : {1}\r\n", x.Word, x.KorWord));
                Words.ForEach(x => _message += string.Format("영어단어 : {0}\n", x.Word));
                await Bot.SendTextMessageAsync(message.Chat.Id, _message);
            }

            /// "/도움말" 라는 명령을 받음
            else if (message.Text.StartsWith("/도움말"))
            {
                var usage = @"/사용자추가    - 사용자 추가 /사용자삭제 - 사용자 삭제 /사용자목록  - 사용자 목록 /도움말 - 도움말";
                await Bot.SendTextMessageAsync(message.Chat.Id, usage, replyMarkup: new ReplyKeyboardMarkup());
            }
            /// 그 외 다른 말을 받을 경우 사용자 상태를 보고 적절하게 대응한다.
            else
            {
                /// 예외처리
                if (!dicUserState.ContainsKey(message.Chat.Id))
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "먼 말인지 모르겠어요.");
                    return;
                }

                /// 사용자 상태가 사용자 추가일 경우
                if (dicUserState[message.Chat.Id] == UserState.addUser)
                {
                    /// 이름,나이 로 입력을 받을 것이기 때문에 , 로 tokenizing하자
                    /// 0번은 이름, 1번은 나이
                    string[] NameAndAge = message.Text.Split(',');

                    /// 쪼갰는데 개수가 2보다 작으면 잘못된 값을 받았다 판단
                    if (NameAndAge.Length < 2)
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, @"다시 입력해 주세요. ex)윈도우프로그래밍,28");
                        return;
                    }

                    /// 이름 나이 셋팅
                    string _name = NameAndAge[0];
                    int _age = 0;
                    bool result = Int32.TryParse(NameAndAge[1], out _age);

                    /// 나이값이 정상이면 추가
                    if (result)
                    {
                        // DB작업을 해야한다면 여기서 하면 될것같다.
                        Users.Add(new User(_name, _age));
                        await Bot.SendTextMessageAsync(message.Chat.Id, message.Text + " 사용자를 추가 했어요.");
                        dicUserState[message.Chat.Id] = UserState.none;
                    }
                    /// 나이값이 이상하면 예외
                    else
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, @"나이가 이상해요. 다시 입력해 주세요.");
                    }
                }

                /// 사용자 상태가 사용자 삭제일 경우
                else if (dicUserState[message.Chat.Id] == UserState.deleteUser)
                {
                    /// SingleOfDefault로 사용자를 찾았는데 없으면 예외처리 있으면 삭제
                    var _user = Users.SingleOrDefault(x => x.Name == message.Text);
                    if (_user != null)
                    {
                        // DB작업을 해야한다면 여기서 하면 될것같다.
                        Users.Remove(_user);
                        await Bot.SendTextMessageAsync(message.Chat.Id, message.Text + " 사용자를 삭제 했어요.");
                        dicUserState[message.Chat.Id] = UserState.none;
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id,
                            message.Text + @" 사용자가 없어요. 다시 입력해 주세요.");
                    }
                }

                /// 사용자 상태가 추가, 삭제가 아닌 경우 시치미 뚝
                else
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "먼 말인지 모르겠어요.");
                }


            }
        }
    }

    /// <summary>
    /// 사용자를 상태를 나타내는 enum
    /// </summary>
    public enum UserState
    {
        addUser,
        deleteUser,
        none
    }

    /// <summary>
    /// 사용자 클래스, 이름과 나이
    /// </summary>
    public class User
    {
        string name = string.Empty;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        int age = 0;
        public int Age
        {
            get { return age; }
            set { age = value; }
        }

        public User() : this("none", 0) { }
        public User(string name, int age)
        {
            this.Name = name;
            this.Age = age;
        }
    }

    public enum EngState
    {
        addUser,
        deleteUser,
        none
    }

    public class EngWord
    {
        string word = string.Empty;
        public string Word
        {
            get { return word; }
            set { word = value; }
        }

        string korWord = "";
        public string KorWord
        {
            get { return korWord; }
            set { korWord = value; }
        }

        public EngWord() : this("none", "") { }
        public EngWord(string word, string korWord)
        {
            this.Word = word;
            this.KorWord = korWord;
        }
    }
}


