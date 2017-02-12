namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v5 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserComments", "PostId", "dbo.Posts");
            DropForeignKey("dbo.PostLinks", "PostId", "dbo.Posts");
            DropForeignKey("dbo.PostTagPosts", "Post_Id", "dbo.Posts");
            DropIndex("dbo.UserComments", new[] { "PostId" });
            DropIndex("dbo.PostLinks", new[] { "PostId" });
            RenameColumn(table: "dbo.PostTagPosts", name: "Post_Id", newName: "Post_Isd");
            RenameIndex(table: "dbo.PostTagPosts", name: "IX_Post_Id", newName: "IX_Post_Isd");
            DropPrimaryKey("dbo.Posts");
            AddColumn("dbo.UserComments", "Post_Isd", c => c.Int());
            AddColumn("dbo.Posts", "Isd", c => c.Int(nullable: false));
            AddColumn("dbo.PostLinks", "Post_Isd", c => c.Int());
            AddPrimaryKey("dbo.Posts", "Isd");
            CreateIndex("dbo.UserComments", "Post_Isd");
            CreateIndex("dbo.PostLinks", "Post_Isd");
            AddForeignKey("dbo.UserComments", "Post_Isd", "dbo.Posts", "Isd");
            AddForeignKey("dbo.PostLinks", "Post_Isd", "dbo.Posts", "Isd");
            AddForeignKey("dbo.PostTagPosts", "Post_Isd", "dbo.Posts", "Isd", cascadeDelete: true);
            DropColumn("dbo.Posts", "Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Posts", "Id", c => c.Int(nullable: false));
            DropForeignKey("dbo.PostTagPosts", "Post_Isd", "dbo.Posts");
            DropForeignKey("dbo.PostLinks", "Post_Isd", "dbo.Posts");
            DropForeignKey("dbo.UserComments", "Post_Isd", "dbo.Posts");
            DropIndex("dbo.PostLinks", new[] { "Post_Isd" });
            DropIndex("dbo.UserComments", new[] { "Post_Isd" });
            DropPrimaryKey("dbo.Posts");
            DropColumn("dbo.PostLinks", "Post_Isd");
            DropColumn("dbo.Posts", "Isd");
            DropColumn("dbo.UserComments", "Post_Isd");
            AddPrimaryKey("dbo.Posts", "Id");
            RenameIndex(table: "dbo.PostTagPosts", name: "IX_Post_Isd", newName: "IX_Post_Id");
            RenameColumn(table: "dbo.PostTagPosts", name: "Post_Isd", newName: "Post_Id");
            CreateIndex("dbo.PostLinks", "PostId");
            CreateIndex("dbo.UserComments", "PostId");
            AddForeignKey("dbo.PostTagPosts", "Post_Id", "dbo.Posts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PostLinks", "PostId", "dbo.Posts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UserComments", "PostId", "dbo.Posts", "Id", cascadeDelete: true);
        }
    }
}
