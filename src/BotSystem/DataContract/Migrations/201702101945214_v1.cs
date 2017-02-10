namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v1 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Links", newName: "CommentLinks");
            DropIndex("dbo.CommentLinks", new[] { "PostId" });
            DropIndex("dbo.CommentLinks", new[] { "Url" });
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
            
            AddColumn("dbo.CommentLinks", "OtherPostId", c => c.Int());
            AddColumn("dbo.Posts", "ProcessedTime", c => c.Time(nullable: false, precision: 7));
            AlterColumn("dbo.CommentLinks", "Url", c => c.String(maxLength: 890));
            CreateIndex("dbo.CommentLinks", "OtherPostId");
            CreateIndex("dbo.CommentLinks", "Url");
            CreateIndex("dbo.Users", "Name");
            DropColumn("dbo.CommentLinks", "PostId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CommentLinks", "PostId", c => c.Int());
            DropForeignKey("dbo.PostLinks", "PostId", "dbo.Posts");
            DropIndex("dbo.PostLinks", new[] { "Url" });
            DropIndex("dbo.PostLinks", new[] { "OtherCommentId" });
            DropIndex("dbo.PostLinks", new[] { "OtherPostId" });
            DropIndex("dbo.PostLinks", new[] { "UserId" });
            DropIndex("dbo.PostLinks", new[] { "PostId" });
            DropIndex("dbo.Users", new[] { "Name" });
            DropIndex("dbo.CommentLinks", new[] { "Url" });
            DropIndex("dbo.CommentLinks", new[] { "OtherPostId" });
            AlterColumn("dbo.CommentLinks", "Url", c => c.String(maxLength: 1024));
            DropColumn("dbo.Posts", "ProcessedTime");
            DropColumn("dbo.CommentLinks", "OtherPostId");
            DropTable("dbo.PostLinks");
            CreateIndex("dbo.CommentLinks", "Url");
            CreateIndex("dbo.CommentLinks", "PostId");
            RenameTable(name: "dbo.CommentLinks", newName: "Links");
        }
    }
}
