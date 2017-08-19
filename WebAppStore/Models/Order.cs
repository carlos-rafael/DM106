using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppStore.Models
{   //Classe com as características de um pedido
    public class Order
    {
        //Construtor da classe
        public Order()
        {
            this.OrderItems = new HashSet<OrderItem>();
        }
        //Chave primária de order será o id
        public int Id { get; set; }
        //User name se refere ao dono do pedido
        public string UserName { get; set; }
        //DataPedido é a data que o pedido foi criado
        //[Column(TypeName = "DateTime2")]
        public DateTime DataPedido { get; set; }
        //DataEntrega é definida após o cálculo do prazo de entrega do pedido
        public DateTime DataEntrega { get; set; }
        //Status do pedido
        public string Status { get; set; }
        //Preço total envolve o preço de cada produto presente no pedido + preço do frete
        public decimal PrecoTotal { get; set; }
        //Peso total do pedido envolve a soma dos pesos de cada produto presente no pedido
        public decimal PesoTotal { get; set; }
        //Preço frete é obtido com auxílio do serviço dos correios
        public decimal PrecoFrete { get; set; }
        //Lista que armazena os itens de um pedido
        public virtual ICollection<OrderItem> OrderItems
        {
            get; set;
        }
    }
}