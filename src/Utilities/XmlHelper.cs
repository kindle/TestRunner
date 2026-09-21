//-------------------------------------------------------------------------------------------------
// <copyright file="XmlHelper.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestRunner.Utilities
{
    using System.Xml;

    /// <summary>
    /// Xml helper
    /// </summary>
    internal class XmlHelper
    {
        /// <summary>
        /// Root namespace for the TestRun node in .trx file
        /// </summary>
        private const string RootNamespace = "xmlns=\"http://microsoft.com/schemas/VisualStudio/TeamTest/2010\"";

        /// <summary>
        /// Get inner text from .rtx file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="queryString">Query string</param>
        /// <returns>Inner text</returns>
        public static string GetInnerTextFromTrx(string fileName, string queryString)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            doc.InnerXml = doc.InnerXml.Replace(RootNamespace, string.Empty);
            var node = doc.SelectSingleNode(queryString);
            return node == null ? string.Empty : node.InnerText;
        }

        public static string GetInnerTextFromXml(string innerXml, string queryString)
        {
            XmlDocument doc = new XmlDocument();
            doc.InnerXml = innerXml;
            var node = doc.SelectSingleNode(queryString);
            return node == null ? string.Empty : node.InnerText;
        }

        public static string GetInnerXmlFromTrx(string fileName, string queryString)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            doc.InnerXml = doc.InnerXml.Replace(RootNamespace, string.Empty);
            var node = doc.SelectSingleNode(queryString);
            return node == null ? string.Empty : node.InnerXml;
        }

        /// <summary>
        /// Get attribute value from .rtx file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="queryString">Query string</param>
        /// <param name="attributeName">Attribute name</param>
        /// <returns>Attribute string value</returns>
        public static string GetAttributeValueFromTrx(string fileName, string queryString, string attributeName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            doc.InnerXml = doc.InnerXml.Replace(RootNamespace, string.Empty);
            var node = doc.SelectSingleNode(queryString);
            return (node == null || node.Attributes[attributeName] == null) ? "" : node.Attributes[attributeName].Value;
        }

        public static XmlNodeList GetNodeListFromTrx(string fileName, string queryString)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            doc.InnerXml = doc.InnerXml.Replace(RootNamespace, string.Empty);
            return doc.SelectNodes(queryString);
        }

        public static XmlNode GetNodeFromTrx(string fileName, string queryString)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            doc.InnerXml = doc.InnerXml.Replace(RootNamespace, string.Empty);
            return doc.SelectSingleNode(queryString);
        }

        public static XmlNode GetNodeFromTrxById(XmlNode node, string queryString)
        {
            if (node == null)
            {
                return null;
            }

            XmlDocument doc = new XmlDocument();
            doc.InnerXml = node.OuterXml;
            return doc.SelectSingleNode(queryString);
        }

        public static string GetAttributeValueFromXml(string innerXml, string queryString, string attributeName)
        {
            XmlDocument doc = new XmlDocument();
            doc.InnerXml = innerXml;
            var node = doc.SelectSingleNode(queryString);
            return (node == null || node.Attributes[attributeName] == null) ? "" : node.Attributes[attributeName].Value;
        }
    }
}
