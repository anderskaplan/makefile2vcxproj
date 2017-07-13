using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace make2vc
{
    class VcxprojWriter
    {
        public static void Write(string vcxprojPath)
        {
            var xmlWriterSettings = new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                OmitXmlDeclaration = false
            };

            using (var stream = new FileStream(vcxprojPath, FileMode.Create, FileAccess.Write))
            using (var writer = XmlWriter.Create(stream, xmlWriterSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
                writer.WriteAttributeString("DefaultTargets", "Build");
                writer.WriteAttributeString("ToolsVersion", "4.0");


                writer.WriteEndElement(); // Project
                writer.WriteEndDocument();
            }
        }
    }
}
