namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v : DbMigration
    {
        public override void Up()
        {
//            DropPrimaryKey("dbo.UserComments");
            CreateTable(
                "dbo.Links",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CommentId = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        UserId = c.Int(),
                        PostId = c.Int(),
                        OtherCommentId = c.Int(),
                        Url = c.String(maxLength: 1024),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserComments", t => t.CommentId, cascadeDelete: true)
                .Index(t => t.CommentId)
                .Index(t => t.UserId)
                .Index(t => t.PostId)
                .Index(t => t.OtherCommentId)
                .Index(t => t.Url);
            
            CreateTable(
                "dbo.Posts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        Rating = c.Int(),
                        AuthorId = c.Int(nullable: false),
                        Content = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.AuthorId, cascadeDelete: true)
                .Index(t => t.AuthorId);
            
            AddColumn("dbo.UserComments", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.UserComments", "Id");
            CreateIndex("dbo.UserComments", "PostId");
            AddForeignKey("dbo.UserComments", "PostId", "dbo.Posts", "Id", cascadeDelete: true);
            DropColumn("dbo.UserComments", "CommentId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserComments", "CommentId", c => c.Int(nullable: false));
            DropForeignKey("dbo.UserComments", "PostId", "dbo.Posts");
            DropForeignKey("dbo.Posts", "AuthorId", "dbo.Users");
            DropForeignKey("dbo.Links", "CommentId", "dbo.UserComments");
            DropIndex("dbo.Posts", new[] { "AuthorId" });
            DropIndex("dbo.Links", new[] { "Url" });
            DropIndex("dbo.Links", new[] { "OtherCommentId" });
            DropIndex("dbo.Links", new[] { "PostId" });
            DropIndex("dbo.Links", new[] { "UserId" });
            DropIndex("dbo.Links", new[] { "CommentId" });
            DropIndex("dbo.UserComments", new[] { "PostId" });
            DropPrimaryKey("dbo.UserComments");
            DropColumn("dbo.UserComments", "Id");
            DropTable("dbo.Posts");
            DropTable("dbo.Links");
            AddPrimaryKey("dbo.UserComments", new[] { "UserId", "CommentId", "PostId", "Rating" });
        }
    }
}
