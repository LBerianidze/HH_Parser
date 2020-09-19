using Newtonsoft.Json;

namespace ParentChildProcessRelation
{
    public class ParserSettings
    {
        public Type RequestType { get; set; }
        public string Proxy
        {
            get
            {
                if(string.IsNullOrEmpty(ProxyName))
                {
                    return string.Format("{0}:{1}", ProxyIP, ProxyPort );

                }
                return string.Format("{0}:{1}:{2}:{3}", ProxyIP, ProxyPort, ProxyName, ProxyPassword);
            }
            set
            {
                string[] sp = value.Split(':');
                ProxyIP = sp[0];
                ProxyPort = sp[1];
                if (sp.Length == 4)
                {
                    ProxyName = sp[2];
                    ProxyPassword = sp[3];
                }
            }
        }
        public string ProxyIP { get; set; }
        public string ProxyPort { get; set; }
        public string ProxyName { get; set; }
        public string ProxyPassword { get; set; }
        public string UserAgent { get; set; } = "";
        public string RequestURL { get; set; } = "";
        public string Script { get; set; }
        public bool DisableImageDisplay { get; set; }
        public LoadType LoadType { get; set; }
        public string CaptchaKey { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
        public static ParserSettings Create(string Json)
        {
            return JsonConvert.DeserializeObject<ParserSettings>(Json);
        }
    }
    public enum Type
    {
        Initialize,
        Load,
        SolveYandexCaptcha,
        ExecuteScript,
        GetSource,
        EnterCaptcha
    }
    public enum LoadType
    {
        interactive,
        complete
    }
}
