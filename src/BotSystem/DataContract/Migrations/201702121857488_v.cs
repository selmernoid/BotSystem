namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CommentLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CommentId = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        UserId = c.Int(),
                        OtherPostId = c.Int(),
                        OtherCommentId = c.Int(),
                        Url = c.String(maxLength: 890),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserComments", t => t.CommentId, cascadeDelete: true)
                .Index(t => t.CommentId)
                .Index(t => t.UserId)
                .Index(t => t.OtherPostId)
                .Index(t => t.OtherCommentId)
                .Index(t => t.Url);
            
            CreateTable(
                "dbo.UserComments",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        IsPostAuthor = c.Boolean(nullable: false),
                        ParentCommentId = c.Int(),
                        PostId = c.Int(nullable: false),
                        Rating = c.Int(),
                        Content = c.String(),
                        Created = c.DateTime(nullable: false),
                        Level = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserComments", t => t.ParentCommentId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ParentCommentId)
                .Index(t => t.PostId);
            
            CreateTable(
                "dbo.Posts",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Title = c.String(maxLength: 100),
                        Content = c.String(),
                        Rating = c.Int(),
                        AuthorId = c.Int(nullable: false),
                        IsLong = c.Boolean(nullable: false),
                        IsMine = c.Boolean(nullable: false),
                        Type = c.String(maxLength: 20),
                        CommentsCount = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        ProcessedTime = c.Time(precision: 7),
                        LastCheck = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.AuthorId, cascadeDelete: true)
                .Index(t => t.AuthorId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100, fixedLength: true),
                        Rating = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name);
            
            CreateTable(
                "dbo.PostLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PostId = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        UserId = c.Int(),
                        OtherPostId = c.Int(),
                        OtherCommentId = c.Int(),
                        Url = c.String(maxLength: 890),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => t.PostId)
                .Index(t => t.UserId)
                .Index(t => t.OtherPostId)
                .Index(t => t.OtherCommentId)
                .Index(t => t.Url);
            
            CreateTable(
                "dbo.PostTags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Level = c.String(maxLength: 50),
                        Logger = c.String(maxLength: 255),
                        Thread = c.String(maxLength: 255),
                        Message = c.String(maxLength: 4000),
                        Exception = c.String(maxLength: 2000),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PostTagPosts",
                c => new
                    {
                        PostTag_Id = c.Int(nullable: false),
                        Post_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PostTag_Id, t.Post_Id })
                .ForeignKey("dbo.PostTags", t => t.PostTag_Id, cascadeDelete: true)
                .ForeignKey("dbo.Posts", t => t.Post_Id, cascadeDelete: true)
                .Index(t => t.PostTag_Id)
                .Index(t => t.Post_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserComments", "PostId", "dbo.Posts");
            DropForeignKey("dbo.PostTagPosts", "Post_Id", "dbo.Posts");
            DropForeignKey("dbo.PostTagPosts", "PostTag_Id", "dbo.PostTags");
            DropForeignKey("dbo.PostLinks", "PostId", "dbo.Posts");
            DropForeignKey("dbo.UserComments", "UserId", "dbo.Users");
            DropForeignKey("dbo.Posts", "AuthorId", "dbo.Users");
            DropForeignKey("dbo.CommentLinks", "CommentId", "dbo.UserComments");
            DropForeignKey("dbo.UserComments", "ParentCommentId", "dbo.UserComments");
            DropIndex("dbo.PostTagPosts", new[] { "Post_Id" });
            DropIndex("dbo.PostTagPosts", new[] { "PostTag_Id" });
            DropIndex("dbo.PostLinks", new[] { "Url" });
            DropIndex("dbo.PostLinks", new[] { "OtherCommentId" });
            DropIndex("dbo.PostLinks", new[] { "OtherPostId" });
            DropIndex("dbo.PostLinks", new[] { "UserId" });
            DropIndex("dbo.PostLinks", new[] { "PostId" });
            DropIndex("dbo.Users", new[] { "Name" });
            DropIndex("dbo.Posts", new[] { "AuthorId" });
            DropIndex("dbo.UserComments", new[] { "PostId" });
            DropIndex("dbo.UserComments", new[] { "ParentCommentId" });
            DropIndex("dbo.UserComments", new[] { "UserId" });
            DropIndex("dbo.CommentLinks", new[] { "Url" });
            DropIndex("dbo.CommentLinks", new[] { "OtherCommentId" });
            DropIndex("dbo.CommentLinks", new[] { "OtherPostId" });
            DropIndex("dbo.CommentLinks", new[] { "UserId" });
            DropIndex("dbo.CommentLinks", new[] { "CommentId" });
            DropTable("dbo.PostTagPosts");
            DropTable("dbo.Logs");
            DropTable("dbo.PostTags");
            DropTable("dbo.PostLinks");
            DropTable("dbo.Users");
            DropTable("dbo.Posts");
            DropTable("dbo.UserComments");
            DropTable("dbo.CommentLinks");
        }
    }
}
