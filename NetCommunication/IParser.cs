using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCommunication
{
    public interface IParser
    {
        public void Parse(string? data);
    }
}
