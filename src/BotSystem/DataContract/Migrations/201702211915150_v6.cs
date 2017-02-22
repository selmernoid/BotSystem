namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v6 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Posts", "AuthorId", "dbo.Users");
            DropIndex("dbo.Posts", new[] { "AuthorId" });
            AddColumn("dbo.CommentLinks", "UserName", c => c.String());
            AddColumn("dbo.UserComments", "UserName", c => c.String());
            AddColumn("dbo.Posts", "AuthorName", c => c.String());
            AddColumn("dbo.PostLinks", "UserName", c => c.String());
            AlterColumn("dbo.Posts", "AuthorId", c => c.Int());
            CreateIndex("dbo.Posts", "AuthorId");
            AddForeignKey("dbo.Posts", "AuthorId", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Posts", "AuthorId", "dbo.Users");
            DropIndex("dbo.Posts", new[] { "AuthorId" });
            AlterColumn("dbo.Posts", "AuthorId", c => c.Int(nullable: false));
            DropColumn("dbo.PostLinks", "UserName");
            DropColumn("dbo.Posts", "AuthorName");
            DropColumn("dbo.UserComments", "UserName");
            DropColumn("dbo.CommentLinks", "UserName");
            CreateIndex("dbo.Posts", "AuthorId");
            AddForeignKey("dbo.Posts", "AuthorId", "dbo.Users", "Id", cascadeDelete: true);
        }
    }
}
