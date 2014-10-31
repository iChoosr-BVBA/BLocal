using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using BLocal.Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace BLocal.Providers
{
    /// <summary>
    /// Provides localization and logging based off of a Microsoft SQL Database
    /// </summary>
    public class MSDBValueAndLogProvider : ILocalizedValueManager, ILocalizedValueProvider, ILocalizationLogger
    {
        private readonly String _connectionString;
        private readonly String _databaseName;
        private readonly Thread _flushLog;
        private readonly TableConfiguration _tableConfiguration;

        private PartTable _partTable;
        private LocaleTable _localeTable;
        private ValueTable _valueTable;
        private LogTable _logTable;
        private readonly bool _insertDummyValues;
        
        /// <summary>
        /// Creates a new MSDBValueAndLogProvider
        /// </summary>
        /// <param name="connectionString">ConnectionString needed to connect to the database</param>
        /// <param name="partTableName">Name of the table to store parts in (gets created if it doesn't exist)</param>
        /// <param name="localeTableName">Name of the table to store locales in (gets created if it doesn't exist)</param>
        /// <param name="valueTableName">Name of the table to store values in (gets created if it doesn't exist)</param>
        /// <param name="logTableName">Name of the table to store logs in (gets created if it doesn't exist)</param>
        /// <param name="schema">Schema to find or create all the tables in (null = default schema)</param>
        /// <param name="insertDummyValues">If set to true, will not throw ValueNotFoundExceptions (which trigger the Notifier), but create dummy values instead.</param>
        public MSDBValueAndLogProvider(String connectionString, String partTableName, String localeTableName, String valueTableName, String logTableName, String schema = null, bool insertDummyValues = false)
        {
            // instantly test connection & get database name
            _connectionString = connectionString;
            using (var conn = new SqlConnection(connectionString)) {
                conn.Open();
                _databaseName = conn.Database;
                schema = schema ?? GetDatabase(GetServer(conn)).DefaultSchema;
            }

            _tableConfiguration = new TableConfiguration(partTableName, localeTableName, valueTableName, logTableName, schema);
            _insertDummyValues = insertDummyValues;

            Reload();
            _flushLog = new Thread(FlushLog);
            _flushLog.Start();
        }

        public string GetValue(Qualifier.Unique qualifier, String defaultValue = null)
        {
            var part = _partTable.GetPart(qualifier.Part.ToString());
            var locale = _localeTable.GetLocale(qualifier.Locale.Name);

            if (part == null || locale == null)
            {
                using (var connection = Connect())
                {
                    if (part == null)
                        part = _partTable.Insert(qualifier.Part, connection);
                    if (locale == null)
                        locale = _localeTable.Insert(qualifier.Locale.Name, connection);
                }
            }

            var value = _valueTable.GetValue(part, locale, qualifier.Key);
            if(value == null && defaultValue != null)
                using(var connection = Connect())
                    value = _valueTable.Insert(new ValueTable.DBValue(part.Id, locale.Id, qualifier.Key, defaultValue), connection);

            if (value == null && _insertDummyValues)
                using (var connection = Connect())
                    value = _valueTable.Insert(new ValueTable.DBValue(part.Id, locale.Id, qualifier.Key, "[-" + qualifier.Key + "-]"), connection);

            return value == null ? null : value.Content;
        }

        public void SetValue(Qualifier.Unique qualifier, string value)
        {
            var part = _partTable.GetPart(qualifier.Part.ToString());
            var locale = _localeTable.GetLocale(qualifier.Locale.Name);

            using(var connection = Connect()) {
                _valueTable.Update(new InternalQualifier(part.Id, locale.Id, qualifier.Key), value, connection);
            }
        }

        public void UpdateCreateValue(QualifiedValue value)
        {
            var part = _partTable.GetPart(value.Qualifier.Part.ToString());
            var locale = _localeTable.GetLocale(value.Qualifier.Locale.Name);

            if(part == null || locale == null)
            {
                using(var connection = Connect())
                {
                    if (part == null)
                        part = _partTable.Insert(value.Qualifier.Part, connection);
                    if (locale == null)
                        locale = _localeTable.Insert(value.Qualifier.Locale.Name, connection);
                }
            }

            var dbval = new ValueTable.DBValue(part.Id, locale.Id, value.Qualifier.Key, value.Value);

            using(var connection = Connect())
                _valueTable.UpdateCreate(dbval, connection);
        }

        public QualifiedValue GetQualifiedValue(Qualifier.Unique qualifier, String defaultValue = null)
        {
            var part = _partTable.GetPart(qualifier.Part.ToString());
            var locale = _localeTable.GetLocale(qualifier.Locale.Name);

            if (part == null || locale == null)
            {
                using (var connection = Connect())
                {
                    if (part == null)
                        part = _partTable.Insert(qualifier.Part, connection);
                    if (locale == null)
                        locale = _localeTable.Insert(qualifier.Locale.Name, connection);
                }
            }

            var qualifiedValue = _valueTable.GetQualifiedValue(part, locale, qualifier.Key);
            if (qualifiedValue == null && defaultValue != null)
                using (var connection = Connect())
                    qualifiedValue = new QualifiedValue(
                        qualifier,
                        _valueTable.Insert(new ValueTable.DBValue(part.Id, locale.Id, qualifier.Key, defaultValue), connection).Content
                    );

            if (qualifiedValue == null && _insertDummyValues)
                using (var connection = Connect())
                    qualifiedValue = new QualifiedValue(
                        qualifier,
                        _valueTable.Insert(new ValueTable.DBValue(part.Id, locale.Id, qualifier.Key, "[-" + qualifier.Key + "-]"), connection).Content
                    );
            
            return qualifiedValue;
        }

        public void Reload()
        {
            using (var connector = Connect()) {
                var connection = connector.Connection;
                var server = GetServer(connection);
                var database = GetDatabase(server);
                if (!database.Schemas.Contains(_tableConfiguration.Schema)) {
                    var schema = new Schema(database, _tableConfiguration.Schema);
                    database.Schemas.Add(schema);
                    schema.Create();
                }

                _partTable = new PartTable(connection, database, _tableConfiguration);
                _localeTable = new LocaleTable(connection, database, _tableConfiguration);
                _valueTable = new ValueTable(connection, database, _tableConfiguration);
                _logTable = new LogTable(database, _tableConfiguration);
            }
        }

        public void CreateValue(Qualifier.Unique qualifier, string value)
        {
            var part = _partTable.GetPart(qualifier.Part.ToString());
            var locale = _localeTable.GetLocale(qualifier.Locale.Name);

            if (part == null || locale == null) {
                using (var connection = Connect()) {
                    if (part == null)
                        part = _partTable.Insert(qualifier.Part, connection);
                    if (locale == null)
                        locale = _localeTable.Insert(qualifier.Locale.Name, connection);
                }
            }

            var qualifiedValue = _valueTable.GetQualifiedValue(part, locale, qualifier.Key);
            if (qualifiedValue == null || !qualifiedValue.Qualifier.Equals(qualifier))
                using(var connector = Connect())
                    _valueTable.Insert(new ValueTable.DBValue(part.Id, locale.Id, qualifier.Key, value), connector);
        }

        public IEnumerable<QualifiedValue> GetAllValuesQualified()
        {
            return _valueTable.GetQualifiedValues(_partTable, _localeTable).Select(v => new QualifiedValue(v.Qualifier, v.Value));
        }

        public void Persist()
        {
            // all values are already persisted
        }

        public IEnumerable<LocalizationAudit> GetAudits()
        {
            return Enumerable.Empty<LocalizationAudit>();
        }

        public void SetAudits(IEnumerable<LocalizationAudit> audits)
        {
            
        }

        public void DeleteValue(Qualifier.Unique qualifier)
        {
            using(var connection = Connect())
                _valueTable.Delete(new InternalQualifier(
                    _partTable.GetPart(qualifier.Part.ToString()).Id,
                    _localeTable.GetLocale(qualifier.Locale.ToString()).Id,
                    qualifier.Key
                ), connection);
        }

        public void DeleteLocalizationsFor(Part part, String key)
        {
            using (var connection = Connect()) {
                var partId = _partTable.GetPart(part.ToString()).Id;
                foreach (var locale in _localeTable.GetAll())
                    _valueTable.Delete(new InternalQualifier(partId, locale.Id, key), connection);
            }

        }

        public void Log(Qualifier.Unique accessedQualifier)
        {
            _logTable.LogAsync(new InternalQualifier(
                _partTable.GetPart(accessedQualifier.Part.ToString()).Id,
                _localeTable.GetLocale(accessedQualifier.Locale.ToString()).Id,
                accessedQualifier.Key
            ));
        }

        public IEnumerable<Log> GetLogsBetween(DateTime start, DateTime end)
        {
            using (var connection = Connect())
                return _logTable.GetLogs(start, end, connection)
                    .Select(log => log.GetLog(_partTable, _localeTable));
        }

        public IDictionary<Qualifier.Unique, Log> GetLatestLogsBetween(DateTime start, DateTime end)
        {
            using (var connection = Connect())
                return _logTable.GetLatestLogs(start, end, connection)
                    .Select(log => log.GetLog(_partTable, _localeTable))
                    .ToDictionary(log => log.Qualifier);
        }

        public Dictionary<int, String> GetLocales()
        {
            var locales = _localeTable.GetAll();
            return locales.ToDictionary(locale => locale.Id, locale => locale.Name);
        } 

        private Database GetDatabase(Server server)
        {
            if (!server.Databases.Contains(_databaseName))
            {
                var newDB = new Database(server, _databaseName);
                server.Databases.Add(newDB);
                newDB.Create();
                return newDB;
            }
            return server.Databases[_databaseName];
        }

// ReSharper disable FunctionNeverReturns
        private void FlushLog()
        {
            while(true)
            {
                using(var connection = Connect())
                    _logTable.ClearLog(connection);
                Thread.Sleep(1000);
            }   
        }
// ReSharper restore FunctionNeverReturns

        private static Server GetServer(SqlConnection connection)
        {
            return new Server(new ServerConnection(connection));
        }

        private Connector Connect()
        {
            return new Connector(_connectionString);
        }

        private class PartTable
        {
            public const String IdColumn = "Id";
            public const String NameColumn = "Name";
            public const String ParentIdColumn = "ParentId";
            private readonly Table _table;
            private readonly Dictionary<string, DBPart> _partsByQualifier = new Dictionary<string, DBPart>();
            private readonly Dictionary<long, DBPart> _partsById = new Dictionary<long, DBPart>();

            public PartTable(SqlConnection connection, Database database, TableConfiguration configuration)
            {
                _table = database.Tables.Cast<Table>().SingleOrDefault(table => table.ToString() == configuration.PartTableQualifier)
                    ?? CreateTable(database, configuration);

                Validate();
                ReloadParts(connection);
            }

            private void Validate()
            {
                var idColumn = _table.Columns[IdColumn];
                var nameColumn = _table.Columns[NameColumn];
                var parentIdColumn = _table.Columns[ParentIdColumn];

                if (idColumn == null || idColumn.DataType.Name != DataType.BigInt.Name)
                    throw new InvalidArgumentException("Table does not contain column " + IdColumn + " or column is not " + DataType.BigInt);
                if (nameColumn == null || nameColumn.DataType.Name != DataType.VarChar(500).Name)
                    throw new InvalidArgumentException("Table does not contain column " + NameColumn + " or column is not " + DataType.VarChar(500));
                if (parentIdColumn == null || parentIdColumn.DataType.Name != DataType.BigInt.Name || !parentIdColumn.Nullable)
                    throw new InvalidArgumentException("Table does not contain column " + ParentIdColumn + " or column is not " + DataType.BigInt + " or column is not nullable");
            }

            private static Table CreateTable(Database database, TableConfiguration configuration)
            {
                var table = new Table(database, configuration.PartTableName, configuration.Schema);
                table.Columns.Add(new Column(table, IdColumn, DataType.BigInt) {Identity = true});
                table.Columns.Add(new Column(table, NameColumn, DataType.VarChar(500)));
                table.Columns.Add(new Column(table, ParentIdColumn, DataType.BigInt) { Nullable = true });

                var primaryKey = new Index(table, "PK_" + configuration.PartTableName) { IndexKeyType = IndexKeyType.DriPrimaryKey };
                primaryKey.IndexedColumns.Add(new IndexedColumn(primaryKey, IdColumn));
                table.Indexes.Add(primaryKey);

                var uniqueKey = new Index(table, "UQ_" + configuration.PartTableName + "_" + ParentIdColumn + "_" + NameColumn) { IndexKeyType = IndexKeyType.DriUniqueKey };
                uniqueKey.IndexedColumns.Add(new IndexedColumn(uniqueKey, ParentIdColumn));
                uniqueKey.IndexedColumns.Add(new IndexedColumn(uniqueKey, NameColumn));
                table.Indexes.Add(uniqueKey);

                var selfLink = new ForeignKey(table, "FK_" + configuration.PartTableName + "_" + configuration.PartTableName) {
                    ReferencedTable = configuration.PartTableName,
                    ReferencedTableSchema = configuration.Schema
                };
                selfLink.Columns.Add(new ForeignKeyColumn(selfLink, ParentIdColumn, IdColumn));
                table.ForeignKeys.Add(selfLink);

                database.Tables.Add(table);
                table.Create();
                return table;
            }

            private void ReloadParts(SqlConnection connection)
            {
                _partsById.Clear();
                _partsByQualifier.Clear();

                var command = new SqlCommand(String.Format(
                    "select {0}, {1} from {3} where {2} IS NULL",
                    IdColumn, NameColumn, ParentIdColumn, _table
                ), connection);

                var unexploredParts = new Dictionary<long, DBPart>();

                using (var result = command.ExecuteReader()) {
                    if (!result.HasRows)
                        return;

                    while (result.Read()) {
                        var part = new DBPart(result.GetInt64(0), result.GetString(1), null);
                        unexploredParts.Add(part.Id, part);
                        _partsByQualifier.Add(part.ToString().ToLower(), part);
                        _partsById.Add(part.Id, part);
                    }
                }

                var innerCommand = new SqlCommand(String.Format(
                    "select {0}, {1} from {3} where {2} = @parentId",
                    IdColumn, NameColumn, ParentIdColumn, _table
                ), connection);

                while (unexploredParts.Count > 0) {
                    var unexploredPart = unexploredParts.First();
                    unexploredParts.Remove(unexploredPart.Key);

                    innerCommand.Parameters.Clear();
                    innerCommand.Parameters.Add(new SqlParameter("parentId", unexploredPart.Key));

                    using(var innerResult = innerCommand.ExecuteReader()) {
                        if (!innerResult.HasRows)
                            continue;

                        while (innerResult.Read()) {
                            var part = new DBPart(innerResult.GetInt64(0), innerResult.GetString(1),
                                                  unexploredPart.Value);
                            unexploredParts.Add(part.Id, part);
                            _partsByQualifier.Add(part.ToString().ToLower(), part);
                            _partsById.Add(part.Id, part);
                        }
                    }
                }
            }

            public DBPart GetPart(String partQualifier)
            {
                try
                {
                    return _partsByQualifier[partQualifier.ToLower()];
                }
                catch(KeyNotFoundException)
                {
                    return null; 
                }
            }

            public DBPart Insert(Part part, Connector connector)
            {
                var connection = connector.Connection;
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                var partsToCreate = new List<Part>();
                while (part != null && GetPart(part.ToString()) == null)
                {
                    partsToCreate.Add(part);
                    part = part.Parent;
                }
                partsToCreate.Reverse();

                var command = new SqlCommand(String.Format(
                        "delete from {0} where {1} = @name and {2} = @parentId; insert into {0}({1}, {2}) values(@name, @parentId); select IDENT_CURRENT('{0}')",
                        _table, NameColumn, ParentIdColumn
                    ), connection
                );

                var lastCreatedPartId = part == null ? null : ((int?)GetPart(part.ToString()).Id);
                var lastCreatedPart = lastCreatedPartId == null ? null : _partsById[lastCreatedPartId.Value];
                foreach(var partToCreate in partsToCreate)
                {
                    command.Parameters.Clear();

                    command.Parameters.Add(new SqlParameter("name", SqlDbType.VarChar) { Value = partToCreate.Name });
                    command.Parameters.Add(new SqlParameter("parentId", SqlDbType.BigInt) { Value = (Object)lastCreatedPartId ?? DBNull.Value });

                    var newPart = new DBPart(Int32.Parse(command.ExecuteScalar().ToString()), partToCreate.Name, lastCreatedPart);
                    _partsById.Add(newPart.Id, newPart);
                    _partsByQualifier.Add(newPart.ToString().ToLower(), newPart);
                    lastCreatedPart = newPart;
                }

                return lastCreatedPart;
            }

            public class DBPart : Part
            {
                public long Id { get; private set; }
                public DBPart(long id, String name, Part parent)
                    : base(name, parent)
                {
                    Id = id;
                }
            }

            public Part GetPart(long partId)
            {
                return _partsById[partId];
            }
        }

        private class LocaleTable
        {
            public const String IdColumn = "Id";
            public const String NameColumn = "Name";
            private readonly Table _table;
            private readonly Dictionary<string, DBLocale> _localesByName = new Dictionary<string, DBLocale>();
            private readonly Dictionary<int, DBLocale> _localesById = new Dictionary<int, DBLocale>();

            public LocaleTable(SqlConnection connection, Database database, TableConfiguration configuration)
            {
                _table = database.Tables.Cast<Table>().SingleOrDefault(table => table.ToString() == configuration.LocaleTableQualifier)
                    ?? CreateTable(database, configuration);

                Validate();
                ReloadLocales(connection);
            }

            private void Validate()
            {
                var idColumn = _table.Columns[IdColumn];
                var nameColumn = _table.Columns[NameColumn];

                if (idColumn == null || idColumn.DataType.Name != DataType.Int.Name)
                    throw new InvalidArgumentException("Table does not contain column " + IdColumn + " or column is not " + DataType.Int);
                if (nameColumn == null || nameColumn.DataType.Name != DataType.VarCharMax.Name)
                    throw new InvalidArgumentException("Table does not contain column " + NameColumn + " or column is not " + DataType.VarCharMax);
            }

            private static Table CreateTable(Database database, TableConfiguration configuration)
            {
                var table = new Table(database, configuration.LocaleTableName, configuration.Schema);
                table.Columns.Add(new Column(table, IdColumn, DataType.Int) { Identity = true });
                table.Columns.Add(new Column(table, NameColumn, DataType.VarChar(500)));

                var primaryKey = new Index(table, "PK_" + configuration.LocaleTableName) { IndexKeyType = IndexKeyType.DriPrimaryKey };
                primaryKey.IndexedColumns.Add(new IndexedColumn(primaryKey, IdColumn));
                table.Indexes.Add(primaryKey);

                var uniqueKey = new Index(table, "UQ_" + configuration.LocaleTableName + "_" + NameColumn) { IndexKeyType = IndexKeyType.DriUniqueKey };
                uniqueKey.IndexedColumns.Add(new IndexedColumn(uniqueKey, NameColumn));
                table.Indexes.Add(uniqueKey);

                database.Tables.Add(table);
                table.Create();
                return table;
            }

            private void ReloadLocales(SqlConnection connection)
            {
                _localesByName.Clear();
                _localesById.Clear();

                var command = new SqlCommand(String.Format(
                    "select {0}, {1} from {2}",
                    IdColumn, NameColumn, _table
                ), connection);

                using (var result = command.ExecuteReader()) {
                    if (result.HasRows) {
                        while (result.Read()) {
                            var locale = new DBLocale(result.GetInt32(0), result.GetString(1));
                            _localesByName.Add(locale.Name, locale);
                            _localesById.Add(locale.Id, locale);
                        }
                    }
                }
            }

            public DBLocale GetLocale(String name)
            {
                try
                {
                    return _localesByName[name];
                }
                catch(KeyNotFoundException)
                {
                    return null;
                }
            }

            public Locale GetLocale(int localeId)
            {
                return _localesById[localeId];
            }

            public DBLocale Insert(String name, Connector connector)
            {
                var connection = connector.Connection;
                if(connection.State != ConnectionState.Open)
                    connection.Open();

                var command = new SqlCommand(String.Format(
                        "insert into {0}({1}) values(@name); select IDENT_CURRENT('{0}')",
                        _table, NameColumn
                    ), connection
                );

                command.Parameters.Add(new SqlParameter("name", SqlDbType.VarChar) { Value = name });
                var newLocale = new DBLocale(Int32.Parse(command.ExecuteScalar().ToString()), name);
                _localesByName[newLocale.Name] = newLocale;
                _localesById[newLocale.Id] = newLocale;
                return newLocale;
            }

            public IEnumerable<DBLocale> GetAll()
            {
                return _localesById.Values;
            }

            public class DBLocale : Locale
            {
                public readonly int Id;

                public DBLocale(int id, String name) : base(name)
                {
                    Id = id;
                }

                public bool Equals(DBLocale other)
                {
                    if (ReferenceEquals(null, other)) return false;
                    if (ReferenceEquals(this, other)) return true;
                    return base.Equals(other) && other.Id == Id;
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    return Equals(obj as DBLocale);
                }

                public override int GetHashCode()
                {
                    return Id;
                }
            }
        }

        private class ValueTable
        {
            public const String PartIdColumn = "PartId";
            public const String LocaleIdColumn = "LocaleId";
            public const String KeyColumn = "QualifierKey";
            public const String ContentColumn = "Content";
            private readonly Table _table;
            private readonly Dictionary<InternalQualifier, DBValue> _valuesByQualifier = new Dictionary<InternalQualifier, DBValue>();

            public ValueTable(SqlConnection connection, Database database, TableConfiguration configuration)
            {
                _table = database.Tables.Cast<Table>().SingleOrDefault(table => table.ToString() == configuration.ValueTableQualifier)
                    ?? CreateTable(database, configuration);

                Validate();
                ReloadValues(connection);
            }

            private void Validate()
            {
                var partIdColumn = _table.Columns[PartIdColumn];
                var localeIdColumn = _table.Columns[LocaleIdColumn];
                var keyColumn = _table.Columns[KeyColumn];
                var contentColumn = _table.Columns[ContentColumn];

                if (partIdColumn == null || partIdColumn.DataType.Name != DataType.BigInt.Name)
                    throw new InvalidArgumentException("Table does not contain column " + PartIdColumn + " or column is not " + DataType.BigInt);
                if (localeIdColumn == null || localeIdColumn.DataType.Name != DataType.Int.Name)
                    throw new InvalidArgumentException("Table does not contain column " + LocaleIdColumn + " or column is not " + DataType.Int);
                if (keyColumn == null || keyColumn.DataType.Name != DataType.VarChar(500).Name)
                    throw new InvalidArgumentException("Table does not contain column " + KeyColumn + " or column is not " + DataType.VarCharMax);
                if (contentColumn == null || contentColumn.DataType.Name != DataType.VarCharMax.Name)
                    throw new InvalidArgumentException("Table does not contain column " + ContentColumn + " or column is not " + DataType.VarCharMax);
            }

            private static Table CreateTable(Database database, TableConfiguration configuration)
            {
                var table = new Table(database, configuration.ValueTableName, configuration.Schema);
                table.Columns.Add(new Column(table, PartIdColumn, DataType.BigInt));
                table.Columns.Add(new Column(table, LocaleIdColumn, DataType.Int));
                table.Columns.Add(new Column(table, KeyColumn, DataType.VarChar(500)));
                table.Columns.Add(new Column(table, ContentColumn, DataType.VarCharMax));

                var primaryKey = new Index(table, "PK_" + configuration.ValueTableName) { IndexKeyType = IndexKeyType.DriPrimaryKey };
                primaryKey.IndexedColumns.Add(new IndexedColumn(primaryKey, PartIdColumn));
                primaryKey.IndexedColumns.Add(new IndexedColumn(primaryKey, LocaleIdColumn));
                primaryKey.IndexedColumns.Add(new IndexedColumn(primaryKey, KeyColumn));
                table.Indexes.Add(primaryKey);

                database.Tables.Add(table);
                table.Create();
                var partLink = new ForeignKey(table, "FK_" + configuration.ValueTableName + "_" + configuration.PartTableName) {
                    ReferencedTable = configuration.PartTableName,
                    ReferencedTableSchema = configuration.Schema
                };
                partLink.Columns.Add(new ForeignKeyColumn(partLink, PartIdColumn, PartTable.IdColumn));
                table.ForeignKeys.Add(partLink);
                partLink.Create();

                var localeLink = new ForeignKey(table, "FK_" + configuration.ValueTableName + "_" + configuration.LocaleTableName) {
                    ReferencedTable = configuration.LocaleTableName,
                    ReferencedTableSchema = configuration.Schema
                };
                localeLink.Columns.Add(new ForeignKeyColumn(localeLink, LocaleIdColumn, LocaleTable.IdColumn));
                table.ForeignKeys.Add(localeLink);
                localeLink.Create();

                return table;
            }

            private void ReloadValues(SqlConnection connection)
            {
                _valuesByQualifier.Clear();

                var command = new SqlCommand(String.Format(
                    "select {0}, {1}, {2}, {3} from {4}",
                    PartIdColumn, LocaleIdColumn, KeyColumn, ContentColumn, _table
                ), connection);
                
                using(var result = command.ExecuteReader()){
                    if (!result.HasRows) return;

                    while (result.Read()) {
                        var value = new DBValue(result.GetInt64(0), result.GetInt32(1), result.GetString(2), result.GetString(3));
                        _valuesByQualifier.Add(value.Qualifier, value);
                    }
                }
            }

            public DBValue GetValue(PartTable.DBPart part, LocaleTable.DBLocale locale, String key)
            {
                try
                {
                    return _valuesByQualifier[new InternalQualifier(part.Id, locale.Id, key)];
                }
                catch(KeyNotFoundException)
                {
                    var parent = part.Parent as PartTable.DBPart;
                    if (parent != null)
                        return GetValue(parent, locale, key);
                }

                return null;
            }

            public QualifiedValue GetQualifiedValue(PartTable.DBPart part, LocaleTable.DBLocale locale, String key)
            {
                try {
                    return new QualifiedValue(new Qualifier.Unique(part, locale, key), _valuesByQualifier[new InternalQualifier(part.Id, locale.Id, key)].Content);
                }
                catch (KeyNotFoundException) {
                    var parent = part.Parent as PartTable.DBPart;
                    if (parent != null)
                        return GetQualifiedValue(parent, locale, key);
                }
                return null;
            }

            public IEnumerable<QualifiedValue> GetQualifiedValues(PartTable partTable, LocaleTable localeTable)
            {
                return _valuesByQualifier
                    .Select(qv => new QualifiedValue(
                        new Qualifier.Unique(
                            partTable.GetPart(qv.Key.PartId),
                            localeTable.GetLocale(qv.Key.LocaleId),
                            qv.Key.Key
                        ),
                        qv.Value.Content
                    )).ToList();
            }

            public void Update(InternalQualifier qualifier, string value, Connector connector)
            {
                var connection = connector.Connection;
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                if (_valuesByQualifier[qualifier].Content.Equals(value))
                    return;

                _valuesByQualifier[qualifier].Content = value;

                var command = new SqlCommand(String.Format(
                        "update {0} set {1} = @content where {2} = @part and {3} = @locale and {4} = @key",
                        _table, ContentColumn, PartIdColumn, LocaleIdColumn, KeyColumn
                    ), connection
                );

                command.Parameters.Add(new SqlParameter("content", SqlDbType.VarChar) {Value = value});
                command.Parameters.Add(new SqlParameter("part", SqlDbType.BigInt) {Value = qualifier.PartId});
                command.Parameters.Add(new SqlParameter("locale", SqlDbType.Int) { Value = qualifier.LocaleId });
                command.Parameters.Add(new SqlParameter("key", SqlDbType.VarChar) { Value = qualifier.Key });
                command.ExecuteNonQuery();
            }

            public void UpdateCreate(DBValue dbval, Connector connector)
            {
                var connection = connector.Connection;
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                if (_valuesByQualifier.ContainsKey(dbval.Qualifier)) {
                    var command = new SqlCommand(String.Format(
                            "update {0} set {1} = @content where {2} = @part and {3} = @locale and {4} = @key",
                            _table, ContentColumn, PartIdColumn, LocaleIdColumn, KeyColumn
                        ), connection
                    );

                    command.Parameters.Add(new SqlParameter("content", SqlDbType.VarChar) { Value = dbval.Content });
                    command.Parameters.Add(new SqlParameter("part", SqlDbType.BigInt) { Value = dbval.Qualifier.PartId });
                    command.Parameters.Add(new SqlParameter("locale", SqlDbType.Int) { Value = dbval.Qualifier.LocaleId });
                    command.Parameters.Add(new SqlParameter("key", SqlDbType.VarChar) { Value = dbval.Qualifier.Key });

                    if (command.ExecuteNonQuery() > 0)
                        _valuesByQualifier[dbval.Qualifier] = dbval;
                }
                else {
                    var command = new SqlCommand(String.Format(
                            "insert into {0}({1}, {2}, {3}, {4}) values(@content, @part, @locale, @key)",
                            _table, ContentColumn, PartIdColumn, LocaleIdColumn, KeyColumn
                        ), connection
                    );

                    command.Parameters.Add(new SqlParameter("content", SqlDbType.VarChar) { Value = dbval.Content });
                    command.Parameters.Add(new SqlParameter("part", SqlDbType.BigInt) { Value = dbval.Qualifier.PartId });
                    command.Parameters.Add(new SqlParameter("locale", SqlDbType.Int) { Value = dbval.Qualifier.LocaleId });
                    command.Parameters.Add(new SqlParameter("key", SqlDbType.VarChar) { Value = dbval.Qualifier.Key });

                    if(command.ExecuteNonQuery() > 0)
                        _valuesByQualifier.Add(dbval.Qualifier, dbval);
                }
            }

            public DBValue Insert(DBValue value, Connector connector)
            {
                var connection = connector.Connection;
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                _valuesByQualifier.Add(value.Qualifier, value);

                var command = new SqlCommand(String.Format(
                        "insert into {0}({1}, {2}, {3}, {4}) values(@part, @locale, @key, @content)",
                        _table, PartIdColumn, LocaleIdColumn, KeyColumn, ContentColumn
                    ), connection
                );

                command.Parameters.Add(new SqlParameter("part", SqlDbType.BigInt) { Value = value.Qualifier.PartId });
                command.Parameters.Add(new SqlParameter("locale", SqlDbType.Int) { Value = value.Qualifier.LocaleId });
                command.Parameters.Add(new SqlParameter("key", SqlDbType.VarChar) { Value = value.Qualifier.Key });
                command.Parameters.Add(new SqlParameter("content", SqlDbType.VarChar) { Value = value.Content });
                command.ExecuteNonQuery();
                return value;
            }

            public void Delete(InternalQualifier qualifier, Connector connector)
            {
                var connection = connector.Connection;
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                var command = new SqlCommand(String.Format(
                        "delete from {0} where {1} = @part and {2} = @locale and {3} = @key",
                        _table, PartIdColumn, LocaleIdColumn, KeyColumn
                    ), connection
                );

                command.Parameters.Add(new SqlParameter("part", SqlDbType.BigInt) { Value = qualifier.PartId });
                command.Parameters.Add(new SqlParameter("locale", SqlDbType.Int) { Value = qualifier.LocaleId });
                command.Parameters.Add(new SqlParameter("key", SqlDbType.VarChar) { Value = qualifier.Key });
                command.ExecuteNonQuery();

                if (_valuesByQualifier.ContainsKey(qualifier))
                    _valuesByQualifier.Remove(qualifier);
            }

            public class DBValue
            {
                public InternalQualifier Qualifier { get; private set; }
                public String Content;

                public DBValue(long partId, int localeId, String key, String content)
                {
                    Qualifier = new InternalQualifier(partId, localeId, key);
                    Content = content;
                }
            }
        }

        private class TemporaryQualifier 
        {
            public InternalQualifier Qualifer { get; set; }
            public DateTime Time { get; set; }

            public TemporaryQualifier(InternalQualifier qualifer, DateTime time)
            {
                Qualifer = qualifer;
                Time = time;
            }
        }

        private class LogTable
        {
            public const String PartIdColumn = "PartId";
            public const String LocaleIdColumn = "LocaleId";
            public const String KeyColumn = "QualifierKey";
            public const String DateColumn = "Date";

            private readonly Table _table;
            private readonly List<TemporaryQualifier> _tmpQualifiers = new List<TemporaryQualifier>(); 

            public LogTable(Database database, TableConfiguration configuration)
            {
                _table = database.Tables.Cast<Table>().SingleOrDefault(table => table.ToString() == configuration.LogTableQualifier)
                    ?? CreateTable(database, configuration);

                Validate();
            }

            private void Validate()
            {
                var partIdColumn = _table.Columns[PartIdColumn];
                var localeIdColumn = _table.Columns[LocaleIdColumn];
                var keyColumn = _table.Columns[KeyColumn];
                var dateColumn = _table.Columns[DateColumn];

                if (partIdColumn == null || partIdColumn.DataType.Name != DataType.BigInt.Name)
                    throw new InvalidArgumentException("Table does not contain column " + PartIdColumn + " or column is not " + DataType.BigInt);
                if (localeIdColumn == null || localeIdColumn.DataType.Name != DataType.Int.Name)
                    throw new InvalidArgumentException("Table does not contain column " + LocaleIdColumn + " or column is not " + DataType.Int);
                if (keyColumn == null || keyColumn.DataType.Name != DataType.VarChar(500).Name)
                    throw new InvalidArgumentException("Table does not contain column " + KeyColumn + " or column is not " + DataType.VarChar(500));
                if (dateColumn == null || dateColumn.DataType.Name != DataType.DateTime.Name)
                    throw new InvalidArgumentException("Table does not contain column " + dateColumn + " or column is not " + DataType.DateTime);
            }

            private static Table CreateTable(Database database, TableConfiguration configuration)
            {
                var table = new Table(database, configuration.LogTableName, configuration.Schema);
                table.Columns.Add(new Column(table, PartIdColumn, DataType.BigInt));
                table.Columns.Add(new Column(table, LocaleIdColumn, DataType.Int));
                table.Columns.Add(new Column(table, KeyColumn, DataType.VarChar(500)));
                table.Columns.Add(new Column(table, DateColumn, DataType.DateTime));

                var index = new Index(table, "IX_" + configuration.LogTableName) { IndexKeyType = IndexKeyType.None };
                index.IndexedColumns.Add(new IndexedColumn(index, PartIdColumn));
                index.IndexedColumns.Add(new IndexedColumn(index, LocaleIdColumn));
                index.IndexedColumns.Add(new IndexedColumn(index, KeyColumn));
                table.Indexes.Add(index);

                database.Tables.Add(table);
                table.Create();
                var partLink = new ForeignKey(table, "FK_" + configuration.LogTableName + "_" + configuration.PartTableName) {
                    ReferencedTable = configuration.PartTableName,
                    ReferencedTableSchema = configuration.Schema
                };
                partLink.Columns.Add(new ForeignKeyColumn(partLink, PartIdColumn, PartTable.IdColumn));
                table.ForeignKeys.Add(partLink);
                partLink.Create();

                var localeLink = new ForeignKey(table, "FK_" + configuration.LogTableName + "_" + configuration.LocaleTableName) {
                    ReferencedTable = configuration.LocaleTableName,
                    ReferencedTableSchema = configuration.Schema
                };
                localeLink.Columns.Add(new ForeignKeyColumn(localeLink, LocaleIdColumn, LocaleTable.IdColumn));
                table.ForeignKeys.Add(localeLink);
                localeLink.Create();

                return table;
            }

            public void LogAsync(InternalQualifier qualifier)
            {
                lock(_tmpQualifiers)
                    _tmpQualifiers.Add(new TemporaryQualifier(qualifier, DateTime.Now));
            }

            public void ClearLog(Connector connector)
            {
                lock(_tmpQualifiers)
                    if (!_tmpQualifiers.Any())
                        return;

                var connection = connector.Connection;
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                lock (_tmpQualifiers)
                {
                    foreach (var log in _tmpQualifiers)
                    {
                        var command = new SqlCommand(String.Format(
                            "insert into {0}({1}, {2}, {3}, {4}) values(@part, @locale, @key, @date)",
                            _table, PartIdColumn, LocaleIdColumn, KeyColumn, DateColumn
                        ), connection);

                        command.Parameters.Add(new SqlParameter("part", SqlDbType.BigInt) {Value = log.Qualifer.PartId});
                        command.Parameters.Add(new SqlParameter("locale", SqlDbType.Int) {Value = log.Qualifer.LocaleId});
                        command.Parameters.Add(new SqlParameter("key", SqlDbType.VarChar) {Value = log.Qualifer.Key});
                        command.Parameters.Add(new SqlParameter("date", SqlDbType.DateTime) {Value = log.Time});
                        command.ExecuteNonQuery();
                    }
                    _tmpQualifiers.Clear();
                }
            }

            public List<DBLog> GetLogs(DateTime start, DateTime end, Connector connector)
            {
                var connection = connector.Connection;
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                var command = new SqlCommand(String.Format(
                    "select {0}, {1}, {2}, {3} from {4} where {3} between @start and @end",
                    PartIdColumn, LocaleIdColumn, KeyColumn, DateColumn, _table
                ), connection);
                command.Parameters.Add(new SqlParameter("start", SqlDbType.Date) { Value = start });
                command.Parameters.Add(new SqlParameter("end", SqlDbType.Date) { Value = end });

                using(var result = command.ExecuteReader()) {
                    if (!result.HasRows)
                        return new List<DBLog>(0);

                    var logs = new List<DBLog>(result.RecordsAffected);
                    while (result.Read()) {
                        var value = new DBLog(result.GetInt64(0), result.GetInt32(1), result.GetString(2), result.GetDateTime(3));
                        logs.Add(value);
                    }
                    return logs;
                }
            }

            public IEnumerable<DBLog> GetLatestLogs(DateTime start, DateTime end, Connector connector)
            {
                var connection = connector.Connection;
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                var command = new SqlCommand(String.Format(
                    "select {0}, {1}, {2}, max({3}) from {4} where {3} between @start and @end group by {0}, {1}, {2}",
                    PartIdColumn, LocaleIdColumn, KeyColumn, DateColumn, _table
                ), connection);
                command.Parameters.Add(new SqlParameter("start", SqlDbType.DateTime) { Value = start });
                command.Parameters.Add(new SqlParameter("end", SqlDbType.DateTime) { Value = end });

                using (var result = command.ExecuteReader())
                {
                    if (!result.HasRows)
                        return new List<DBLog>(0);

                    var logs = new List<DBLog>(result.RecordsAffected < 0 ? 0 : result.RecordsAffected);
                    while (result.Read()) {
                        var value = new DBLog(result.GetInt64(0), result.GetInt32(1), result.GetString(2), result.GetDateTime(3));
                        logs.Add(value);
                    }
                    return logs;
                }
            }

            public class DBLog
            {
                public readonly long PartId;
                public readonly int LocaleId;
                public readonly string Key;
                public readonly DateTime Date;

                public DBLog(long partId, int localeId, String key, DateTime date)
                {
                    PartId = partId;
                    LocaleId = localeId;
                    Key = key;
                    Date = date;
                }

                public Log GetLog(PartTable parts, LocaleTable locales)
                {
                    return new Log(new Qualifier.Unique(parts.GetPart(PartId), locales.GetLocale(LocaleId), Key), Date);
                }
            }
        }

        private class InternalQualifier
        {
            public readonly long PartId;
            public readonly int LocaleId;
            public readonly String Key;
            private readonly String _lowerKey;

            public InternalQualifier(long partId, int localeId, string key)
            {
                PartId = partId;
                LocaleId = localeId;
                Key = key;
                _lowerKey = key.ToLowerInvariant();
            }

            public bool Equals(InternalQualifier other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return other.PartId == PartId && other.LocaleId == LocaleId && Equals(_lowerKey, other._lowerKey);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (InternalQualifier)) return false;
                return Equals((InternalQualifier) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var result = PartId.GetHashCode();
                    result = (result*397) ^ LocaleId;
                    result = (result * 397) ^ (_lowerKey != null ? _lowerKey.GetHashCode() : 0);
                    return result;
                }
            }
        }

        private class Connector : IDisposable
        {
            private readonly string _connectionString;
            private SqlConnection _connection;
            public SqlConnection Connection { get { return _connection ?? (_connection = new SqlConnection(_connectionString)); } }

            public Connector(String connectionString)
            {
                _connectionString = connectionString;
            }

            public void Dispose()
            {
                if(_connection != null)
                    _connection.Dispose();
                _connection = null;
            }
        }

        private class TableConfiguration
        {
            public readonly String PartTableName;
            public readonly String LocaleTableName;
            public readonly String ValueTableName;
            public readonly String LogTableName;

            public readonly String PartTableQualifier;
            public readonly String LocaleTableQualifier;
            public readonly String ValueTableQualifier;
            public readonly String LogTableQualifier;

            public readonly String Schema;

            public TableConfiguration(string partTableName, string localeTableName, string valueTableName, string logTableName, string schema)
            {
                PartTableName = partTableName;
                LocaleTableName = localeTableName;
                ValueTableName = valueTableName;
                LogTableName = logTableName;
                Schema = schema;
                PartTableQualifier = FormatTable(partTableName);
                LocaleTableQualifier = FormatTable(localeTableName);
                ValueTableQualifier = FormatTable(valueTableName);
                LogTableQualifier = FormatTable(logTableName);
            }

            private String FormatTable(String tableName)
            {
                return String.Format("[{0}].[{1}]", Schema, tableName);
            }
        }
    }
}
