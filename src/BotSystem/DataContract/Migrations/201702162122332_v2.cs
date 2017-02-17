namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2 : DbMigration
    {
        public override void Up() {
            SqlResource("DataContract.Sql.DefaultSettings.sql");
        }
        
        public override void Down()
        {
        }
    }
}
