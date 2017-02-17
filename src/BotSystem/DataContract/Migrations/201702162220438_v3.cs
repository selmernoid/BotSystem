namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Posts", "Title", c => c.String(maxLength: 142));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Posts", "Title", c => c.String(maxLength: 100));
        }
    }
}
