
namespace ProjectScheduler
{
    public interface IXmlWriter
    {
        void WriteStartElement(string elementName);
        void WriteEndElement();
        
        void WriteAttributeString(string attributeName, string attributeValue);            
    }
}
