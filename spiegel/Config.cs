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
                    try {
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
                    }catch(Exception e)
                    {
                        Debug.WriteLine("XML FOUND");
                        Debug.WriteLine(e.ToString());
                    }
                }
            }
            return false;
        }
        public async Task<bool> changeSetting(string name, string v1, string v2)
        {
            Debug.WriteLine("Changing Setting");
            try {
                if (settings.Keys.Contains(name))
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
                                foreach (XmlNode _x in x)
                                {
                                    if (_x.Name == v1)
                                    {
                                        _x.InnerText = v2;
                                    }
                                }
                            }
                        }

                    }
                    await writeToFile();
                    return true;
                }
                return false;
            }catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
                return false;
            }
        }
        private async Task writeToFile()
        {
            configFile = await storageFolder.CreateFileAsync(configFileName, CreationCollisionOption.ReplaceExisting);
            var writeStream = await configFile.OpenStreamForWriteAsync();
            xdoc.Save(writeStream);
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
            try {
                return settings[moduleName][setting];
            }
            catch(Exception e)
            {
                Debug.WriteLine(moduleName);
                Debug.WriteLine(setting);
                Debug.WriteLine(e.ToString());
                return "";
            }
        }
        public Dictionary<string, string> getModuleSettings(string moduleName)
        {
            try
            {
                return settings[moduleName];
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return new Dictionary<string, string>();
            }
        }
        public async void setSetting(string moduleName,string setting,string value)
        {
            if (hasSetting(moduleName, setting))
            {
                settings[moduleName][setting] = value;
                await changeSetting(moduleName, setting, value);
            }
        }
        public bool getEnabled(string moduleName)
        {
            try
            {
                return (settings[moduleName]["enabled"] == "true")? true :false  ;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return false;
            }
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
            Debug.WriteLine("There wasn't a file o we are making one");
            xdoc = new XmlDocument();
            XmlNode root = xdoc.CreateNode(XmlNodeType.Element, "configuration", null);
            XmlNode modules = xdoc.CreateNode(XmlNodeType.Element, "modules", null);

            foreach(String moduleName in moduleNames){
                XmlNode module = xdoc.CreateNode(XmlNodeType.Element, "module", null);
                XmlAttribute attr = xdoc.CreateAttribute("name");
                attr.Value = moduleName;
                module.Attributes.Append(attr);

                XmlNode enabled = xdoc.CreateElement("enabled");
                enabled.InnerText = "false";

                module.AppendChild(enabled);

                modules.AppendChild(module);

                settings[moduleName] = new Dictionary<string, string>();
                settings[moduleName]["enabled"] = "false";

            }
            root.AppendChild(modules);
            xdoc.AppendChild(root);

            configFile = await storageFolder.CreateFileAsync(configFileName, CreationCollisionOption.ReplaceExisting);
            var writeStream = await configFile.OpenStreamForWriteAsync();
            xdoc.Save(writeStream);
            writeStream.Flush();
            writeStream.Dispose();
        }
    }
}
