using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebAppStore.Models
{   //Classe com as características de um produto
    public class Product
    {
        //Chave primária da tabela de produtos
        public int Id { get; set; }
        //Campo que define o nome de um produto, de preenchimento obrigatório
        [Required(ErrorMessage = "O campo nome é obrigatório")]
        public string Nome { get; set; }
        //Campo que define a descrição de um produto
        public string Descricao { get; set; }
        //Campo que define a cor de um produto
        public string Cor { get; set; }
        //Campo que define o modelo de um produto, de preenchimento obrigatório
        [Required(ErrorMessage = "O campo nome é obrigatório")]
        public String Modelo { get; set; }
        //Campo que define o tamanho máximo de um produto, de preenchimento obrigatório
        [Required(ErrorMessage = "O campo nome é obrigatório")]
        public string Codigo { get; set; }
        //Campo que define o preço de um produto
        public decimal Preco { get; set; }
        //Campo que define o peso de um produto
        public decimal Peso { get; set; }
        //Campo que define a altura de um produto
        public decimal Altura { get; set; }
        //Camop que define a largura de um produto
        public decimal Largura { get; set; }
        //Campo que define o comprimento de um produto
        public decimal Comprimento { get; set; }
        //Campo que define o diâmetro de um produto
        public decimal Diametro { get; set; }
        //Campo que define a url de um produto 
        public string Url { get; set; }
    }
}