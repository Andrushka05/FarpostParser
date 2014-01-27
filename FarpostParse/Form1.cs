using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Reflection;
using System.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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
            var fbd = new FolderBrowserDialog();
            if(fbd.ShowDialog() == DialogResult.OK)
            {
                path.Text = fbd.SelectedPath;
            }
        }

        private void GetNameShop()
        {
            var client = new WebClient();
            
            var url = "http://vladivostok.farpost.ru/clothes/?owner=1217732";
            var data = client.OpenRead(url);
            var reader = new StreamReader(data, Encoding.GetEncoding("windows-1251"));
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();
            var page=new HtmlAgilityPack.HtmlDocument();
            page.LoadHtml(s);
            
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
                var catNew = new List<Category>();
                var products = new List<Product>();
                var categories = new List<Category>();
                //if (File.Exists(path.Text + @"\categories.xml"))
                //{
                //    var temp = ReadXml(path.Text + @"\categories.xml");
                //    if (temp.Any())
                //        categories.AddRange(temp);
                //}
                 
                for (int j = 0; j < countPage; j++)
               {
                    var client = new WebClient();
                    Stream data = client.OpenRead(j == 0 ? shop.Url : shop.Url + "&page=" + (j + 1).ToString());
                    var reader = new StreamReader(data, Encoding.GetEncoding("windows-1251"));
                    string s = reader.ReadToEnd();
                    data.Close();
                    reader.Close();
                    var shopHtml = new HtmlAgilityPack.HtmlDocument();
                    shopHtml.LoadHtml(s);

                    var prLinks = new List<string>();
                    var productsTr =
                        shopHtml.DocumentNode.SelectNodes(
                            "//table[contains(concat(' ', @class, ' '), ' viewdirBulletinTable pageableContent ')]/tbody/tr");
                    
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
                            break;
                        }
                    }
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
												//var contentDiv =
												//		productHtml.DocumentNode.SelectNodes(
												//				"//div[contains(concat(' ', @id, ' '), ' content ')]/table/tr/td/div")[0];
                        //<h1 class="subject">Кеды DVS Landmark		<span><nobr>
                        //(<span class="fieldSetCopyHide"><a href="#userInfo" class="ajaxLink" onclick="baza.scrollTo('.ownerInfo', {offset: -20}); return false">
                        //Advance			</a>, </span>Владивосток)</nobr></span></h1>
												var titleH1 = productHtml.DocumentNode.SelectNodes("//h1[contains(concat(' ', @class, ' '), ' subject ')]")[0];
                        pr.Name =
                            titleH1.InnerHtml.Substring(0, titleH1.InnerHtml.IndexOf("<span"))
                                .Trim()
                                .Replace("\r\n", "")
                                .Replace("\t", "");

                        var priceDiv =
														productHtml.DocumentNode.SelectNodes("//div[contains(concat(' ', @class, ' '), ' field big ')]/div")[1];

                        if (priceDiv.InnerHtml.Contains("discountButtonFrame"))
                        {
                            pr.Price = Regex.Replace(
                                priceDiv.InnerHtml.Substring(0, priceDiv.InnerHtml.IndexOf("<div")), @"[^\d]", "");
                        }
                        else
                            pr.Price = Regex.Replace(priceDiv.InnerText, @"[^\d]", "");

                        //bulletinText
                        var descriptionP =
														productHtml.DocumentNode.SelectNodes("//div[contains(concat(' ', @class, ' '), ' bulletinText ')]")[0];
                        if (descriptionP.InnerHtml.Contains("<p>"))
                        {
                            //<p>Цвет: зеленый;<br>Размер (ДхШхВ) см.: 180x55:x;<br>Материал: Хлопок;<br>Артикул: 031;<br><br>Бесплатная доставка при покупке сумки</p>
                            var str = descriptionP.InnerHtml;
                            var begin = str.IndexOf("<p>");
                            var end = str.IndexOf("</p>");
                            var res = str.Substring(begin + 3, end - begin - 3).Trim().Replace("<br>","");
                            pr.Description =res;
                        }
                        var preview = "";

                        //<img src="http://static.baza.farpost.ru/v/1364896377672_bulletin" imagewidth="640" imageheight="482" style="width: auto; height: 354px; margin: 0px 0px 0px 1px;"></a>
                        var foto = new List<string>();
                        //<img src="http://static.baza.farpost.ru/v/1355615239833_bulletin" imagewidth="426" imageheight="640"></div></div>
                        var fotoDiv =
														productHtml.DocumentNode.SelectNodes(
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
                                pr.Photos.Add(pr.Id+ @"/" + i + ".jpg");
                                client.DownloadFile(new Uri(foto[i]), pathFolder + i + ".jpg");
                            }
                        }
                        
                        products.Add(pr);
                        label2.Text = "Осталось " + (Convert.ToInt32(shop.CountProduct) - products.Count).ToString();
                        label2.Refresh();
                        
                        if ((Convert.ToInt32(shop.CountProduct) - products.Count) == 0)
                            break;
                    }
                    
                    if ((j == (countPage - 1) || j % 3 == 0))
                    {
                        //var sss = new HashSet<string>(products.Select(x => x.Name));
                        //var prNew = new List<Product>();
                        //foreach (var name in sss)
                        //{
                        //    var pp = products.FirstOrDefault(x => x.Name == name);
                        //    if (pp != null)
                        //        prNew.Add(pp);

                        //}

                        SaveProducts(products, path.Text + @"\products.xml");
                        //if(catNew.Any())
                            SaveCategory(catNew, path.Text + @"\categories.xml");
                            
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
            document.DocumentElement.AppendChild(element); 
            Parallel.ForEach(c, cat =>
            {
                XmlNode e = document.CreateElement("Category"); 

                XmlNode subElement1 = document.CreateElement("Id"); 
                subElement1.InnerText = cat.Id; 
                e.AppendChild(subElement1); 

                XmlNode subElement2 = document.CreateElement("Name"); 
                subElement2.InnerText = cat.Name; 
                e.AppendChild(subElement2);

                XmlNode subElement3 = document.CreateElement("ParentId");
                subElement3.InnerText = cat.ParentId;
                e.AppendChild(subElement3); 

                XmlNode subElement4 = document.CreateElement("ParentName"); 
                subElement4.InnerText = cat.ParentName; 
                e.AppendChild(subElement4); 
                element.AppendChild(e); 
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
            document.DocumentElement.AppendChild(element); 
            
            Parallel.ForEach(p, pr => { 
            
                var e = document.CreateElement("Product"); 

                var subElement1 = document.CreateElement("Id"); 
                subElement1.InnerText = pr.Id; 
                e.AppendChild(subElement1); 

                var subElement2 = document.CreateElement("StoreId"); 
                subElement2.InnerText = pr.StoreId; 
                e.AppendChild(subElement2); 

                var subElement3 = document.CreateElement("StoreName"); 
                subElement3.InnerText = pr.StoreName; 
                e.AppendChild(subElement3); 

                var subElement4 = document.CreateElement("CategoryId"); 
                subElement4.InnerText = pr.CategoryId; 
                e.AppendChild(subElement4); 

                XmlNode subElement5 = document.CreateElement("CategoryName"); 
                subElement5.InnerText = pr.CategoryName; 
                e.AppendChild(subElement5); 

                XmlNode subElement6 = document.CreateElement("Name"); 
                subElement6.InnerText = pr.Name; 
                e.AppendChild(subElement6); 

                XmlNode subElement7 = document.CreateElement("Description"); 
                subElement7.InnerText = pr.Description; 
                e.AppendChild(subElement7); 

                XmlNode subElement8 = document.CreateElement("Price"); 
                subElement8.InnerText = pr.Price; 
                e.AppendChild(subElement8); 

                XmlNode subElement9 = document.CreateElement("Photos"); 
                e.AppendChild(subElement9); 
                if (pr.Photos != null && pr.Photos.Any())
                {
                    foreach (var photo in pr.Photos)
                    {
                        XmlNode subElement10 = document.CreateElement("Photo"); 
                        subElement10.InnerText = photo; 
                        subElement9.AppendChild(subElement10); 
                    }
                }
                element.AppendChild(e); 
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
