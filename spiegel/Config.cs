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

        public Dictionary<string, Dictionary<string,string >> settings;

        public Config(List<string> moduleNames)
        {
            this.moduleNames = moduleNames;
            settings = new Dictionary<string,Dictionary<string,string>>();
        }

        public async Task LoadFromFile()
        {
            XmlDocument xdoc = new XmlDocument();

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
    }
}
