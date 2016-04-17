using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;

namespace spiegel
{
    

    class Config
    {



        private List<string> moduleNames;
        private const string configFileName = "configuration.xml";
        private StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        private StorageFile configFile;
        private XmlDocument xdoc;
        public Dictionary<string, Dictionary<string,string >> settings;

        public async Task<bool> makeSetting(string name, string v1, string v2)
        {
            Debug.WriteLine("Making new Setting");
            if (settings.Keys.Contains(name))
            {
                if (settings[name].Keys.Contains(v1))
                {
                    return false;
                }
                else
                {
                    settings[name][v1] = v2;
                    XmlNodeList element = xdoc.GetElementsByTagName("configuration");
                    XmlNode modules = element.Item(0);
                    foreach (XmlNode xn in modules)
                    {
                        foreach (XmlNode x in xn)
                        {
                            if (x.Attributes.Item(0).InnerXml == name)
                            {
                                XmlNode newNode = xdoc.CreateElement(v1);
                                newNode.InnerText = v2;
                                x.AppendChild(newNode);
                            }
                        }

                    }
                    await writeToFile();
                    return true;
                }
            }
            return false;
        }

        private async Task writeToFile()
        {
            configFile = await storageFolder.CreateFileAsync(configFileName, CreationCollisionOption.ReplaceExisting);
            var writeStream = await configFile.OpenStreamForWriteAsync();
            xdoc.Save(writeStream);
            writeStream.Flush();
            writeStream.Dispose();
        }
        public bool hasSetting(string name, string v)
        {
            Debug.WriteLine("Checking setting ");
            if (settings.Keys.Contains(name))
            {
                if (settings[name].Keys.Contains(v))
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        public Config(List<string> moduleNames)
        {
            this.moduleNames = moduleNames;
            settings = new Dictionary<string,Dictionary<string,string>>();
        }

        public async Task LoadFromFile()
        {
           xdoc  = new XmlDocument();

            try
            {
                IRandomAccessStream readStream;

                configFile = await storageFolder.GetFileAsync(configFileName);
                readStream = await configFile.OpenAsync(FileAccessMode.Read);

                xdoc.Load(readStream.AsStreamForRead());

                readStream.Dispose();
            }
            catch
            {
                throw new UnableToReadConfigurationFileException();
            }


            try {
                XmlNodeList element = xdoc.GetElementsByTagName("configuration");
                XmlNode modules = element.Item(0);
                foreach (XmlNode xn in modules)
                {
                    foreach (XmlNode x in xn)
                    {
                        string name = x.Attributes.Item(0).InnerXml;
                        settings[name] = new Dictionary<string, string>();
                        foreach(XmlNode _x in x)
                        {
                            settings[name][_x.Name] = _x.InnerText;
                        }
                        
                    }
                }
                foreach(KeyValuePair<string, Dictionary<string, string>> entry in settings)
                {
                    Debug.WriteLine(entry.Key);
                    foreach (KeyValuePair<string,string> entry2 in entry.Value)
                    {
                        Debug.WriteLine("\t" + entry2.Key + " : " + entry2.Value);
                    }
                }
                
            }
            catch
            {
                throw new UnableToAsignConfigurationSettingsException();
            }
        }
        public string getSetting(string moduleName,string setting)
        {
            return settings[moduleName][setting];
        }
        public void setSetting(string moduleName,string setting,string value)
        {
            if (hasSetting(moduleName, setting))
            {
                settings[moduleName][setting] = value;
            }
        }
        public bool getEnabled(string moduleName)
        {
            return (settings[moduleName]["enabled"] == "true")? true :false  ;
        }
        public async void storeToFile()
        {
            try {
                XmlDocument configXml = new XmlDocument();

                XmlElement root = configXml.DocumentElement;

                XmlElement configRoot = configXml.CreateElement("configuration");
                configXml.AppendChild(configRoot);

               /* foreach (KeyValuePair<ConfigType, string> setting in settings)
                {
                    string settingName = Enum.GetName(typeof(ConfigType), setting.Key);

                    XmlElement element = configXml.CreateElement(settingName);
                    XmlText elementText = configXml.CreateTextNode(setting.Value);
                    element.AppendChild(elementText);
                    configRoot.AppendChild(element);
                }*/

                await storeXmlToFile(configXml);
            }
            catch
            {
                throw new UnableToStoreConfigurationException();
            }
        }
        private async Task storeXmlToFile(XmlDocument document)
        {
            configFile = await storageFolder.CreateFileAsync(configFileName, CreationCollisionOption.ReplaceExisting);
            var writeStream = await configFile.OpenStreamForWriteAsync();
            document.Save(writeStream);
            writeStream.Flush();
            writeStream.Dispose();
        }

        public async Task makeFile(List<string> moduleNames)
        {
            XmlDocument _xdoc = new XmlDocument();
            XmlNode root = _xdoc.CreateNode(XmlNodeType.Element, "configuration", null);
            XmlNode modules = _xdoc.CreateNode(XmlNodeType.Element, "modules", null);

            foreach(String moduleName in moduleNames){
                XmlNode module = _xdoc.CreateNode(XmlNodeType.Element, "module", null);
                XmlAttribute attr = _xdoc.CreateAttribute("name");
                attr.Value = moduleName;
                module.Attributes.Append(attr);

                XmlNode enabled = _xdoc.CreateElement("enabled");
                enabled.InnerText = "false";

                module.AppendChild(enabled);

                modules.AppendChild(module);
            }
            root.AppendChild(modules);
            _xdoc.AppendChild(root);
            configFile = await storageFolder.CreateFileAsync(configFileName, CreationCollisionOption.ReplaceExisting);
            var writeStream = await configFile.OpenStreamForWriteAsync();
            _xdoc.Save(writeStream);
            writeStream.Flush();
            writeStream.Dispose();
        }
    }
}
