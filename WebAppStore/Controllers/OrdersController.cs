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
using System.Security.Principal;
using WebAppStore.CRMClient;
using WebAppStore.br.com.correios.ws;
   
namespace WebAppStore.Controllers
{   
    //Todas operações dessa classe demandam que o usuário seja autenticado
    [Authorize]
    //rota default para acesso das operações presentes nessa classe
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        //cria uma refferência ao banco de dados
        private WebAppStoreContext db = new WebAppStoreContext();

        //soap
        //Método que calcula o frete de um pedido, recebe como parâmetro de entrada o id do pedido
        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("frete")]
        public IHttpActionResult CalculaFrete(int id)
        {
            //busca o produto que possui o id fornecido pelo usuário
            Order order = db.Orders.Find(id);

            //se o pedido não existir no banco, notificar o usuário
            if (order == null)
            {
                return BadRequest("O pedido não existe");
            }

            if (CheckUserFromOrder(User, order))
            {

                string frete;
                CRMRestClient crmClient = new CRMRestClient();
                //obtém referência do usuário com base na autenticação que ele realizou (o cep do usuário será utilizado nesse método)
                Customer customer = crmClient.GetCustomerByEmail(User.Identity.Name);

                //definição das variáveis de auxílio referentes ao diâmetro, altura, largura e comprimento do(s) produto(s) do pedido
                decimal maiorDiametro = 0, maiorAltura = 0, maiorLargura = 0, comprimento = 0;
                
                
                if (customer != null)
                {
                    /*para cada item de pedido presente em um pedido, é incrementado o preço total do pedido, peso total, maior diâmetro, largura e altura, 
                     e a soma do comprimento*/
                    foreach (OrderItem oi in order.OrderItems)
                    {
                        //a soma com o preço do frete ocorrerá no fim do método
                        order.PrecoTotal += oi.Product.Preco * oi.QuantidadeProd;
                        order.PesoTotal += oi.Product.Peso * oi.QuantidadeProd;
                        //soma dos comprimentos, tomando como premissa que um produto ficará ao lado do outro
                        comprimento += oi.Product.Comprimento * oi.QuantidadeProd;
                        
                        if (oi.Product.Diametro > maiorDiametro)
                        {
                            maiorDiametro = oi.Product.Diametro;
                        }
                        if (oi.Product.Altura > maiorAltura)
                        {
                            maiorAltura = oi.Product.Altura;
                        }
                        if (oi.Product.Largura > maiorLargura)
                        {
                            maiorLargura = oi.Product.Largura;
                        }

                    }
                    /*DEBUG*/
                    /*
                    System.Diagnostics.Debug.WriteLine("order.PrecoTotal é " + order.PrecoTotal);
                    System.Diagnostics.Debug.WriteLine("order.PesoTotal é " + order.PesoTotal);
                    System.Diagnostics.Debug.WriteLine("Maior diâmetro é " + maiorDiametro);
                    System.Diagnostics.Debug.WriteLine("Maior altura é " + maiorAltura);
                    System.Diagnostics.Debug.WriteLine("Maior largura é " + maiorLargura);
                    System.Diagnostics.Debug.WriteLine("Maior comprimento é " + comprimento);
                    */
                    
                    /*o serviço dos correios para cálculo de preço e prazo é invocado passando como parâmetros as informações do(s) produto(s) presente(s) no pedido*/
                    CalcPrecoPrazoWS correios = new CalcPrecoPrazoWS();
                    //empresa,senha,serviço(SEDEX varejo), cepOrigem (Manaus (Bairro Cidade Nova)),cepDestino, Peso, CdFormato, comprimento, altura, largura, diametro, ,maopropria, valordeclarado (0), avisorecebimento (s)
                    cResultado resultado = correios.CalcPrecoPrazo("", "", "40010", "69096010", customer.zip, order.PesoTotal.ToString(), 1, comprimento, maiorAltura, maiorLargura, maiorDiametro, "N", 0, "S");
                    
                    if (resultado.Servicos[0].Erro.Equals("0"))
                    {
                        frete = "Valor do frete: " + resultado.Servicos[0].Valor + " - Prazo de entrega: " + resultado.Servicos[0].PrazoEntrega + " dia(s)";
                        //realizada conversão de String para Decimal    

                        order.PrecoFrete = Convert.ToDecimal(resultado.Servicos[0].Valor)/100;
                        //definição do preço total do pedido, que é a soma do preço de cada produto + o preço do frete
                        order.PrecoTotal += order.PrecoFrete;
                        //data da entrega é a soma do retorno de prazo do serviço à data do pedido (foi realizada conversão de String para double) antes de somar as datas
                        order.DataEntrega = order.DataPedido.AddDays(Convert.ToDouble(resultado.Servicos[0].PrazoEntrega)); 
                        //modificações são persistidas no banco de dados
                        db.SaveChanges();
                        //retorna status Ok, com mensagem do valor do frete e prazo em dias para entrega do pedido
                        return Ok(frete);
                    }
                    //caso o serviço retorne erro, o usuário é informado
                    else
                    {
                        return BadRequest("Código do erro: " + resultado.Servicos[0].Erro + "-" + resultado.Servicos[0].MsgErro);
                    }
                }
                //caso customer==null, o usuário é informado de erro
                else
                {
                    return BadRequest("Falha ao consultar o CRM");
                }
            }
            //caso o usuário não tenha autorização para visualizar o pedido
            else
            {
                return BadRequest("Acesso não autorizado");
            }
        }
        
        //Método que busca pedidos com base no e-mail fornecido pelo usuário
        [ResponseType(typeof(List<Order>))]
        [HttpGet]
        [Route("email")]
        //public List<Order> GetOrdersEmail(String email)
        public IHttpActionResult GetOrdersEmail(String email)
        {
               
            //valida permissão  
            if (CheckUserFromEmail(User, email))
            {  
               //Cria uma lista com o retorno de todos os pedidos criados pelo usuário 
               List<Order> orderList = db.Orders.Include(order => order.OrderItems).Where(p => p.UserName == email).ToList();
               //retorna a lista de pedidos criados pelo usuário
                return Ok(orderList);
            }
            //caso permissão não seja concedida retorna mensagem ao usuário
            else return BadRequest("Acesso não autorizado");
        }


        // GET: api/Orders
        //Método que retorna todos os pedidos presentes no banco, pode ser acessado somente pelo usuário administrador
        [Authorize(Roles = "ADMIN")]
        public List<Order> GetOrders()
        {
            return db.Orders.Include(order => order.OrderItems).ToList();
        }

        //Close Order
        // POST: api/Orders/closeOrder/5
        //Método para encerramento de um produto, recebe como parâmetro de entrada o id do produto que será encerrado
        [HttpPut]
        [Route("closeOrder")]
        [ResponseType(typeof(Order))]
        public IHttpActionResult CloseOrder(int id)
        {   //busca o produto no banco
            Order order = db.Orders.Find(id);
            //caso o produto não exista, notifica o usuário
            if (order == null)
            {
                return BadRequest("O pedido não existe");
            }
            //checa permissão do usuário para acessar tal operação
            if (CheckUserFromOrder(User, order))
            {
                //caso o frete do pedido ainda não tenha sido calculado, tal operação não pode ser realizada    
                if (order.PrecoFrete == 0)
                {
                    return BadRequest("Não é possível fechar este pedido, pois o preço do frete ainda não foi calculado");
                }
                //caso contrário o status do pedido é modificado para "fechado" e o banco persiste as laterações realizadas
                order.Status = "fechado";
                db.SaveChanges();
                //o pedido é retornado ao usuário
                return Ok(order);
            }
            //caso o usuário não possua permissão, notifica impossibilidade de acesso a esta operação
            else
            {
                return BadRequest("Acesso não autorizado");

            }
        }


        // GET: api/Orders/5
        //Método que busca um pedido com base no id fornecido pelo usuário
        [ResponseType(typeof(Order))]
        public IHttpActionResult GetOrder(int id)
        {
            //busca o pedido no banco
            Order order = db.Orders.Find(id);
            //se o pedido não existir no banco, notifica o usuário
            if (order == null)
            {
                return BadRequest("Este pedido não existe");
            }
            //checa se o usuário é o dono do pedido ou o admin
            if (CheckUserFromOrder(User, order))
            {

                //retorna o pedido ao usuário
                return Ok(order);
            }
            //caso o usuário não possua autorização, notifica-o da impossibilidade de prosseguir com a operação
            else
            {
                return BadRequest("Acesso não autorizado");
                //return StatusCode(HttpStatusCode.Forbidden);
            }
        }

        // PUT: api/Orders/5
        /*Método para atualizar um pedido, possui como parâmteros de entrada o id do pedido que será atualizado
         * assim como os novos valores para o pedido
        */
        [ResponseType(typeof(void))]
        public IHttpActionResult PutOrder(int id, Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != order.Id)
            {
                return BadRequest();
            }
            //checa se o usuário é o dono do pedido ou admin
            if (CheckUserFromOrder(User, order))
            {   //atualiza o pedido com as novas informações
                db.Entry(order).State = EntityState.Modified;

                try
                {

                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(id))
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
            else
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }
        }

        // POST: api/Orders
        /*Método para criar um novo pedido, recebe como parâmetro de entrada um objeto do tipo pedido*/
        [ResponseType(typeof(Order))]
        public IHttpActionResult PostOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (CheckUserFromOrder(User, order))
            {
                /*todo novo pedido possui por default status="novo", peso total=0,preço do frete=0, 
                preço total=0, e data do pedido = data do momento em que foi criado
                 */
                order.Status = "novo";
                order.PesoTotal = 0;
                order.PrecoFrete = 0;
                order.PrecoTotal = 0;
                order.DataPedido = System.DateTime.Now;
                order.DataEntrega = System.DateTime.MinValue;

                var dataConvertidaPedido = order.DataPedido.ToShortDateString();
                order.DataPedido = DateTime.Parse(dataConvertidaPedido);
                
                var dataConvertidaEntrega = order.DataEntrega.ToShortDateString();
                order.DataEntrega = DateTime.Parse(dataConvertidaEntrega);

                //Adiciona o pedido e atualiza o banco de dados
                db.Orders.Add(order);
                db.SaveChanges();

                return CreatedAtRoute("DefaultApi", new { id = order.Id }, order);
            }
            else
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }
        }

        // DELETE: api/Orders/5
        //Método que deleta um pedido com base no seu id
        [ResponseType(typeof(Order))]
        public IHttpActionResult DeleteOrder(int id)
        {
            //busca o pedido no banco
            Order order = db.Orders.Find(id);
            //caso o pedido não exista, informa o usuário
            if (order == null)
            {
                return BadRequest("O pedido " + id + " não existe");
            }
            //checa se o usuário é o dono do pedido ou admin
            if (CheckUserFromOrder(User, order))
            {
                //remove o pedido e atualiza o banco de dados
                db.Orders.Remove(order);
                db.SaveChanges();
                //retorna o pedido removido
                return Ok(order);
            }
            else
            {
                //return StatusCode(HttpStatusCode.Forbidden);
                return BadRequest("Acesso não autorizado");
            }
        }
 
        //método booleano auxiliar que diz se o usuário qeu está tentando a operação é o mesmo do parâmetro email informado por ele, ou o usuário admin
        private bool CheckUserFromEmail(IPrincipal user, String email)
        {
            System.Diagnostics.Debug.WriteLine("User: " + user + ", email: " + email);
            return ((user.Identity.Name.Equals(email)) || (user.IsInRole("ADMIN")));
        }

        //método booleano auxiliar que diz se o usuário que está tentando uma operação é o dono do pedido ou o usuário admin
        private bool CheckUserFromOrder(IPrincipal user, Order order)
        {
            return ((user.Identity.Name.Equals(order.UserName)) || (user.IsInRole("ADMIN")));
        }
        
        //método booleano que retorna true caso o pedido procurado exista
        private bool OrderExists(int id)
        {
            return db.Orders.Count(e => e.Id == id) > 0;
        }
    }
}