namespace AssistantAPI.Service
{
    public class AiClientSettings
    {
        public string AzureOpenAIEndpoint { get; set; }
        public string AzureOpenAIKey { get; set; }
        public string DeploymentName { get; set; }
        public string SearchEndpoint { get; set; }
        public string SearchKey { get; set; }
        public string SearchIndex { get; set; }
    }
}
