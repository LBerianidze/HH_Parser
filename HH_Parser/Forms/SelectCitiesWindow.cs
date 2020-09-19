using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace HH_Parser
{
    public partial class SelectCitiesWindow : Form
    {
        private List<Place> Places;
        private List<Place> AllPlaces = new List<Place>();
        public SelectCitiesWindow()
        {
            InitializeComponent();
            var serializer = new XmlSerializer(typeof(List<Place>));
            Places = (List<Place>)serializer.Deserialize(new StreamReader("Cities.xml", Encoding.UTF8));
            foreach (var country in Places)
            {
                var node = new TreeNode
                {
                    Tag = country.ID,
                    Text = country.Name,
                    Checked = country.Checked
                };
                FillNode(node, country.SubPlaces);
                CitiesTreeView.Nodes.Add(node);
            }
            foreach (var item in Places)
            {
                SetParents(item);
            }
            foreach (var item in Places)
            {
                AllPlaces.Add(item);
                FillPlaces(item);
            }
        }
        private void SetParents(Place place)
        {
            foreach (var item in place.SubPlaces)
            {
                item.ParentPlace = place;
                SetParents(item);
            }
        }
        private void FillPlaces(Place place)
        {
            foreach (var item in place.SubPlaces)
            {
                AllPlaces.Add(item);
                FillPlaces(item);
            }
        }
        private void FillNode(TreeNode node, List<Place> places)
        {
            foreach (var item in places)
            {
                var tr = new TreeNode
                {
                    Tag = item.ID,
                    Text = item.Name
                };
                FillNode(tr, item.SubPlaces);
                node.Nodes.Add(tr);
            }
        }
        private void CitiesTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            CitiesTreeView.BeginUpdate();
            AllPlaces.Find(t => t.ID == (string)e.Node.Tag).Checked = e.Node.Checked;
            foreach (TreeNode item in e.Node.Nodes)
            {
                item.Checked = e.Node.Checked;
            }
            if (e.Node.Parent != null)
            {
                if (!e.Node.Checked)
                {
                    CitiesTreeView.AfterCheck -= CitiesTreeView_AfterCheck;
                    e.Node.Parent.Checked = false;
                    AllPlaces.Find(t => t.ID == (string)e.Node.Parent.Tag).Checked = false;
                    CitiesTreeView.AfterCheck += CitiesTreeView_AfterCheck;
                }
                else if (e.Node.Parent.Nodes.Cast<TreeNode>().Count(t => t.Checked) == e.Node.Parent.Nodes.Count && !filteron)
                {
                    CitiesTreeView.AfterCheck -= CitiesTreeView_AfterCheck;
                    e.Node.Parent.Checked = true;
                    AllPlaces.Find(t => t.ID == (string)e.Node.Parent.Tag).Checked = true;
                    CitiesTreeView.AfterCheck += CitiesTreeView_AfterCheck;
                }
            }
            CitiesTreeView.EndUpdate();
        }

        private List<Place> FilteredPlaces;
        private bool filteron = false;
        private void CitySearchTB_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CitySearchTB.Text))
            {
                filteron = false;
            }
            else
            {
                filteron = true;
            }
            CitiesTreeView.Scrollable = false;
            CitiesTreeView.Nodes.Clear();
            FilteredPlaces = AllPlaces.Where(t => t.Name.ToUpper().Contains(CitySearchTB.Text.ToUpper())).ToList();
            foreach (var item in AllPlaces.Where(t => t.Name.ToUpper().Contains(CitySearchTB.Text.ToUpper())).ToList())
            {
                if (item.ParentPlace != null)
                {
                    AddParentNode(item.ParentPlace);
                    FilteredPlaces.Add(item.ParentPlace);
                }
            }
            FilteredPlaces = FilteredPlaces.GroupBy(t => t).Select(f => f.FirstOrDefault()).ToList();
            foreach (var country in Places)
            {
                if (FilteredPlaces.Contains(country))
                {
                    var node = new TreeNode
                    {
                        Tag = country.ID,
                        Text = country.Name,
                        Checked = country.Checked
                    };
                    FillNode1(node, country.SubPlaces);
                    CitiesTreeView.Nodes.Add(node);
                }
            }
            CitiesTreeView.Scrollable = true;

        }
        private void FillNode1(TreeNode node, List<Place> places)
        {
            foreach (var item in places)
            {
                if (FilteredPlaces.Contains(item))
                {
                    var tr = new TreeNode
                    {
                        Tag = item.ID,
                        Text = item.Name,
                        Checked = item.Checked
                    };
                    FillNode1(tr, item.SubPlaces);
                    node.Nodes.Add(tr);
                }
            }
        }
        private void AddParentNode(Place place)
        {
            if (place.ParentPlace != null)
            {
                AddParentNode(place.ParentPlace);
                FilteredPlaces.Add(place.ParentPlace);
            }
        }

        private List<Place> result;
        public new List<Place> ShowDialog(IWin32Window owner)
        {
            base.ShowDialog(owner);
            return result;

        }

        private void FillResult(Place p)
        {
            foreach (var item in p.SubPlaces)
            {
                if (item.Checked)
                {
                    result.Add(item);
                }
                else
                {
                    FillResult(item);
                }
            }

        }
        private void AcceptCities_Click(object sender, EventArgs e)
        {
            result = new List<Place>();
            foreach (var item in Places)
            {
                if (item.Checked)
                {
                    result.Add(item);
                }
                else
                {
                    FillResult(item);
                }
            }
            Hide();
        }

        private void CancelCities_Click(object sender, EventArgs e)
        {
            Hide();
        }
    }
    public class Place
    {
        public string ID;
        public string Name;
        public bool Checked;
        [JsonIgnore]
        public Place ParentPlace;
        [JsonIgnore]
        public List<Place> SubPlaces = new List<Place>();
    }
    public class Metros
    {
        public List<Metro> MetroList;
        public Metros()
        {
            var serializer = new JsonSerializer();
            Newtonsoft.Json.JsonReader reader = new Newtonsoft.Json.JsonTextReader(new StreamReader("metro.json", Encoding.UTF8));
            MetroList = (List<Metro>)serializer.Deserialize(reader, typeof(List<Metro>));
        }

    }
    public class Metro
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string url { get; set; }
        public List<Line> Lines { get; set; }
    }
    public class Line
    {
        public int ID { get; set; }
        public string hex_color { get; set; }
        public string Name { get; set; }
        public List<Station> Stations { get; set; }
    }
    public class Station
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Order { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
    }
}
