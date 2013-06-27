namespace PolymeliaDeploy.Activities
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xaml;
    using System.Xml;

    class IgnorableXamlXmlWriter : XamlXmlWriter
    {

        HashSet<NamespaceDeclaration> ignorableNamespaces = new HashSet<NamespaceDeclaration>();
        HashSet<NamespaceDeclaration> allNamespaces = new HashSet<NamespaceDeclaration>();
        bool objectWritten;
        bool hasDesignNamespace;
        string designNamespacePrefix;

        public IgnorableXamlXmlWriter(TextWriter tw, XamlSchemaContext context)
            : base(XmlWriter.Create(tw,
                                    new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }),
                                    context,
                                    new XamlXmlWriterSettings { AssumeValidInput = true })
        {

        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            if (!this.objectWritten)
            {
                this.allNamespaces.Add(namespaceDeclaration);
                // if we find one, add that to ignorable namespaces
                // the goal here is to collect all of them that might point to this
                // if you had a broader set of things to ignore, you would collect 
                // those here.
                if (namespaceDeclaration.Namespace.Contains("http://schemas.microsoft.com/netfx/20") &&
                    namespaceDeclaration.Namespace.Contains("/xaml/activities/presentation"))
                {
                    this.hasDesignNamespace = true;
                    this.designNamespacePrefix = namespaceDeclaration.Prefix;
                }
            }
            base.WriteNamespace(namespaceDeclaration);
        }

        public override void WriteStartObject(XamlType type)
        {
            if (!this.objectWritten)
            {
                // we should check if we should ignore 
                if (this.hasDesignNamespace)
                {
                    // note this is not robust as mc could naturally occur
                    string mcAlias = "mc";
                    this.WriteNamespace(
                        new NamespaceDeclaration(
                            "http://schemas.openxmlformats.org/markup-compatibility/2006",
                            mcAlias)
                            );

                }
            }
            base.WriteStartObject(type);
            if (!this.objectWritten)
            {
                if (this.hasDesignNamespace)
                {
                    XamlDirective ig = new XamlDirective(
                        "http://schemas.openxmlformats.org/markup-compatibility/2006",
                        "Ignorable");
                    this.WriteStartMember(ig);
                    this.WriteValue(this.designNamespacePrefix);
                    this.WriteEndMember();
                    this.objectWritten = true;
                }
            }
        }

    }
}