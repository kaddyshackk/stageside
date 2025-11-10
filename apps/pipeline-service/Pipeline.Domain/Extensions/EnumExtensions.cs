using System.ComponentModel;

namespace StageSide.Pipeline.Domain.Extensions
{
    public static class EnumExtensions
    {
        public static string GetEnumDescription(this Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());
            var attributes = fi?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            if (attributes != null && attributes.Length != 0)
            {
                return attributes.First().Description;
            }
            return value.ToString();
        }

        public static T? ParseFromDescription<T>(string description) where T : struct, Enum
        {
            return Enum.GetValues<T>()
                .FirstOrDefault(e => e.GetEnumDescription()
                    .Equals(description, StringComparison.OrdinalIgnoreCase));
        }

        public static T ParseFromDescriptionOrThrow<T>(string description) where T : struct, Enum
        {
            return ParseFromDescription<T>(description)
                ?? throw new ArgumentException($"Invalid {typeof(T).Name} value: '{description}'");
        }
    }
}