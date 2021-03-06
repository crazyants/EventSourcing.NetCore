﻿using System;
using System.Data;
using Marten.Events;
using Npgsql;

namespace Marten.Integration.Tests.TestsInfrasructure
{
    public abstract class MartenTest : IDisposable
    {
        protected readonly IDocumentSession Session;

        protected IEventStore EventStore => Session.Events;

        protected readonly string SchemaName = "sch" + Guid.NewGuid().ToString().Replace("-",string.Empty);

        protected MartenTest() : this(true)
        {
        }

        protected MartenTest(bool shouldCreateSession)
        {
            if(shouldCreateSession)
                Session = CreateSession();
        }

        protected virtual IDocumentSession CreateSession(Action<StoreOptions> storeOptions = null)
        {
            return DocumentSessionProvider.Get(SchemaName, storeOptions);
        }

        public void Dispose()
        {
            Session?.Dispose();

            var sql = $"DROP SCHEMA {SchemaName} CASCADE;";
            using (var conn = new NpgsqlConnection(Settings.ConnectionString))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var command = conn.CreateCommand();
                    command.CommandText = sql;

                    command.ExecuteNonQuery();

                    tran.Commit();
                }
            }
        }
    }
}
