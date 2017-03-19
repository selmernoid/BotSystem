namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v8 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.PostTags", "Name");
        }
        
        public override void Down()
        {
            DropIndex("dbo.PostTags", new[] { "Name" });
        }
    }
}
