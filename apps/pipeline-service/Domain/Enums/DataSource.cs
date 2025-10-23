using System.ComponentModel;

namespace ComedyPull.Domain.Enums
{
    public enum DataSource
    {
        [Description(DataSourceKeys.Punchup)]
        Punchup
    }

    public static class DataSourceKeys
    {
        public const string Punchup = "punchup";
    }
}
