namespace OnlyT.Report.Database
{
    using System;
    using System.Threading;
    using LiteDB;

    internal sealed class LocalDatabaseContext : IDisposable
    {
        private static readonly object Locker = new();

        public LocalDatabaseContext(string localDbFile)
        {
#pragma warning disable PH_P006 // Discouraged Monitor Method
            Monitor.Enter(Locker);
#pragma warning restore PH_P006 // Discouraged Monitor Method
            Db = new LiteDatabase(localDbFile);
        }

        public LiteDatabase Db { get; }

        public void Dispose()
        {
            Db.Dispose();
            Monitor.Exit(Locker);
        }
    }
}
