﻿using System;
using System.Xml.Linq;

namespace OSDiagTool
{
    class OSServiceConfigFileParser
    {
        private string _serviceName;
        private string _filepath;
        private bool _isFileLoaded;

        private int _logLevel;
        private string _logFilePath;
        private string _logServerFilePath;

        // if needed, we can fetch more info, but this should be enough for now

        public OSServiceConfigFileParser(string serviceName, string filepath)
        {
            _serviceName = serviceName;
            _filepath = filepath;
            _isFileLoaded = false;
        }

        public OSServiceConfigFileParser(string serviceName, string filepath, bool forceLoad) : this(serviceName, filepath)
        {
            if (forceLoad) {
                LoadConfFileInfo();
            }
                
        }

        public string LogServerAPIAndIdentityFilePath {
            get {
                // lazy load
                if (!_isFileLoaded)
                    LoadServerAPIAndIdentityLogPath(_filepath);

                return _logServerFilePath;
            }
        }

        public string LogFilePath
        {
            get
            {
                // lazy load
                if (!_isFileLoaded)
                    LoadConfFileInfo();

                return _logFilePath;
            }
        }

        public int LogLevel
        {
            get
            {
                // lazy load
                if (!_isFileLoaded)
                    LoadConfFileInfo();

                return _logLevel;
            }
        }

        // Loads all the required information from the OS service conf file
        private void LoadConfFileInfo()
        {
            if (_isFileLoaded)
                return;

            try
            {
                XElement confXml = XElement.Load(_filepath);
                                
                LoadLogFilePathFromConfRoot(confXml);
                LoadLogLevelFromConfRoot(confXml);

                _isFileLoaded = true;
            }
            catch (Exception e) {
                FileLogger.LogError("Attempted to load OS Configuration file but failed:", e.Message + e.StackTrace);
            }
        }

        // Obtain the location of the service log file
        private void LoadLogFilePathFromConfRoot(XElement root)
        {
            // Element chaining can be done because Element is null safe - if the element doesn't exist, it returns an empty element
            foreach (XElement el in root.Element("system.diagnostics").Element("trace").Element("listeners").Elements("add"))
            {
                XAttribute attrName = el.Attribute("name");

                if (attrName != null && attrName.Value == "MyListener")
                {
                    XAttribute attrData = el.Attribute("initializeData");
                    if (attrData != null)
                    {
                        _logFilePath = attrData.Value;
                    }
                }
            }
        }


        // Obtain the value for LogLevel of the service
        private void LoadLogLevelFromConfRoot(XElement root)
        {
            foreach (XElement el in root.Element("system.diagnostics").Element("switches").Elements("add"))
            {
                XAttribute attrName = el.Attribute("name");

                if (attrName != null && attrName.Value == "LogLevel")
                {
                    XAttribute attrValue = el.Attribute("value");
                    if (attrValue != null)
                    {
                        int.TryParse(attrValue.Value, out _logLevel);
                    }
                }
            }
        }

        private void LoadServerAPIAndIdentityLogPath(string _filepath) {

            try {
                var xml = XDocument.Load(_filepath);

                XNamespace ns = xml.Root.GetDefaultNamespace();

                foreach (XElement el in xml.Descendants(ns + "targets").Elements(ns + "target")) {
                    _logServerFilePath = el.Attribute("fileName").Value.ToString();
                    break;
                }

            } catch (Exception e) {
                FileLogger.LogError("Failed to parse Server API / Identity xml ", e.Message + e.StackTrace);
            }
        }
    }
}
