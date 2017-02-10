namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CommentLinks", "CommentId", "dbo.UserComments");
            DropForeignKey("dbo.PostLinks", "PostId", "dbo.Posts");
            DropForeignKey("dbo.UserComments", "PostId", "dbo.Posts");
            DropPrimaryKey("dbo.UserComments");
            DropPrimaryKey("dbo.Posts");
            AddColumn("dbo.UserComments", "Content", c => c.String());
            AddColumn("dbo.Posts", "CommentsCount", c => c.Int(nullable: false));
            AlterColumn("dbo.UserComments", "Id", c => c.Int(nullable: false));
            AlterColumn("dbo.Posts", "Id", c => c.Int(nullable: false));
            AlterColumn("dbo.Posts", "ProcessedTime", c => c.Time(precision: 7));
            AddPrimaryKey("dbo.UserComments", "Id");
            AddPrimaryKey("dbo.Posts", "Id");
            AddForeignKey("dbo.CommentLinks", "CommentId", "dbo.UserComments", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PostLinks", "PostId", "dbo.Posts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UserComments", "PostId", "dbo.Posts", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserComments", "PostId", "dbo.Posts");
            DropForeignKey("dbo.PostLinks", "PostId", "dbo.Posts");
            DropForeignKey("dbo.CommentLinks", "CommentId", "dbo.UserComments");
            DropPrimaryKey("dbo.Posts");
            DropPrimaryKey("dbo.UserComments");
            AlterColumn("dbo.Posts", "ProcessedTime", c => c.Time(nullable: false, precision: 7));
            AlterColumn("dbo.Posts", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.UserComments", "Id", c => c.Int(nullable: false, identity: true));
            DropColumn("dbo.Posts", "CommentsCount");
            DropColumn("dbo.UserComments", "Content");
            AddPrimaryKey("dbo.Posts", "Id");
            AddPrimaryKey("dbo.UserComments", "Id");
            AddForeignKey("dbo.UserComments", "PostId", "dbo.Posts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PostLinks", "PostId", "dbo.Posts", "Id", cascadeDelete: true);
            AddForeignKey("dbo.CommentLinks", "CommentId", "dbo.UserComments", "Id", cascadeDelete: true);
        }
    }
}
