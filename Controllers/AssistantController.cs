using AssistantAPI.Service;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Polly.Retry;

namespace AssistantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssistantController(AiClient client, AsyncRetryPolicy retryPolicy) : ControllerBase
    {
        [HttpPost]
        public async Task<ChatResponseMessage?> GetQuestionsAndAnswer(string documentName, QuestionType questionType)
        {
            try
            {
                var response = await retryPolicy.ExecuteAsync(async () =>
                {
                    var responseMessage = await client.GenerateQuestionsAsync(documentName, questionType);
                    return responseMessage;
                });

                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        [HttpPost]
        public async Task<ChatResponseMessage?> EvaluateAnswer(string documentTitle, string userAnswer)
        {
            try
            {
                var response = await retryPolicy.ExecuteAsync(async () =>
                {
                    var responseMessage = await client.EvaluateAnswer(documentTitle, userAnswer);
                    return responseMessage;
                });

                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
    }
}
