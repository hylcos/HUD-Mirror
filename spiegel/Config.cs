using System;
using System.Collections.Generic;
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
        public enum ConfigType { googleCalendarKey, city};



        private const string configFileName = "configuration.xml";
        private StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        private StorageFile configFile;

        public Dictionary<ConfigType, string> settings;

        public Config()
        {
            settings = new Dictionary<ConfigType, string>();
        }

        public async Task LoadFromFile()
        {
            IRandomAccessStream readStream;

            configFile = await storageFolder.GetFileAsync(configFileName);
            readStream = await configFile.OpenAsync(FileAccessMode.Read);

            XmlDocument xdoc = new XmlDocument();
            try {
                xdoc.Load(readStream.AsStreamForRead());
            }
            catch (Exception e)
            {

            }

            readStream.Dispose();



            XmlNodeList element = xdoc.GetElementsByTagName("configuration");
            

            foreach (ConfigType configType in Enum.GetValues(typeof(ConfigType)))
            {
                string configTypeName = Enum.GetName(typeof(ConfigType), configType);
                foreach (XmlNode xn in element)
                {
                    foreach (XmlNode x in xn)
                    {
                        if (x.Name.Equals(configTypeName))
                        {
                            settings[configType] = x.InnerText;
                        }
                    }
                }
            }

        }

        public async void storeToFile()
        {
            XmlDocument configXml = new XmlDocument();

            XmlElement root = configXml.DocumentElement;

            XmlElement configRoot = configXml.CreateElement("configuration");
            configXml.AppendChild(configRoot);

            foreach (KeyValuePair<ConfigType, string> setting in settings)
            {
                string settingName = Enum.GetName(typeof(ConfigType), setting.Key);

                XmlElement element = configXml.CreateElement(settingName);
                XmlText elementText = configXml.CreateTextNode(setting.Value);
                element.AppendChild(elementText);
                configRoot.AppendChild(element);
            }

            await storeXmlToFile(configXml);
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
