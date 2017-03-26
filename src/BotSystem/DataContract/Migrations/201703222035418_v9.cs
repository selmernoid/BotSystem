namespace DataContract.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v9 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserComments", "ModificationType", c => c.Int(nullable: false));
            AddColumn("dbo.UserComments", "ModificationComment", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserComments", "ModificationComment");
            DropColumn("dbo.UserComments", "ModificationType");
        }
    }
}
