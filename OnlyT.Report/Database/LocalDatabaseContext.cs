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
            Monitor.Enter(Locker);
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
