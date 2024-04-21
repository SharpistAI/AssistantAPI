using System.Text;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;

namespace AssistantAPI.Service
{
    public class AiClient
    {
        private readonly AiClientSettings _settings;
        private readonly OpenAIClient _client;

        public AiClient(IOptions<AiClientSettings> settingOptions)
        {
            _settings = settingOptions.Value;
            _client = new OpenAIClient(new Uri(_settings.AzureOpenAIEndpoint), new AzureKeyCredential(_settings.AzureOpenAIKey));
        }

        public async Task<ChatResponseMessage?> GenerateQuestionsAsync(string documentName, string keyword, QuestionType questionType)
        {
            var prompt = new StringBuilder(
                "**IN CONTEXT: I am a large language model trained to generate questions based on factual information. I can access and process information from Azure AI Search.**" +
                $"**You:** I have a document with title: {documentName} in Azure AI Search and about {keyword}. Can you generate {questionType} questions for this document?" +
                "**Azure Search:** (Here, Azure Search will return the content of the file)");
                           
            switch (questionType)
            {
                case QuestionType.ShortAnswer:
                    prompt.AppendLine("**With short-answer. Output following format without extra text:**");
                    prompt.AppendLine("1. Question 1");
                    prompt.AppendLine("     * Correct Answer:");
                    prompt.AppendLine("2. Question 2 (Follow the same format)");
                    prompt.AppendLine("3. Question 3 (Follow the same format)");
                    break;
                case QuestionType.FreeAnswer:
                    prompt.AppendLine($"**Based on the retrieved content, generate 3 {questionType} questions the following format without extra text:**");
                    prompt.AppendLine("1. Question 1");
                    prompt.AppendLine("2. Question 2");
                    prompt.AppendLine("3. Question 3");
                    break;
                case QuestionType.MultiChoice:
                default:
                    prompt.AppendLine($"**Based on the retrieved content, generate 3 {questionType} questions with answers the following format without extra text:**");
                    prompt.AppendLine("1. Question 1");
                    prompt.AppendLine("    * Answer choice A");
                    prompt.AppendLine("    * Answer choice B");
                    prompt.AppendLine("    * Answer choice C (One of these choices should be the correct answer)");
                    prompt.AppendLine("    * Correct Answer");
                    prompt.AppendLine("2. Question 2 (Follow the same format)");
                    prompt.AppendLine("3. Question 3 (Follow the same format)");
                    break;
            }

            return await GetChatResponseMessageAsync(prompt.ToString());
        }

        public async Task<ChatResponseMessage?> EvaluateAnswer(string documentTitle,string keyword,string question, string answer)
        {
            var prompt = new StringBuilder(
                "**IN CONTEXT: I am a large language model trained to evaluate user answers based on factual information. I can access and process information from Azure Search Service.**" +
                $"**You:** I have a document with title: {documentTitle} in Azure AI Search and about {keyword}. The user provided the following answer: \"{answer}\"" +
                "**Azure Search:** (Here, Azure Search will return the content of the file)" +
                $@"**Based on the retrieved content and the user answer, is the user answer true or false for this question: {question} the following format without any extra text:?**" +
                "True or False");

            return await GetChatResponseMessageAsync(prompt.ToString());
        }

        private async Task<ChatResponseMessage?> GetChatResponseMessageAsync(string prompt)
        {
            try
            {
                var chatCompletionsOptions = new ChatCompletionsOptions()
                {
                    Messages =
                    {
                        new ChatRequestUserMessage(prompt),
                    },
                    DeploymentName = _settings.DeploymentName,
                    AzureExtensionsOptions = new AzureChatExtensionsOptions()
                    {
                        Extensions =
                        {
                            new AzureSearchChatExtensionConfiguration()
                            {
                                IndexName = _settings.SearchIndex,
                                SearchEndpoint = new Uri(_settings.SearchEndpoint),
                                Authentication = new OnYourDataApiKeyAuthenticationOptions(_settings.SearchKey)

                            }
                        }
                    }
                };

                Response<ChatCompletions> response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);

                var chatChoice = response.Value.Choices.FirstOrDefault(chch => chch.FinishReason == CompletionsFinishReason.Stopped);
                if (chatChoice is not null)
                {
                    return chatChoice.Message;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return null;
        }
    }
}
