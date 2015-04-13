using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace ByteFlood
{
    public static class HAPHelper
    {
        public static HtmlNode[] GetElementsByClassName(this HtmlNode node, string name)
        {
            List<HtmlNode> nodes = new List<HtmlNode>();

            if (node.GetAttributeValue("class", "").Contains(name))
            {
                nodes.Add(node);
            }

            if (node.ChildNodes.Count > 0)
            {
                foreach (HtmlNode n in node.ChildNodes)
                {
                    nodes.AddRange(GetElementsByClassName(n, name));
                }
            }

            return nodes.ToArray();
        }

        public static HtmlNode[] GetElementsByAttributeValue(this HtmlNode node, string name, string value)
        {
            List<HtmlNode> nodes = new List<HtmlNode>();

            if (node.GetAttributeValue(name, null) == value)
            {
                nodes.Add(node);
            }

            if (node.ChildNodes.Count > 0)
            {
                foreach (HtmlNode n in node.ChildNodes)
                {
                    nodes.AddRange(GetElementsByAttributeValue(n, name, value));
                }
            }

            return nodes.ToArray();
        }

        public static HtmlNode[] GetElementsByTagName(this HtmlNode node, string name)
        {
            List<HtmlNode> nodes = new List<HtmlNode>();

            if (node.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                nodes.Add(node);
            }

            if (node.ChildNodes.Count > 0)
            {
                foreach (HtmlNode n in node.ChildNodes)
                {
                    nodes.AddRange(GetElementsByTagName(n, name));
                }
            }

            return nodes.ToArray();
        }

        public static HtmlNode GetElementById(this HtmlNode node, string id)
        {
            if (node.Id == id)
            {
                return node;
            }
            else
            {
                foreach (HtmlNode n in node.ChildNodes)
                {
                    var a = GetElementById(n, id);
                    if (a != null)
                    {
                        return a;
                    }
                }
                return null;
            }
        }
    }
}