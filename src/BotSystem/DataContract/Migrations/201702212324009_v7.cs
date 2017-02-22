namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v7 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.UserComments", new[] { "UserId" });
            AlterColumn("dbo.UserComments", "UserId", c => c.Int());
            CreateIndex("dbo.UserComments", "UserId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserComments", new[] { "UserId" });
            AlterColumn("dbo.UserComments", "UserId", c => c.Int(nullable: false));
            CreateIndex("dbo.UserComments", "UserId");
        }
    }
}
