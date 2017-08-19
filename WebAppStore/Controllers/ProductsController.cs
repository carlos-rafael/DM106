using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using WebAppStore.Models;

namespace WebAppStore.Controllers
    //Classe que gerencia as operações relativas a produtos na aplicação

{   //somente usuários autorizados poderão fazer uso da classe
    [Authorize]
    public class ProductsController : ApiController
    {
        //Instancia uma referência para acesso ao banco de dados
        private WebAppStoreContext db = new WebAppStoreContext();

        // GET: api/Products
        //Método que retorna todos os produtos presentes no banco
        public IQueryable<Product> GetProducts()
        {
            return db.Products;
        }

        // GET: api/Products/5
        /*Método que retorna apenas o produto que o usuário busca, caso o
         * produto não seja encontrado, retorna uma mensagem ao usuário*/
        [ResponseType(typeof(Product))]
        public IHttpActionResult GetProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return BadRequest("O produto de id "+id+" não existe");
            }

            return Ok(product);
        }

        // PUT: api/Products/5
        /*Método para alterar informações de um produto, recebe como parâmetro o id do produto que será alterado
        e o objeto produto com os novos valores para cada atributo*/
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProduct(int id, Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //caso o usuário tenha mantido o id, código e modelo do produto, alterando outros campos, tal atualização deve ser permitida
            if (db.Products.Any(o => o.Codigo == product.Codigo && o.Id == product.Id && o.Modelo == product.Modelo))
            {
                //System.Diagnostics.Debug.WriteLine("esta alteração pode ser realizada");
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return Ok();
            }
            //caso o usuário tenha modificado o campo código e modelo para valores não presentes nos outros produtos, tal atualização deve ser permitida
            if (!db.Products.Any(o => o.Codigo == product.Codigo))
            {
                if (!db.Products.Any(o => o.Modelo == product.Modelo))
                {
                    //System.Diagnostics.Debug.WriteLine("comparacao do modelo " + !db.Products.Any(o => o.Modelo == product.Modelo.ToString()));
                    db.Entry(product).State = EntityState.Modified;
                    db.SaveChanges();
                    return Ok();
                }
                
            }
            //caso contrário, retorna operação não permitida
            else { return BadRequest("Operação não permitida"); }

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Products
        //Método para criação de um novo produto. Recebe como parâmetro um objeto produto
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(Product))]
        public IHttpActionResult PostProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //caso o código do novo produto seja igual a algum código já existente, a criação do novo produto não pode ser permitida
            if (db.Products.Any(o => o.Codigo == product.Codigo))
            {
                System.Diagnostics.Debug.WriteLine("Este código já existe, operação negada");
                return BadRequest("Este código já existe, operação negada");
            }
            //caso o modelo do novo produto seja igual a algum modelo já existente, a criação do novo produto não pode ser permitida    
            if (db.Products.Any(o => o.Modelo == product.Modelo))
            {
                System.Diagnostics.Debug.WriteLine("Este modelo já existe, operação negada");
                return BadRequest("Este modelo já existe, operação negada");
            }
            //caso contrário o novo produto poderá ser criado e salvo no banco
            db.Products.Add(product);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        /*Método responsável por deletear um produto do banco. Recebe como parâmetro o id do produto a ser deletado.
         Caso o produto não exista, o usuário é informado através da mensagem de resposta*/
        [Authorize(Roles = "ADMIN")]
        [ResponseType(typeof(Product))]
        public IHttpActionResult DeleteProduct(int id)
        {
            //busca-se o produto no banco com base no id passado pelo usuário
            Product product = db.Products.Find(id);
            //se o produto não existe, informa o usuário de tal situação
            if (product == null)
            {
                return BadRequest("O produto de id "+id+" não existe");
            }
            //caso contrário o produto é removido e o banco atualizado
            db.Products.Remove(product);
            db.SaveChanges();
            //retorna o produto que acabou de ser removido
            return Ok(product);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //Valida se um produto existe com base no parâmetro id de entrada
        private bool ProductExists(int id)
        {
            return db.Products.Count(e => e.Id == id) > 0;
        }
    }
}