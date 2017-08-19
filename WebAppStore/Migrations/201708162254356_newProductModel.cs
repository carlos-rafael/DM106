namespace WebAppStore.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newProductModel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Products", "Codigo", c => c.String(nullable: false));
            AlterColumn("dbo.Products", "Url", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Products", "Url", c => c.String(maxLength: 80));
            AlterColumn("dbo.Products", "Codigo", c => c.String(nullable: false, maxLength: 8));
        }
    }
}
