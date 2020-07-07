using System;
using Umbraco.Core.Migrations;
using Umbraco.Core.Logging;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Our.Umbraco.FullTextSearch.Migrations
{
    public class CreateCacheTable : MigrationBase
    {
        public CreateCacheTable(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            Logger.Debug<CreateCacheTable>("Running migration {MigrationStep}", "CreateCacheTable");

            // Lots of methods available in the MigrationBase class - discover with this.
            if (TableExists("FullTextCache") == false)
            {
                Create.Table<CacheTableSchema>().Do();
            }
            else
            {
                Logger.Debug<CacheTableSchema>("The database table {DbTable} already exists, skipping", "FullTextCache");
            }
        }

        [TableName("FullTextCache")]
        [PrimaryKey("Id", AutoIncrement = true)]
        [ExplicitColumns]
        public class CacheTableSchema
        {
            [Column("Id")]
            [PrimaryKeyColumn(AutoIncrement = true)]
            public int Id { get; set; }

            [Column("NodeId")]
            public int NodeId { get; set; }

            [Column("Culture")]
            public string Culture { get; set; }

            [Column("Text")]
            [SpecialDbType(SpecialDbTypes.NTEXT)]
            public string Text { get; set; }

            [Column("LastUpdated")]
            public DateTime LastUpdated { get; set; }
        }
    }
}
