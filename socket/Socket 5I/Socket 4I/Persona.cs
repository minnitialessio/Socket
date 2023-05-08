using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socket_4I
{
    public class Persona //mi salvo la classe persona per salvare tutti i contatti
    {
        //per ogni contatto mi salvo il nome, la porta e lo storico della chat
        private string _nome;
        private int _porta;

        public List<Messaggio> Chat { set; get; }

        public string Nome
        {
            set
            {
                if(!string.IsNullOrEmpty(value))
                    _nome = value;
            }
            get
            {
                return _nome;
            }
        }
        public int Porta
        {
            set
            {
                if (value >= 10000)
                    _porta = value;
            }
            get
            {
                return _porta;
            }
        }
        public Persona(string nome, int porta)
        {
            Nome = nome;
            Porta = porta;
            Chat = new List<Messaggio>();
        }
        public override string ToString()
        {
            return Nome + " porta: " + Porta;
        }
    }
}
