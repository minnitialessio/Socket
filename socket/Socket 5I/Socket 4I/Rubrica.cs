using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socket_4I
{
    public class Rubrica //mi salvo una classe rubrica che contiene tutti i contatti
    {
        public List<Persona> Persone { set; get; }

        public Rubrica(List<Persona> persone)
        {
            Persone = persone;
        }
        public Rubrica()
        {
            Persone = new List<Persona>();
        }
    }
}
