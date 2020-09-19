using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HH_Parser
{
    public static class HtmlHelper
    {
        public static string FindElementByAttributeAndGetInnerText(this HtmlAgilityPack.HtmlNode htmlDocument, string TagName, string AttributeName, string AttributeValue)
        {
            foreach (var item in GetAllNodes(htmlDocument))
            {
                if(item.Name==TagName&&item.Attributes.FirstOrDefault(t=>t.Name==AttributeName)?.Value==AttributeValue)
                {
                    return item.InnerText; 
                }
            }
            return "";
        }
        public static string FindElementByAttributeAndGetInnerText(this HtmlAgilityPack.HtmlNode htmlDocument, string TagName, string AttributeName, string AttributeValue,int skip)
        {
            foreach (var item in GetAllNodes(htmlDocument))
            {
                if (item.Name == TagName && item.Attributes.FirstOrDefault(t => t.Name == AttributeName)?.Value == AttributeValue)
                {
                    if (skip == 0)
                        return item.InnerText;
                    else skip--;
                }
            }
            return "";
        }
        public static HtmlAgilityPack.HtmlNode FindElementByAttributeAndGetNode(this HtmlAgilityPack.HtmlNode htmlDocument, string TagName, string AttributeName, string AttributeValue, int Skip=0)
        {
            foreach (var item in GetAllNodes(htmlDocument))
            {
                if (item.Name == TagName && item.Attributes.FirstOrDefault(t => t.Name == AttributeName)?.Value == AttributeValue)
                {
                    if (Skip == 0)
                        return item;
                    else Skip--;
                }
            }
            return null;
        }
        public static HtmlAgilityPack.HtmlNode FindElementByTagNameAndGetNode(this HtmlAgilityPack.HtmlNode htmlDocument, string TagName,  int Skip = 0)
        {
            foreach (var item in GetAllNodes(htmlDocument))
            {
                if (item.Name == TagName)
                {
                    if (Skip == 0)
                        return item;
                    else Skip--;
                }
            }
            return null;
        }
        public static List<HtmlAgilityPack.HtmlNode> FindElementByAttributeAndGetNodes(this HtmlAgilityPack.HtmlNode htmlDocument, string TagName, string AttributeName, string AttributeValue)
        {
            List<HtmlAgilityPack.HtmlNode> nodes = new List<HtmlAgilityPack.HtmlNode>();
            foreach (var item in GetAllNodes(htmlDocument))
            {
                if (item.Name == TagName && item.Attributes.FirstOrDefault(t => t.Name == AttributeName)?.Value == AttributeValue)
                {
                    nodes.Add(item);
                }
            }
            return nodes;
        }
        public static string FindElementByAttributeAndInnerTextAndGetXpath(this HtmlAgilityPack.HtmlNode htmlDocument, string TagName, string AttributeName, string AttributeValue,string Text)
        {
            foreach (var item in GetAllNodes(htmlDocument))
            {
                if (item.Name == TagName && item.Attributes.FirstOrDefault(t => t.Name == AttributeName)?.Value == AttributeValue)
                {
                    if( item.InnerText.Trim().ToUpper() == Text.ToUpper())
                    return item.XPath;
                }
            }
            return "";
        }
        public static HtmlAgilityPack.HtmlNode FindElementByAttributeAndInnerTextAndGetNode(this HtmlAgilityPack.HtmlNode htmlDocument, string TagName, string AttributeName, string AttributeValue, string Text)
        {
            foreach (var item in GetAllNodes(htmlDocument))
            {
                if (item.Name == TagName && item.Attributes.FirstOrDefault(t => t.Name == AttributeName)?.Value == AttributeValue)
                {
                    if (item.InnerText.Trim().ToUpper() == Text.ToUpper())
                    {
                        return item;
                    }
                }
            }
            return null;
        }
        public static HtmlAgilityPack.HtmlNode FindElementByTagNameAndInnerText(this HtmlAgilityPack.HtmlNode htmlDocument, string TagName, string Text)
        {
            foreach (var item in GetAllNodes(htmlDocument))
            {
                if (item.Name == TagName)
                {
                    if (item.InnerText.ToUpper().Contains( Text.ToUpper()))
                        return item;
                }
            }
            return null;
        }
        public static string FindElementByAttributeAndGetXpath(this HtmlAgilityPack.HtmlNode htmlDocument, string TagName, string AttributeName, string AttributeValue)
        {
            foreach (var item in GetAllNodes(htmlDocument))
            {
                if (item.Name == TagName && item.Attributes.FirstOrDefault(t => t.Name == AttributeName)?.Value == AttributeValue)
                {
                        return item.XPath;
                }
            }
            return "";
        }
        public static string FindElementByAttributeAndGetXpath(this HtmlAgilityPack.HtmlNode htmlDocument, string TagName, string AttributeName, string AttributeValue,int skip)
        {
            foreach (var item in GetAllNodes(htmlDocument))
            {
                if (item.Name == TagName && item.Attributes.FirstOrDefault(t => t.Name == AttributeName)?.Value == AttributeValue)
                {
                    if (skip == 0)
                        return item.XPath;
                    else skip--;
                }
            }
            return "";
        }
        public static string FindElementByAttributeAndGetAnotherAttributeValue(this HtmlAgilityPack.HtmlNode htmlDocument, string TagName, string AttributeName, string AttributeValue,string TargetAttribute)
        {
            foreach (var item in GetAllNodes(htmlDocument))
            {
                if (item.Name == TagName && item.Attributes.FirstOrDefault(t => t.Name == AttributeName)?.Value == AttributeValue)
                {
                    return item.Attributes.FirstOrDefault(t => t.Name == TargetAttribute)?.Value;
                }
            }
            return "";
        }
        public static List<HtmlAgilityPack.HtmlNode> GetAllNodes(HtmlAgilityPack.HtmlNode htmlNode)
        {
            List<HtmlAgilityPack.HtmlNode> htmlNodes = new List<HtmlAgilityPack.HtmlNode>();
            htmlNodes.Add(htmlNode);
            foreach (var item in htmlNode.ChildNodes)
            {
                htmlNodes.AddRange(GetAllNodes(item));
            }
            return htmlNodes;
        }
    }
}
