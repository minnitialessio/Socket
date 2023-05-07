using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socket_4I
{
    public class Messaggio
    {
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