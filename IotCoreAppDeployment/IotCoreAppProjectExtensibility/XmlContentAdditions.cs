// Copyright (c) Microsoft. All rights reserved.

using System.IO;
using System.Xml;

namespace Microsoft.Iot.IotCoreAppProjectExtensibility
{
    public class AppxManifestCapabilityAddition : IContentChange
    {
        public string Capability { set; get; }
        public string CapabilityNamespace { set; get; }
        public string CapabilityName { set; get; }
        public string DeviceId { set; get; }
        public string FunctionType { set; get; }

        public bool ApplyToContent(string rootFolder)
        {
            string fullPath = rootFolder + @"\AppxManifest.xml";
            if (!File.Exists(fullPath))
            {
                return false;
            }

            var document = new XmlDocument();
            document.XmlResolver = null;
            using (var reader = new XmlTextReader(fullPath))
            {
                reader.DtdProcessing = DtdProcessing.Ignore;
                document.Load(reader);
            }

            var xmlnsManager = new System.Xml.XmlNamespaceManager(document.NameTable);
            xmlnsManager.AddNamespace("std", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");
            xmlnsManager.AddNamespace("mp", "http://schemas.microsoft.com/appx/2014/phone/manifest");
            xmlnsManager.AddNamespace("uap", "http://schemas.microsoft.com/appx/manifest/uap/windows10");
            xmlnsManager.AddNamespace("iot", "http://schemas.microsoft.com/appx/manifest/iot/windows10");
            xmlnsManager.AddNamespace("build", "http://schemas.microsoft.com/developer/appx/2015/build");

            var capability = Capability;
            if (capability == null)
            {
                if (CapabilityNamespace != null)
                {
                    capability = CapabilityNamespace + ":Capability";
                }
                else
                {
                    capability = "Capability";
                }
            }
            else
            {
                if (CapabilityNamespace != null)
                {
                    capability = CapabilityNamespace + ":" + Capability;
                }
                else
                {
                    capability = Capability;
                }
            }

            var capabilitiesNode = document.SelectSingleNode(@"/std:Package/std:Capabilities", xmlnsManager);
            var newCapability = document.CreateElement(capability, (CapabilityNamespace == null) ? document.DocumentElement.NamespaceURI : xmlnsManager.LookupNamespace(CapabilityNamespace));

            var capabilityNameAttribute = document.CreateAttribute("Name");
            capabilityNameAttribute.Value = CapabilityName;

            if (DeviceId != null)
            {
                var deviceNode = document.CreateElement("Device", document.DocumentElement.NamespaceURI);
                var deviceIdNameAttribute = document.CreateAttribute("Id");
                deviceIdNameAttribute.Value = DeviceId;

                deviceNode.Attributes.Append(deviceIdNameAttribute);

                if (FunctionType != null)
                {
                    var functionNode = document.CreateElement("Function", document.DocumentElement.NamespaceURI);
                    var functionTypeAttribute = document.CreateAttribute("Type");
                    functionTypeAttribute.Value = FunctionType;

                    functionNode.Attributes.Append(functionTypeAttribute);

                    deviceNode.AppendChild(functionNode);

                }

                newCapability.AppendChild(deviceNode);
            }

            newCapability.Attributes.Append(capabilityNameAttribute);
            capabilitiesNode.AppendChild(newCapability);


            document.Save(fullPath);
            return true;
        }
    }
}
