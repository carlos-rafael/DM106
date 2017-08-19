namespace WebAppStore.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using WebAppStore.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<WebAppStore.Models.WebAppStoreContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(WebAppStore.Models.WebAppStoreContext context)
        {

            context.Products.AddOrUpdate(
                p => p.Id,
                new Product
                {
                    Id = 1,
                    Nome = "produto 1",
                    Descricao="este é o produto 1",
                    Cor = "preto 1",
                    Modelo="modelo 1",
                    Codigo ="COD1",
                    Preco = 10,
                    Peso = 100,
                    Altura = 10,
                    Largura = 10,
                    Comprimento=10,
                    Diametro=10,
                    Url= "www.siecolasystems.com/produto1"
                },
                new Product
                {
                    Id = 2,
                    Nome = "produto 2",
                    Descricao = "este é o produto 2",
                    Cor = "preto 2",
                    Modelo = "modelo 2",
                    Codigo = "COD2",
                    Preco = 20,
                    Peso = 200,
                    Altura = 20,
                    Largura = 20,
                    Comprimento = 20,
                    Diametro = 20,
                    Url = "www.siecolasystems.com/produto2"
                });
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
