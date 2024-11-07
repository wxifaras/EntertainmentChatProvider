using EntertainmentChatApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using System.Net.Mime;
using EntertainmentChatApi.Services;
using Microsoft.SemanticKernel.ChatCompletion;
using EntertainmentChatApi.Interfaces;
using EntertainmentChatApi.Plugins;
using SqlDbSchemaExtractor.Schema;
using EntertainmentChatApi.Prompts;

namespace EntertainmentChatApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chat;
        private readonly IChatHistoryManager _chatHistoryManager;
        private readonly IAzureSqlDbService _azureDbService;
        private readonly string _connectionString;
        private readonly string _databaseDescription;
        private readonly string _tables;

        public ChatController(
           ILogger<ChatController> logger,
           IConfiguration configuration,
           Kernel kernel,
           IChatCompletionService chat,
           IChatHistoryManager chathistorymanager,
           IAzureSqlDbService azuredbservice)
        {
            _kernel = kernel;
            _chat = chat;
            _chatHistoryManager = chathistorymanager;
            _logger = logger;
            _azureDbService = azuredbservice;

            _connectionString = configuration.GetValue<string>("DatabaseConnection") ?? throw new ArgumentNullException("DatabaseConnection");
            _databaseDescription = configuration.GetValue<string>("DatabaseDescription") ?? throw new ArgumentNullException("DatabaseDescription");
            _tables = configuration.GetValue<string>("Tables") ?? throw new ArgumentNullException("Tables");
        }

        [HttpPost()]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] ChatProviderRequest chatRequest)
        {
            var response = new ChatProviderResponse();

            try
            {
                if (string.IsNullOrEmpty(chatRequest.SessionId))
                {
                    // needed for new chats
                    chatRequest.SessionId = Guid.NewGuid().ToString();
                }

                if (string.IsNullOrEmpty(chatRequest.Prompt))
                {
                    _logger.LogWarning("Chat request is missing prompt.");
                    return new BadRequestResult();
                }

                var sessionId = chatRequest.SessionId;
                var chatHistory = _chatHistoryManager.GetOrCreateChatHistory(sessionId);

                _kernel.ImportPluginFromObject(new DBQueryPlugin(_azureDbService));

                var jsonSchema = await GetDatabaseSchemaAsync();

                chatHistory.AddUserMessage(NLPSqlPluginPrompts.GetNLPToSQLSystemPrompt(jsonSchema));
                chatHistory.AddUserMessage(chatRequest.Prompt);

                ChatMessageContent? result = null;

                result = await _chat.GetChatMessageContentAsync(
                      chatHistory,
                      executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.8, TopP = 0.0, ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions },
                      kernel: _kernel);

                response.ChatResponse = result.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return StatusCode(500, "Internal server error.");
            }

            return new OkObjectResult(response);
        }

        private async Task<string> GetDatabaseSchemaAsync()
        {
            var sqlHarness = new SqlSchemaProviderHarness(_connectionString, _databaseDescription);
            var jsonSchema = string.Empty;
            var tableNames = _tables.Split("|");
            jsonSchema = await sqlHarness.ReverseEngineerSchemaJSONAsync(tableNames);

            return jsonSchema;
        }
    }
}
