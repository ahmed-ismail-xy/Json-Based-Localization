using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace JsonBasedLocalization.Web
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly JsonSerializer _jsonSerializer = new JsonSerializer();
        public LocalizedString this[string name]
        {
            get
            {
                var value = GetString(name);
                return new LocalizedString(name, value);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var actualValue = this[name];
                return !actualValue.ResourceNotFound ? new LocalizedString(name, string.Format(actualValue.Value, arguments)) : actualValue;
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var filePath = $"Resources/{Thread.CurrentThread.CurrentCulture.Name}.json";

            using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using StreamReader reader = new StreamReader(stream);
            using JsonTextReader jsonTextReader = new JsonTextReader(reader);

            while (jsonTextReader.Read())
            {
               if(jsonTextReader.TokenType != JsonToken.PropertyName)
                    continue;

               var key = jsonTextReader.Value as string;
                
                jsonTextReader.Read();
                var value = _jsonSerializer.Deserialize<string>(jsonTextReader);
                
                yield return new LocalizedString(key, value);
            }
        }
        private string GetString(string key)
        {
            var filePath = $"Resources/{Thread.CurrentThread.CurrentCulture.Name}.json";
            var fileFullPath = Path.GetFullPath(filePath);

            if (File.Exists(fileFullPath))
                return GetValueFromJson(key, fileFullPath);
            
            return string.Empty;
        }
        private string GetValueFromJson(string propertyName, string filePath)
        {
            if (string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(filePath))
                return string.Empty;

            using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using StreamReader reader = new StreamReader(stream);
            using JsonTextReader jsonTextReader = new JsonTextReader(reader);


            while (jsonTextReader.Read())
            {
                if (jsonTextReader.TokenType == JsonToken.PropertyName && jsonTextReader.Value as string == propertyName)
                {
                    jsonTextReader.Read();
                    return _jsonSerializer.Deserialize<string>(jsonTextReader);
                }
            }

            return string.Empty;
        }
    }
}
