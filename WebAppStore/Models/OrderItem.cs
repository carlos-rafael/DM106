using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppStore.Models
{   //Classe com as características de um item de pedido
    public class OrderItem
    {
        //Chave primária do item de pedido será o Id
        public int Id { get; set; }
        //Armazena a quantidade de produtos presente em um item de pedido. É utilizado para calcular peso, tamanho e preço de um pedido
        public int QuantidadeProd { get; set; }
        //Chave estrangeira, faz referência ao produto do pedido
        public int ProductId { get; set; }
        //Faz referência ao objeto de produto
        public virtual Product Product { get; set; }
        //Faz a ligação entre um item pedido e a chave primária da tabela de pedidos
        public int OrderId { get; set; }
    }
}