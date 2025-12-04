using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CJR_Racing
{
    public abstract class CarModule : ICarModule
    {
        
        public abstract void Configure(); 
        public abstract void Show();      
        public abstract object GetData();

    }
}
