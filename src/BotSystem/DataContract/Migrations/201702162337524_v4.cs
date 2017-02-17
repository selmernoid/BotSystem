namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v4 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Communities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 32),
                        Link = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name);
            
            AddColumn("dbo.CommentLinks", "DataId", c => c.String(maxLength: 50));
            AddColumn("dbo.Posts", "CommunityId", c => c.Int());
            AddColumn("dbo.PostLinks", "DataId", c => c.String(maxLength: 50));
            CreateIndex("dbo.Posts", "CommunityId");
            AddForeignKey("dbo.Posts", "CommunityId", "dbo.Communities", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Posts", "CommunityId", "dbo.Communities");
            DropIndex("dbo.Communities", new[] { "Name" });
            DropIndex("dbo.Posts", new[] { "CommunityId" });
            DropColumn("dbo.PostLinks", "DataId");
            DropColumn("dbo.Posts", "CommunityId");
            DropColumn("dbo.CommentLinks", "DataId");
            DropTable("dbo.Communities");
        }
    }
}
