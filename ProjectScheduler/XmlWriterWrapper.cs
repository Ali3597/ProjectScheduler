using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProjectScheduler
{
    public class XmlWriterWrapper : IXmlWriter
    {
        private XmlWriter inner;

        public XmlWriterWrapper(XmlWriter inner)
        {
            this.inner = inner;
        }

        public void WriteAttributeString(string attributeName, string attributeValue)
        {
            if (attributeValue!=null)
            {
                inner.WriteAttributeString(attributeName, attributeValue);
            }
        }
        public void WriteStartElement(string elementName)
        {
            inner.WriteStartElement(elementName);
        }

        public void WriteEndElement()
        {
            inner.WriteEndElement();
        }

    }
}
