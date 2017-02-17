namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v5 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.CommentLinks", new[] { "UserId" });
            DropIndex("dbo.PostLinks", new[] { "UserId" });
            CreateIndex("dbo.CommentLinks", "UserId");
            CreateIndex("dbo.PostLinks", "UserId");
            AddForeignKey("dbo.PostLinks", "UserId", "dbo.Users", "Id");
            AddForeignKey("dbo.CommentLinks", "UserId", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CommentLinks", "UserId", "dbo.Users");
            DropForeignKey("dbo.PostLinks", "UserId", "dbo.Users");
            DropIndex("dbo.PostLinks", new[] { "UserId" });
            DropIndex("dbo.CommentLinks", new[] { "UserId" });
            CreateIndex("dbo.PostLinks", "UserId");
            CreateIndex("dbo.CommentLinks", "UserId");
        }
    }
}
