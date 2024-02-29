using System;
using System.Collections.Generic;
using System.Text;

namespace Fila_Unica_BQ.Models
{
    public class EscolaCandidato
    {
        public int EscolaCod { get; set; }
        public string EscolaNome { get; set; }
        public int Opcao1 { get; set; }
        public int Opcao2 { get; set; }
        public int Opcao3 { get; set; }
        public string Opcoes { get; set; }
    }
}