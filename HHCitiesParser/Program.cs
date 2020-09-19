using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HHCitiesParser
{
    public class Place
    {
        public string ID;
        public string Name;
        public List<Place> SubPlaces = new List<Place>();
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.Load(@"C:\Users\Luka\Desktop\Cities.xml", Encoding.UTF8);
            List<Place> Places = new List<Place>();
            foreach (HtmlAgilityPack.HtmlNode item in document.DocumentNode.ChildNodes[0].ChildNodes[0].ChildNodes)
            {
                if (item.Name == "div")
                {
                    Place place = new Place();
                    GetNodeRegions(item, place);
                    Places.Add(place);
                }
            }
            foreach (var item in Places)
            {

                Console.WriteLine(string.Join(" , ", item.SubPlaces.Select(t => t.Name)));
                Console.WriteLine(new string('-', 50));
            }
            Console.ReadLine();
        }

        private static void GetNodeRegions(HtmlAgilityPack.HtmlNode node, Place place)
        {
            place.ID = node.Attributes.First(t => t.Name == "data-id").Value;
            place.Name = node.ChildNodes[1].InnerText.Trim();
            node.ChildNodes.RemoveAt(1);
            foreach (var item in node.ChildNodes)
            {
                Place p = new Place();
                if (item.Name == "div")
                {
                    if (item.Attributes.FirstOrDefault(t => t.Name == "data-qa")?.Value == "bloko-tree-selector-items")
                    {
                        foreach (var it in item.ChildNodes)
                        {
                            if (it.Name == "div")
                            {
                                Place p1 = new Place();
                                GetNodeRegions(it, p1);
                                place.SubPlaces.Add(p1);
                            }
                        }
                    }
                    else
                    {
                        GetNodeRegions(item, p);
                        place.SubPlaces.Add(p);
                    }
                }
            }
        }
    }
}
