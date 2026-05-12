using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BERecepcion.Core.Utils
{
    public class StringWriterWithEncoding : StringWriter
    {
        private readonly System.Text.Encoding m_Encoding;
        public StringWriterWithEncoding(System.Text.Encoding encoding) : base()
        {
            this.m_Encoding = encoding;
        }

        public override System.Text.Encoding Encoding
        {
            get
            {
                return this.m_Encoding;
            }
        }
    }
}
