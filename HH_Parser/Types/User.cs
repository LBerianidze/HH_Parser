using System;

namespace HH_Parser
{
    public class User
    {
        public int ID { get; set; }
        public string LoadedName { get; set; }
        public string SearchName { get; set; }
        public string Phone { get; set; }
        public string Mail { get; set; }
        public Resume LastResume { get; set; }
        public int ResumesCount { get; set; }
        public string ResumeFound { get; set; }
        /// <summary>
        /// Дата первого парсинга резюме пользователя.
        /// </summary>
        public DateTime FirstlyParsedDate { get; set; }
        public string Office { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string OldPhone { get; set; }
        public string OldMail { get; set; }
        //public List<Resume> Resumes { get; set; } = new List<Resume>();
        public int ParsedBy { get; set; }
    }
}
