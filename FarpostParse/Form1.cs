using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace FarpostParse
{
    public partial class Form1 : Form
    {
        private List<Shop> shops;

        public Form1()
        {
            InitializeComponent();
            shops = new List<Shop>();
            GetNameShop();
            
        }
        
        private void Open_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if(fbd.ShowDialog() == DialogResult.OK)
            {
                path.Text = fbd.SelectedPath;
            }
        }

        private void GetNameShop()
        {
            WebClient client = new WebClient();
            
            var url = "http://vladivostok.farpost.ru/clothes/?owner=1217732";
            Stream data = client.OpenRead(url);
            StreamReader reader = new StreamReader(data, Encoding.GetEncoding("windows-1251"));
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();
            HtmlAgilityPack.HtmlDocument page=new HtmlAgilityPack.HtmlDocument();
            page.LoadHtml(s);
            //if (page == null) return null;
            var shopUl = page.DocumentNode.SelectNodes("//ul[contains(concat(' ', @id, ' '), ' owner_list ')]")[1];
            foreach (var il in shopUl.ChildNodes)
            {
                
                if (!string.IsNullOrWhiteSpace(il.InnerHtml))
                {
                    if (!il.InnerHtml.Contains("onclick"))
                    {
                        var link = il.ChildNodes[0].Attributes["href"].Value;
                        //<li><a href="/clothes/?owner=1453008" id="owner1453008">ИП Лиманов А.Н.</a><small>&nbsp;&nbsp;6850</small></li>
                        var title = il.ChildNodes[0].InnerText;
                        var id = link.Substring(link.IndexOf("owner") + 6);
                        link = "http://vladivostok.farpost.ru/" + link;
                        var countProduct = Regex.Replace(il.ChildNodes[1].InnerText, @"[^\d]", "");
                        shops.Add(new Shop() { Id = id, Name = title, Url = link, CountProduct=countProduct });
                        nameCB.Items.Add(title);
                    }
                }
            }
            //http://vladivostok.farpost.ru/clothes/
            nameCB.SelectedIndex = 1;
            label2.Text = "Всего товаров " + shops[1].CountProduct;
            label2.Refresh();
        }

        private void Start_Click(object sender, EventArgs e)
        {
            var num = nameCB.SelectedIndex;
            if (num > -1)
            {
                var shop = shops[num];
                label2.Text = "Осталось "+shop.CountProduct;
                var countPage = Convert.ToInt32(shop.CountProduct)/50+1;
                //загрузка страницы магазина
                var catNew = new List<Category>();
                var products = new List<Product>();
                var productsAdd = new List<Product>();
                var categories = new List<Category>();
                if (File.Exists(path.Text + @"\categories.xml"))
                {
                    var temp = ReadXml(path.Text + @"\categories.xml");
                    if (temp.Any())
                        categories.AddRange(temp);
                }
                 
                //if (File.Exists(path.Text + @"\products.xml"))
                //{
                //    var temp = ReadXmlPr(path.Text + @"\products.xml");
                //    //if (temp.Any())
                //    //    productsAdd.AddRange(temp);
                //}
                List<TimeSpan> sts = new List<TimeSpan>();
                Stopwatch st = new Stopwatch();
                st.Start();
               for (int j = 0; j < countPage; j++)
               {
                    WebClient client = new WebClient();
                    Stream data = client.OpenRead(j == 0 ? shop.Url : shop.Url + "&page=" + (j + 1).ToString());
                    StreamReader reader = new StreamReader(data, Encoding.GetEncoding("windows-1251"));
                    string s = reader.ReadToEnd();
                    data.Close();
                    reader.Close();
                    var shopHtml = new HtmlAgilityPack.HtmlDocument();
                    shopHtml.LoadHtml(s);

                    sts.Add(st.Elapsed);

                    var prLinks = new List<string>();
                    var productsTr =
                        shopHtml.DocumentNode.SelectNodes(
                            "//table[contains(concat(' ', @class, ' '), ' viewdirBulletinTable pageableContent ')]/tbody/tr");
                    //if (j == 0)
                    //{

                    //    //<span class="pagebarInner"><strong class="page">1</strong><a href="/clothes/women/?owner=1453008&amp;page=2" class="page nextnumber">2</a>
                    //    //<a href="/clothes/women/?owner=1453008&amp;page=3" class="page ">3</a><a href="/clothes/women/?owner=1453008&amp;page=4" class="page ">4</a><a href="/clothes/women/?owner=1453008&amp;page=5" class="page ">5</a><a href="/clothes/women/?owner=1453008&amp;page=6" class="page ">6</a><a href="/clothes/women/?owner=1453008&amp;page=7" class="page ">7</a><a href="/clothes/women/?owner=1453008&amp;page=8" class="page ">8</a>
                    //    //<span>...</span>111 страниц			</span>
                    //    var count = shopHtml.DocumentNode.SelectNodes("//*[@id='bulletins']/div[2]/div/span[contains(concat(' ', @class, ' '), ' pagebarInner ')]")[0];
                    //    var t = count.InnerText.Replace("\t", "");
                    //    //*[@id="bulletins"]/div[2]/div/span[1]
                    //    if (!t.Contains("8"))
                    //    {
                    //        var str = Regex.Replace(t, @"[^\d]", "");
                    //        var countP = Convert.ToInt32(str.Remove(0, str.Length - 1));
                    //    }
                    //    else
                    //    {
                    //        var str = t.Substring(t.IndexOf("8") + 1);
                    //        str = Regex.Replace(str, @"[^\d]", "");
                    //        //if (str != "")
                    //        //    int countP = Convert.ToInt32(str);
                    //        //else
                    //        //    int countP = 8;
                    //    }

                    //}
                    
                    foreach (var product in productsTr)
                    {
                        if (!product.InnerHtml.Contains("<table"))
                        {
                            //<a class="bulletinLink" href="http://vladivostok.farpost.ru/good/platje-victoria-orhideja-molochnoe-134944159.html">Платье Victoria Орхидея молочное</a>
                            var a =
                                product.SelectNodes("//a[contains(concat(' ', @class, ' '), ' bulletinLink ')]");
                            //prLinks.Add(a.Attributes["href"].Value);
                            foreach (var l in a)
                            {
                                prLinks.Add(l.Attributes["href"].Value);
                            }
                            HashSet<string> sd = new HashSet<string>(prLinks);
                            prLinks = sd.ToList();
                        }
                    }
                    sts.Add(st.Elapsed);
                    foreach (var url in prLinks)
                    {
                        var wc = new WebClient();
                        Stream d = wc.OpenRead(url);
                        var r = new StreamReader(d, Encoding.GetEncoding("windows-1251"));
                        string page = r.ReadToEnd();
                        d.Close();
                        r.Close();
                        var productHtml = new HtmlAgilityPack.HtmlDocument();
                        productHtml.LoadHtml(page);
                        Product pr = new Product()
                        {
                            Id = Guid.NewGuid().ToString(),
                            StoreId = shops[num].Id.Trim(),
                            StoreName = shops[num].Name.Trim()
                        };
                        //<div id="breadcrumbs"><a href="/">Фарпост Барахолка</a>		&gt;	<a href="/clothes/">Одежда, обувь и аксессуары</a>
                        //&gt;	<a href="/clothes/men/">Мужская одежда и обувь</a>&gt;	<a href="/clothes/men/shoes/">Обувь</a>&gt;	<span>Кеды DVS Landmark</span>
                        var categoryA =
                            productHtml.DocumentNode.SelectNodes(
                                "//div[contains(concat(' ', @id, ' '), ' breadcrumbs ')]/a");
                        var parentId = "";
                        var parentName = "";

                        sts.Add(st.Elapsed);
                        foreach (var categ in categoryA)
                        {
                            var c = categories.FirstOrDefault(x => x.Name == categ.InnerText);
                            if (c == null)
                            {
                                var id = Guid.NewGuid().ToString();
                                var cat = new Category()
                                {
                                    Id = id,
                                    Name = categ.InnerText.Trim(),
                                    ParentId = parentId,
                                    ParentName = parentName
                                };
                                categories.Add(cat);
                                catNew.Add(cat);
                                parentId = id;
                            }
                            else
                            {
                                parentId = c.Id;
                            }
                            parentName = categ.InnerText;
                        }
                        pr.CategoryId = parentId.Trim();
                        pr.CategoryName = parentName;
                        sts.Add(st.Elapsed);
                        var contentDiv =
                            productHtml.DocumentNode.SelectNodes(
                                "//div[contains(concat(' ', @id, ' '), ' content ')]/table/tr/td/div")[0];
                        //<h1 class="subject">Кеды DVS Landmark		<span><nobr>
                        //(<span class="fieldSetCopyHide"><a href="#userInfo" class="ajaxLink" onclick="baza.scrollTo('.ownerInfo', {offset: -20}); return false">
                        //Advance			</a>, </span>Владивосток)</nobr></span></h1>
                        var titleH1 = contentDiv.SelectNodes("//h1[contains(concat(' ', @class, ' '), ' subject ')]")[0];
                        pr.Name =
                            titleH1.InnerHtml.Substring(0, titleH1.InnerHtml.IndexOf("<span"))
                                .Trim()
                                .Replace("\r\n", "")
                                .Replace("\t", "");

                        var priceDiv =
                            contentDiv.SelectNodes("//div[contains(concat(' ', @class, ' '), ' field big ')]/div")[1];

                        if (priceDiv.InnerHtml.Contains("discountButtonFrame"))
                        {
                            pr.Price = Regex.Replace(
                                priceDiv.InnerHtml.Substring(0, priceDiv.InnerHtml.IndexOf("<div")), @"[^\d]", "");
                        }
                        else
                            pr.Price = Regex.Replace(priceDiv.InnerText, @"[^\d]", "");

                        //bulletinText
                        var descriptionP =
                            contentDiv.SelectNodes("//div[contains(concat(' ', @class, ' '), ' bulletinText ')]")[0];
                        if (descriptionP.InnerHtml.Contains("<p>"))
                        {
                            pr.Description =
                                descriptionP.ChildNodes[0].InnerText.Trim().Replace("\r\n", "").Replace("\t", "");
                        }
                        var preview = "";

                        //<img src="http://static.baza.farpost.ru/v/1364896377672_bulletin" imagewidth="640" imageheight="482" style="width: auto; height: 354px; margin: 0px 0px 0px 1px;"></a>

                        //var ds = contentDiv.SelectNodes("//div[contains(concat(' ', @class, ' '), ' imagesEx ')]/a")[0];
                        //var fotoImg = contentDiv.SelectNodes("//div[contains(concat(' ', @class, ' '), ' imagesEx ')]/a/img")[0];
                        var foto = new List<string>();
                        //foto.Add(fotoImg.Attributes["src"].Value);
                        //<div class="bulletinImages"><div class="image">
                        //<img src="http://static.baza.farpost.ru/v/1355615239833_bulletin" imagewidth="426" imageheight="640"></div></div>
                        var fotoDiv =
                            contentDiv.SelectNodes(
                                "//div[contains(concat(' ', @class, ' '), ' bulletinImages ')]/div/img");

                        if (fotoDiv != null)
                        {
                            foreach (var f in fotoDiv)
                            {
                                foto.Add(f.Attributes["src"].Value);
                            }
                        }
                        if (foto.Any())
                        {
                            var pathFolder = path.Text + @"\" + pr.Id + @"\";

                            Directory.CreateDirectory(pathFolder);
                            pr.Photos = new List<string>();
                            for (int i = 0; i < foto.Count; i++)
                            {
                                var image = new WebClient();

                                pr.Photos.Add(pathFolder + i + ".jpg");
                                client.DownloadFile(new Uri(foto[i]), pathFolder + i + ".jpg");
                            }
                        }
                        sts.Add(st.Elapsed);
                        products.Add(pr);
                        if (productsAdd.Any())
                        {
                            label2.Text = "Осталось " + (Convert.ToInt32(shop.CountProduct) - productsAdd.Count - products.Count).ToString();
                            label2.Refresh();
                        }
                        else
                        {
                            label2.Text = "Осталось " + (Convert.ToInt32(shop.CountProduct) - products.Count).ToString();
                            label2.Refresh();
                        }
                        sts.Add(st.Elapsed);
                        if ((Convert.ToInt32(shop.CountProduct) - products.Count) == 0)
                            break;
                    }
                    
                    if ((j == (countPage - 1) || j % 3 == 0) && (countPage > 1 && j != 0))
                    {
                        var sss = new HashSet<string>(products.Select(x => x.Name));
                        var prNew = new List<Product>();
                        foreach (var name in sss)
                        {
                            var pp = products.FirstOrDefault(x => x.Name == name);
                            var pp1 = productsAdd.FirstOrDefault(x => x.Name == name);
                            if (pp != null && pp1 == null)
                                prNew.Add(pp);

                        }

                        SaveProducts(prNew, path.Text + @"\products.xml");
                        SaveCategory(catNew, path.Text + @"\categories.xml");
                        productsAdd.AddRange(prNew);
                        catNew = new List<Category>();
                        products = new List<Product>();
                        st.Stop();
                        var rrrr = st.Elapsed;
                    }
                }
                
            }
        }

        public void SaveCategory(List<Category> c, string fileName)
        {
            XmlTextWriter textWritter = new XmlTextWriter(fileName, Encoding.UTF8);
            textWritter.WriteStartDocument();
            textWritter.WriteStartElement("head");
            textWritter.WriteEndElement();
            textWritter.Close();

            XmlDocument document = new XmlDocument();
            document.Load(fileName);
            XmlNode element = document.CreateElement("Categories");
            document.DocumentElement.AppendChild(element); // указываем родителя
            Parallel.ForEach(c, cat =>
            {
                XmlNode e = document.CreateElement("Category"); // даём имя

                XmlNode subElement1 = document.CreateElement("Id"); // даём имя
                subElement1.InnerText = cat.Id; // и значение
                e.AppendChild(subElement1); // и указываем кому принадлежит

                XmlNode subElement2 = document.CreateElement("Name"); // даём имя
                subElement2.InnerText = cat.Name; // и значение
                e.AppendChild(subElement2); // и указываем кому принадлежит

                XmlNode subElement3 = document.CreateElement("ParentId"); // даём имя
                subElement3.InnerText = cat.ParentId; // и значение
                e.AppendChild(subElement3); // и указываем кому принадлежит

                XmlNode subElement4 = document.CreateElement("ParentName"); // даём имя
                subElement4.InnerText = cat.ParentName; // и значение
                e.AppendChild(subElement4); // и указываем кому принадлежит
                element.AppendChild(e); // и указываем кому принадлежит
            });
            document.Save(fileName);
        }
        public void SaveProducts(List<Product> p, string fileName)
        {
            XmlTextWriter textWritter = new XmlTextWriter(fileName, Encoding.UTF8);
            textWritter.WriteStartDocument();
            textWritter.WriteStartElement("head");
            textWritter.WriteEndElement();
            textWritter.Close();

            XmlDocument document = new XmlDocument();
            document.Load(fileName);
            XmlNode element = document.CreateElement("Products");
            document.DocumentElement.AppendChild(element); // указываем родителя
            //XmlAttribute attribute = document.CreateAttribute("number"); // создаём атрибут
            //attribute.Value = 1; // устанавливаем значение атрибута
            //element.Attributes.Append(attribute); // добавляем атрибут
            Parallel.ForEach(p, pr => { 
            
                XmlNode e = document.CreateElement("Product"); // даём имя

                XmlNode subElement1 = document.CreateElement("Id"); // даём имя
                subElement1.InnerText = pr.Id; // и значение
                e.AppendChild(subElement1); // и указываем кому принадлежит

                XmlNode subElement2 = document.CreateElement("StoreId"); // даём имя
                subElement2.InnerText = pr.StoreId; // и значение
                e.AppendChild(subElement2); // и указываем кому принадлежит

                XmlNode subElement3 = document.CreateElement("StoreName"); // даём имя
                subElement3.InnerText = pr.StoreName; // и значение
                e.AppendChild(subElement3); // и указываем кому принадлежит

                XmlNode subElement4 = document.CreateElement("CategoryId"); // даём имя
                subElement4.InnerText = pr.CategoryId; // и значение
                e.AppendChild(subElement4); // и указываем кому принадлежит

                XmlNode subElement5 = document.CreateElement("CategoryName"); // даём имя
                subElement5.InnerText = pr.CategoryName; // и значение
                e.AppendChild(subElement5); // и указываем кому принадлежит

                XmlNode subElement6 = document.CreateElement("Name"); // даём имя
                subElement6.InnerText = pr.Name; // и значение
                e.AppendChild(subElement6); // и указываем кому принадлежит

                XmlNode subElement7 = document.CreateElement("Description"); // даём имя
                subElement7.InnerText = pr.Description; // и значение
                e.AppendChild(subElement7); // и указываем кому принадлежит

                XmlNode subElement8 = document.CreateElement("Price"); // даём имя
                subElement8.InnerText = pr.Price; // и значение
                e.AppendChild(subElement8); // и указываем кому принадлежит

                XmlNode subElement9 = document.CreateElement("Photos"); // даём имя
                e.AppendChild(subElement9); // и указываем кому принадлежит
                if (pr.Photos != null && pr.Photos.Any())
                {
                    foreach (var photo in pr.Photos)
                    {
                        XmlNode subElement10 = document.CreateElement("Photo"); // даём имя
                        subElement10.InnerText = photo; // и значение
                        subElement9.AppendChild(subElement10); // и указываем кому принадлежит
                    }
                }
                element.AppendChild(e); // и указываем кому принадлежит
            });
            document.Save(fileName);
            
        }
        
        public List<Category> ReadXml(string fileName)
        {
            XmlDocument document = new XmlDocument();
            document.Load(fileName);
            var categories = document.GetElementsByTagName("Category");
            List<Category> cat = new List<Category>();
            Parallel.For(0, categories.Count, i =>
            {
                var t = (XmlElement)document.GetElementsByTagName("Category")[i];
                var id = t.ChildNodes[0].InnerText;
                var name = t.ChildNodes[1].InnerText;
                var parentId = t.ChildNodes[2].InnerText;
                var parentName = t.ChildNodes[3].InnerText;
                cat.Add(new Category() { Id = id, Name = name, ParentId = parentId, ParentName = parentName });

            });
            return cat;
        }

        public List<Product> ReadXmlPr(string fileName)
        {
            XmlDocument document = new XmlDocument();
            document.Load(fileName);
            var products = document.GetElementsByTagName("Product");
            List<Product> pr = new List<Product>();
            Parallel.For(0, products.Count, i =>
            {
                var t = (XmlElement)document.GetElementsByTagName("Product")[i];
                var id = t.ChildNodes[0].InnerText;
                var storeId = t.ChildNodes[1].InnerText;
                var storeName = t.ChildNodes[2].InnerText;
                var categoryId = t.ChildNodes[3].InnerText;
                var categoryName = t.ChildNodes[4].InnerText;
                var name = t.ChildNodes[5].InnerText;
                var description = t.ChildNodes[6].InnerText;
                var price = t.ChildNodes[7].InnerText;
                var Photos = document.GetElementsByTagName("Photo");
                List<string> ph = new List<string>();
                if (Photos != null && Photos.Count>0)
                {
                    for (int j = 0; j < Photos.Count; j++)
                    {
                        var sss = (XmlElement)document.GetElementsByTagName("Photo")[i];
                        ph.Add(sss.InnerText);
                    }
                }
                pr.Add(new Product() { Id = id, Name = name, CategoryId=categoryId, CategoryName=categoryName, Description=description,
                    Price=price, StoreId=storeId,StoreName=storeName,Photos=ph
                });

            });
            return pr;
        }

        private void nameCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            var num = nameCB.SelectedIndex;
            label2.Text = "Всего товаров " + shops[num].CountProduct;
            label2.Refresh();
        }
    }
}
