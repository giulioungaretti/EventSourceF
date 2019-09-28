using Microsoft.Extensions.Configuration;
using System;

namespace Test.Orleans.Patterns.EventSourcing
{
    public static partial class Extensions
    {
        public static string StringOrDefault(this string s, string defaultValue) =>
            string.IsNullOrWhiteSpace(s) ? defaultValue : s;

        public static int IntOrDefault(this string v, int defaultValue) =>
            (int.TryParse(v, out var result)) ? result : defaultValue;

        public static TEnum EnumOrDefault<TEnum>(this string v, TEnum defaultValue) where TEnum : struct =>
            (Enum.TryParse<TEnum>(v, out var result)) ? result : defaultValue;

    }

    public static partial class Extensions
    {
        public static string EventStorageConnectionString(this IConfiguration _this) =>
            _this?["ENV_EVENT_SOURCING_AZURE_STORAGE"]
                .StringOrDefault(
                    _this?["ENV_DEFAULT_EVENT_SOURCING_AZURE_STORAGE"]
                    .StringOrDefault("UseDevelopmentStorage=true"));
        public static string EventStorageTableName(this IConfiguration _this) =>
            _this?["ENV_EVENT_SOURCING_TABLE_NAME"]
                .StringOrDefault("events");
    }
}