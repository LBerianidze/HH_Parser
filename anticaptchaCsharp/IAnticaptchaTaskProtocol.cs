using Anticaptcha.ApiResponse;
using Newtonsoft.Json.Linq;

namespace Anticaptcha
{
    public interface IAnticaptchaTaskProtocol
    {
        JObject GetPostData();
        TaskResultResponse.SolutionData GetTaskSolution();
    }
}