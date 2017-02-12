namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PostTags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
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
            
            AddColumn("dbo.UserComments", "IsPostAuthor", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserComments", "ParentCommentId", c => c.Int());
            AddColumn("dbo.UserComments", "Created", c => c.DateTime(nullable: false));
            AddColumn("dbo.UserComments", "Level", c => c.Int(nullable: false));
            AddColumn("dbo.Posts", "Title", c => c.String(maxLength: 100));
            AddColumn("dbo.Posts", "IsLong", c => c.Boolean(nullable: false));
            AddColumn("dbo.Posts", "IsMine", c => c.Boolean(nullable: false));
            AddColumn("dbo.Posts", "Type", c => c.String(maxLength: 20));
            AddColumn("dbo.Posts", "Created", c => c.DateTime(nullable: false));
            AddColumn("dbo.Posts", "LastCheck", c => c.DateTime(nullable: false));
            AlterColumn("dbo.UserComments", "Rating", c => c.Int());
            CreateIndex("dbo.UserComments", "ParentCommentId");
            AddForeignKey("dbo.UserComments", "ParentCommentId", "dbo.UserComments", "Id");
            DropColumn("dbo.Posts", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Posts", "Name", c => c.String(maxLength: 100));
            DropForeignKey("dbo.PostTagPosts", "Post_Id", "dbo.Posts");
            DropForeignKey("dbo.PostTagPosts", "PostTag_Id", "dbo.PostTags");
            DropForeignKey("dbo.UserComments", "ParentCommentId", "dbo.UserComments");
            DropIndex("dbo.PostTagPosts", new[] { "Post_Id" });
            DropIndex("dbo.PostTagPosts", new[] { "PostTag_Id" });
            DropIndex("dbo.UserComments", new[] { "ParentCommentId" });
            AlterColumn("dbo.UserComments", "Rating", c => c.Int(nullable: false));
            DropColumn("dbo.Posts", "LastCheck");
            DropColumn("dbo.Posts", "Created");
            DropColumn("dbo.Posts", "Type");
            DropColumn("dbo.Posts", "IsMine");
            DropColumn("dbo.Posts", "IsLong");
            DropColumn("dbo.Posts", "Title");
            DropColumn("dbo.UserComments", "Level");
            DropColumn("dbo.UserComments", "Created");
            DropColumn("dbo.UserComments", "ParentCommentId");
            DropColumn("dbo.UserComments", "IsPostAuthor");
            DropTable("dbo.PostTagPosts");
            DropTable("dbo.PostTags");
        }
    }
}
