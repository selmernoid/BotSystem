namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Settings",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 50),
                        ValueString = c.String(maxLength: 100),
                        ValueInt = c.Int(),
                    })
                .PrimaryKey(t => t.Name);
            
            AddColumn("dbo.Posts", "IsDeleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.Posts", "IsStraw", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Posts", "IsStraw");
            DropColumn("dbo.Posts", "IsDeleted");
            DropTable("dbo.Settings");
        }
    }
}
