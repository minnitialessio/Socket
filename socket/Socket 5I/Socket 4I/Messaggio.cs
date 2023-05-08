using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socket_4I
{
    public class Messaggio //mi salvo una classe messaggio che contiene un messaggio inviato
    {
        //ogni messaggio contiene il messaggio in formato string e la persona che ha scritto il messaggio
        public string SMS { set; get; }
        public Persona Mittente { set; get; }

        public Messaggio(string sms, Persona persona) 
        { 
            SMS = sms;
            Mittente = persona;
        }

        public override string ToString()
        {
            return Mittente.Nome + " : " + SMS;
        }
    }
}