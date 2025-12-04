using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CJR_Racing
{
    public interface ICarModule
    {
        void Configure();
        void Show();
        object GetData();
    }
}
