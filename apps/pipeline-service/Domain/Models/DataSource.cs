using System.ComponentModel;

namespace ComedyPull.Domain.Models
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
