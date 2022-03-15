using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDBLib
{
    class ErrorsBuffer
    {
        private static ErrorsBuffer instance = null;
        private static readonly object padlock = new object();
        private List<string> messages = new List<string>();
        private ErrorsBuffer()
        {          
        }
        public static ErrorsBuffer Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ErrorsBuffer();
                    }
                    return instance;
                }
            }
        }
        public void AddError(string errorMsg)
        {
            messages.Add(errorMsg);
        }
        public List<string> GetErros()
        {
            List<string> ret = new List<string>(messages);
            messages.Clear();
            return ret;
        }
    }
}
