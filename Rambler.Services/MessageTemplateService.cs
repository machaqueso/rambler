using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using Rambler.Models;

namespace Rambler.Services
{
    public class MessageTemplateService
    {
        public class Globals
        {
            public dynamic author;
        }

        private readonly ILogger<MessageTemplateService> logger;

        public MessageTemplateService(ILogger<MessageTemplateService> logger)
        {
            this.logger = logger;
        }
        

        //public async Task<string> Interpolate(string template, Author author)
        //{
        //    var globals = new Globals
        //    {
        //        author = new
        //        {
        //            name = author.Name,
        //            score = author.Score
        //        }
        //    };

        //    var result = await CSharpScript.EvaluateAsync<string>("return $\"" + template + "\";", globals: globals);
        //    logger.LogDebug($"Interpolate result = {result}");
        //    return result;
        //}
        
        public async Task<string> Interpolate(string template, Author author)
        {
            var result = template;
            if (result.Contains("{"))
            {
                result = result.Replace("{author.name}", author.Name);
                result = result.Replace("{author.score}", author.Score.ToString());
            }

            return result;
        }
    }
}