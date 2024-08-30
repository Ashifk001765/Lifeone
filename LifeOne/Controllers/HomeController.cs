using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data;
using LifeOne.Models.API.DAL;
using AesEncryption;
using System.Configuration;
using LifeOne.Models.Common;
using LifeOne.Models.AdminManagement.AEntity;
using System.Net.Http;
using LifeOne.Models.FranchiseManagement.FService;
using LifeOne.Models.HomeManagement.HDAL;
using LifeOne.Models.HomeManagement.HEntity;
using LifeOne.Models.HomeManagement.HService;
using LifeOne.Models.Manager;
using LifeOne.Models.AdminManagement.ADAL;
using LifeOne.Models;
using LifeOne.Models.AdminManagement.AService;
using Razorpay.Api;
using System.Net;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.EMMA;
using static LifeOne.Models.ShoppingRequest;

namespace LifeOne.Controllers
{

    public class HomeController : Controller
    {
        string RazorPayLiveKey = ConfigurationManager.AppSettings["RazorKey"].ToString();
        string RazorPayLocalKey = ConfigurationManager.AppSettings["RazorKeyLocal"].ToString();
        string RazorPayLocalSecret = ConfigurationManager.AppSettings["RazorSecretLocaL"].ToString();
        string RazorPayLiveSecret = ConfigurationManager.AppSettings["RazorSecret"].ToString();
        string Aeskey = ConfigurationManager.AppSettings["Aeskey"].ToString();
        string baseurl = ConfigurationManager.AppSettings["baseurl"].ToString();
        string baseurlNoImg = ConfigurationManager.AppSettings["baseurlNoImg"].ToString();
        private readonly object _objService;
        //LifeOne.Models.API.ManageCartService _objservice = new LifeOne.Models.API.ManageCartService();
        //DataTable dt = _objservice.CreateCartProduct("0.80938891", "1", "hi", "2021/01/20");
        // LifeOne.Models.API.Common.SendEmailByAPICommonVerification("amar@qloginuaeretech.com", "Amar" + " " + "yadav", "7800444476", "123456");
        public ActionResult Index(Products products, string Add, string Fk_CategoryId, string ProductName)
        {
            string time = DateTime.Now.ToString("ddMMyyyyhhMMss") + "_" + 4050;
            WebSitePopup model = DALProductServices.WebSitePopup();
            ViewBag.PopupStatus = model.Status;
            ViewBag.ImageUrl = model.ImageUrl;


            if (!string.IsNullOrEmpty(Add))
            {
                products.Quantity = 1;
                products.OpCode = 1;


                DataSet dataSet1 = products.ManageShoppingCart();
                products.DtDetails = dataSet1.Tables[0];
                SessionManager.TotalItems = int.Parse(products.DtDetails.Rows[0]["Quantity"].ToString());

            }
            if (products.Page == null || products.Page == 0)
            {
                products.Page = 1;
            }
            if (Fk_CategoryId != null)
            {
                products.Fk_CategoryId = int.Parse(Crypto.Decryption(Aeskey, Fk_CategoryId));

            }
            List<Products> productsList1 = new List<Products>();
            products.Size = SessionManager.Size;
            products.Pk_ProductId = 0;
            DataSet dataSet = products.GetAllProducts();
            products.DtDetails = dataSet.Tables[0];
            if (products.DtDetails != null)
            {
                for (int i = 0; i < products.DtDetails.Rows.Count; i++)
                {
                    Products listdata1 = new Products();
                    if (!string.IsNullOrEmpty(dataSet.Tables[0].Rows[i]["ProductImage"].ToString()))
                    {
                        listdata1.ProductImage = baseurl + dataSet.Tables[0].Rows[i]["ProductImage"].ToString();
                    }
                    else
                    {
                        listdata1.ProductImage = baseurlNoImg;
                    }
                    listdata1.Pk_ProductId = int.Parse(dataSet.Tables[0].Rows[i]["Pk_ProductId"].ToString());
                    listdata1.ProductName = dataSet.Tables[0].Rows[i]["ProductName"].ToString();
                    listdata1.DP = dataSet.Tables[0].Rows[i]["DP"].ToString();
                    listdata1.OfferedPrice = dataSet.Tables[0].Rows[i]["OfferedPrice"].ToString();
                    listdata1.ReviewCount = dataSet.Tables[0].Rows[i]["ReviewCount"].ToString();
                    listdata1.TotalRecords = int.Parse(dataSet.Tables[0].Rows[i]["TotalRecord"].ToString());
                    productsList1.Add(listdata1);
                }
                DataTable dataTable = DALCommon.ToDataTable(productsList1);
                products.DtDetails = dataTable;
            }
            //products.DtDetailsecond = dataSet.Tables[1];
            //products.ProductType = products.DtDetails.Rows[0]["CategoryName"].ToString();
            List<Products> productsList = new List<Products>();
            dataSet = products.GetAllCategory();
            //products.dtCategory = dataSet.Tables[0];
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                {
                    Products listdata = new Products();
                    if (!string.IsNullOrEmpty(dataSet.Tables[0].Rows[i]["ProductTypeImage"].ToString()))
                    {
                        listdata.ProductTypeImage = baseurl + dataSet.Tables[0].Rows[i]["ProductTypeImage"].ToString();
                    }
                    else
                    {
                        listdata.ProductTypeImage = baseurlNoImg;
                    }
                    listdata.Pk_ProductTypeId = int.Parse(dataSet.Tables[0].Rows[i]["PK_ProductTypeId"].ToString());
                    listdata.ProductType = dataSet.Tables[0].Rows[i]["ProductType"].ToString();
                    listdata.TotalRecords = int.Parse(dataSet.Tables[0].Rows[i]["TotalRecords"].ToString());
                    productsList.Add(listdata);
                }
                DataTable dataTable = DALCommon.ToDataTable(productsList);
                products.dtCategory = dataTable;
            }
            return View(products);
        }

        [HttpPost]
        public ActionResult Index(Products products, string Add)
        {
            if (products.Quantity == null || products.Quantity == 0)
            {
                products.Quantity = 1;
            }
            if (!string.IsNullOrEmpty(Add))
            {
                Session["Pk_ProductId"] = products.Pk_ProductId;

                if (string.IsNullOrEmpty(SessionManager.LoginId))
                {
                    return RedirectToAction("Login", "Home");
                }

                return RedirectToAction("ShoppingCartList", "Home");
            }


            return View(products);
        }
        [HttpPost]
        public JsonResult ProductListDetail(Products products, string Fk_CategoryId)
        {
            if (Fk_CategoryId != null)
            {
                products.Fk_CategoryId = int.Parse(Fk_CategoryId);

            }
            products.Size = SessionManager.Size;
            products.Pk_ProductId = 0;
            products.LstMenu = products.GetAllProductsDetail();
            //products.DtDetails = dataSet.Tables[0];
            return Json(products.LstMenu);
        }

        [ChildActionOnly]
        public ActionResult RenderMenu()
        {
            Products products = new Products();
            if (products.Page == null || products.Page == 0)
            {
                products.Page = 1;
            }
            products.Size = SessionManager.Size;
            products.LstMenu = products.GetAllCategoryDetail();
            return PartialView("_PartialHomeLayout", products);

        }
        [ChildActionOnly]
        public ActionResult ProductCatRenderMenu()
        {
            Products products = new Products();
            if (products.Page == null || products.Page == 0)
            {
                products.Page = 1;
            }
            products.Size = SessionManager.Size;
            products.LstMenu = products.GetAllCategoryDetail();
            return PartialView("_PartialProductMenuLayout", products);

        }

        public ActionResult CartPartialMenu()
        {
            Products products = new Products();
            products.TokenNo = SessionManager.TokenNo;
            products.OpCode = 2;
            products.LstMenu = products.GetAllCartList();
            return PartialView("_Cart", products);

        }
        public ActionResult LandingPage()
        {

            return View();
        }

        public ActionResult CommingSoon1()
        {
            return View();
        }
        public ActionResult Gallery()
        {

            return View();
        }
        public ActionResult About()
        {
            Contact obj = new Contact();
            try
            {
                DataSet ds = obj.getInfgo();
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        obj.Address1 = ds.Tables[0].Rows[i]["CompanyAddress"].ToString();
                        obj.Address2 = ds.Tables[0].Rows[i]["CompanyAddress1"].ToString();
                        obj.Mobile2 = ds.Tables[0].Rows[i]["contactNo1"].ToString();
                        obj.Mobile = ds.Tables[0].Rows[i]["ContactNo"].ToString();
                        obj.Email = ds.Tables[0].Rows[i]["companyEmail"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(obj);
        }
        public ActionResult Opportunity()
        {
            return View();
        }

        public ActionResult Career()
        {
            return View();
        }
        public ActionResult Health_Care()
        {
            return View();
        }
        public ActionResult Personal_Care()
        {
            return View();
        }
        public ActionResult Agriculture()
        {
            return View();
        }
        public ActionResult Affiliate_Shopping()
        {
            return View();
        }
        public ActionResult Contact()
        {
            Contact obj = new Contact();
            try
            {
                DataSet ds = obj.getInfgo();
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        obj.Address1 = ds.Tables[0].Rows[i]["CompanyAddress"].ToString();
                        obj.Address2 = ds.Tables[0].Rows[i]["CompanyAddress1"].ToString();
                        obj.Mobile2 = ds.Tables[0].Rows[i]["contactNo1"].ToString();
                        obj.Mobile = ds.Tables[0].Rows[i]["ContactNo"].ToString();
                        obj.Email = ds.Tables[0].Rows[i]["companyEmail"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View(obj);

        }
        //public ActionResult ContactInsert()
        //{
        //    Contact obj = new Contact();
        //    string Status = "";
        //    string Message = "";
        //    try
        //    {

        //        DataSet ds = obj.getInfgo();
        //        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        //        {
        //            if (ds.Tables[0].Rows[0]["Flag"].ToString() == "1")
        //            {
        //                Status = ds.Tables[0].Rows[0]["Flag"].ToString();
        //                Message = ds.Tables[0].Rows[0]["Message"].ToString();
        //            }
        //            else
        //            {
        //                Status = ds.Tables[0].Rows[0]["Flag"].ToString();
        //                Message = ds.Tables[0].Rows[0]["Message"].ToString();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return Json(new {Status = Status, Message = Message });

        //}
        public ActionResult PrivacyPolicy()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Contact(Contact _model)
        {
            ContactResponse objres = ContactService.Contact(_model);
            //LifeOne.Models.API.Common.SendEmailByAPICommon(_model.Email, "", "", _model.Grievance, _model.Message, "");
            if (objres != null)
            {

                TempData["code"] = objres.Flag.ToString();
                TempData["msg"] = objres.Msg.ToString();
            }

            else
            {
                TempData["code"] = "0";
                TempData["msg"] = "Can not process the request";

            }
            return RedirectToAction("Contact", "Home");
        }
        public ActionResult Service()
        {
            return View();
        }
        public ActionResult BusinessPlan()
        {
            return View();
        }
        public ActionResult Products(Products products, string Add, string id, string ProductId, string ProductName)
        {
            if (products.Page == null || products.Page == 0)
            {
                products.Page = 1;

            }
            products.Size = 12;
            if (!string.IsNullOrEmpty(Add))
            {
                //Session["Pk_ProductId"] = products.Pk_ProductId;

                products.Quantity = 1;
                products.OpCode = 1;
                SessionManager.TokenNo = products.TokenNo;
                products.Pk_ProductId = int.Parse(id);
                DataSet dataSet1 = products.ManageShoppingCart();
                products.DtDetails = dataSet1.Tables[0];
                SessionManager.TotalItems = int.Parse(products.DtDetails.Rows[0]["Quantity"].ToString());

            }
            if (id != null)
            {
                id = id.Replace(" ", "+");
                products.Fk_CategoryId = int.Parse(Crypto.Decryption(Aeskey, id));

            }
            // products.Size = SessionManager.Size;
            List<Products> productsList1 = new List<Products>();
            products.Pk_ProductId = 0;
            DataSet dataSet = products.GetAllProductAccPageSize();
            products.DtDetails = dataSet.Tables[0];
            if (products.DtDetails != null && products.DtDetails.Rows.Count > 0)
            {
                products.ProductType = products.DtDetails.Rows[0]["CategoryName"].ToString();

                for (int i = 0; i < products.DtDetails.Rows.Count; i++)
                {
                    Products listdata1 = new Products();
                    if (!string.IsNullOrEmpty(dataSet.Tables[0].Rows[i]["PrimaryImage"].ToString()))
                    {
                        listdata1.PrimaryImage = baseurl + dataSet.Tables[0].Rows[i]["PrimaryImage"].ToString();
                    }
                    else
                    {
                        listdata1.PrimaryImage = baseurlNoImg;
                    }
                    listdata1.Pk_ProductId = int.Parse(dataSet.Tables[0].Rows[i]["Pk_ProductId"].ToString());
                    listdata1.ProductName = dataSet.Tables[0].Rows[i]["ProductName"].ToString();
                    listdata1.DP = dataSet.Tables[0].Rows[i]["DP"].ToString();
                    listdata1.MRP = dataSet.Tables[0].Rows[i]["MRP"].ToString();
                    listdata1.OfferedPrice = dataSet.Tables[0].Rows[i]["OfferedPrice"].ToString();
                    listdata1.ReviewCount = dataSet.Tables[0].Rows[i]["ReviewCount"].ToString();
                    listdata1.TotalRecords = int.Parse(dataSet.Tables[0].Rows[i]["TotalRecords"].ToString());
                    productsList1.Add(listdata1);
                }
                DataTable dataTable = DALCommon.ToDataTable(productsList1);
                products.DtDetails = dataTable;
            }
            int totalRecords = 0;
            if (dataSet.Tables[0].Rows.Count > 0)
            {
                totalRecords = Convert.ToInt32(products.DtDetails.Rows[0]["TotalRecords"].ToString());
                var pager = new Pager(totalRecords, products.Page, products.Size);
                products.Pager = pager;
            }
            dataSet = products.GetAllCategory();
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                products.dtCategory = dataSet.Tables[0];
            }
            return View(products);
        }
        [HttpPost]
        public ActionResult Products(Products products, string Add, int? CategoryId)
        {
            if (products.Quantity == null || products.Quantity == 0)
            {
                products.Quantity = 1;
            }
            if (products.Page == null || products.Page == 0)
            {
                products.Page = 1;
            }
            products.Size = 12;
            if (!string.IsNullOrEmpty(Add))
            {
                Session["Pk_ProductId"] = products.Pk_ProductId;
                if (string.IsNullOrEmpty(SessionManager.LoginId))
                {
                    return RedirectToAction("Login", "Home");
                }
                return RedirectToAction("ShoppingCartList", "Home");
            }
            products.Fk_CategoryId = int.Parse(CategoryId.ToString());
            DataSet dataSet = products.GetAllProductAccPageSize();
            products.DtDetails = dataSet.Tables[0];
            int totalRecords = 0;
            if (dataSet.Tables[0].Rows.Count > 0)
            {
                totalRecords = Convert.ToInt32(products.DtDetails.Rows[0]["TotalRecords"].ToString());
                var pager = new Pager(totalRecords, products.Page, products.Size);
                products.Pager = pager;
            }
            products.Page = 1;
            products.Size = 12;
            dataSet = products.GetAllCategory();
            products.dtCategory = dataSet.Tables[0];
            //dataSet = products.GetAllCategory();
            //products.dtCategory = dataSet.Tables[0];
            return View(products);
        }

        public ActionResult ProductDetails(string id, string Add, ProductsDetail productDetails, string Fk_CategoryId, string Pk_ProductId, string AddCart)
        {
            Products products = new Products();
            //if (string.IsNullOrEmpty(SessionManager.LoginId))
            //{
            //    return RedirectToAction("Login", "Home");
            //}
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Products");
            }
            if (!string.IsNullOrEmpty(Add))
            {
                products.Quantity = productDetails.Quantity;
                products.OpCode = 1; //AddtoShoppingCart
                products.Pk_ProductId = productDetails.Pk_ProductId;
                DataSet dataSet1 = products.ManageShoppingCart();
                products.DtDetails = dataSet1.Tables[0];
                SessionManager.TotalItems = int.Parse(products.DtDetails.Rows[0]["Quantity"].ToString());
                id = Crypto.Encryption(Aeskey, productDetails.Pk_ProductId.ToString());
            }
            DataTable dtImages = new DataTable();
            dtImages.Columns.Add("Images");
            id = id.Replace(" ", "+");
            products.Pk_ProductId = int.Parse(Crypto.Decryption(Aeskey, id));
            DataSet dataSet = products.GetAllProducts();
            //productDetails.dtTopProducts = dataSet.Tables[0];
            productDetails.CategoryName = dataSet.Tables[0].Rows[0]["CategoryName"].ToString();
            productDetails.ProductName = dataSet.Tables[0].Rows[0]["ProductName"].ToString();
            productDetails.ReviewCount = dataSet.Tables[0].Rows[0]["ReviewCount"].ToString();
            productDetails.MRP = dataSet.Tables[0].Rows[0]["MRP"].ToString();
            productDetails.OfferedPrice = dataSet.Tables[0].Rows[0]["OfferedPrice"].ToString();
            productDetails.ProductImage = baseurl + dataSet.Tables[0].Rows[0]["ProductImage"].ToString();
            productDetails.ProductDescription = dataSet.Tables[0].Rows[0]["ProductDescription"].ToString();
            productDetails.DirectionOfUse = dataSet.Tables[0].Rows[0]["DirectionOfUse"].ToString();
            productDetails.Doses = dataSet.Tables[0].Rows[0]["Doses"].ToString();
            productDetails.Avalibility = dataSet.Tables[0].Rows[0]["Avalibility"].ToString();
            productDetails.BalanceQuantity = dataSet.Tables[0].Rows[0]["BalanceQuantity"].ToString();
            productDetails.Pk_ProductId = int.Parse(dataSet.Tables[0].Rows[0]["Pk_ProductId"].ToString());
            productDetails.DP = decimal.Parse(dataSet.Tables[0].Rows[0]["DP"].ToString());
            //productDetails.AvgRating = dataSet.Tables[0].Rows[0]["AvgRating"].ToString();
            string[] img = dataSet.Tables[0].Rows[0]["ProductImage"].ToString().Split(',');
            for (int i = 0; i < img.Length; i++)
            {
                dtImages.Rows.Add(img[i]);
            }
            productDetails.dtImages = dtImages;
            if (string.IsNullOrEmpty(Fk_CategoryId))
            {
                List<ProductsDetail> productsList1 = new List<ProductsDetail>();
                productDetails.Pk_ProductId = 0;
                productDetails.Fk_CategoryId = dataSet.Tables[0].Rows[0]["CategoryId"].ToString();
                DataSet dataSet3 = productDetails.GetAllProducts();
                productDetails.dtProductDetails = dataSet3.Tables[0];
                for (int i = 0; i < productDetails.dtProductDetails.Rows.Count; i++)
                {
                    ProductsDetail listdata1 = new ProductsDetail();
                    if (!string.IsNullOrEmpty(dataSet3.Tables[0].Rows[i]["ProductImage"].ToString()))
                    {
                        listdata1.ProductImage = baseurl + dataSet3.Tables[0].Rows[i]["ProductImage"].ToString();
                    }
                    else
                    {
                        listdata1.ProductImage = baseurlNoImg;
                    }
                    listdata1.Pk_ProductId = int.Parse(dataSet3.Tables[0].Rows[i]["Pk_ProductId"].ToString());
                    listdata1.ProductName = dataSet3.Tables[0].Rows[i]["ProductName"].ToString();
                    listdata1.DP = decimal.Parse(dataSet3.Tables[0].Rows[i]["DP"].ToString());
                    listdata1.OfferedPrice = dataSet3.Tables[0].Rows[i]["OfferedPrice"].ToString();
                    listdata1.ReviewCount = dataSet3.Tables[0].Rows[i]["ReviewCount"].ToString();
                    productsList1.Add(listdata1);
                }
                DataTable dataTable = DALCommon.ToDataTable(productsList1);
                productDetails.dtProductDetails = dataTable;
                //var v = int.Parse(dataSet.Tables[0].Rows[0]["Pk_ProductId"].ToString());
                //int.Parse(dataSet.Tables[0].Rows[0]["Pk_ProductId"].ToString().Remove(v));
                productDetails.Pk_ProductId = int.Parse(dataSet.Tables[0].Rows[0]["Pk_ProductId"].ToString());
            }
            //if (string.IsNullOrEmpty(AddCart))
            //{
            //    products.Quantity = 1;
            //    products.OpCode = 1;
            //    DataSet dataSet1 = products.ManageShoppingCart();
            //    products.DtDetails = dataSet1.Tables[0];
            //    SessionManager.TotalItems = int.Parse(products.DtDetails.Rows[0]["Quantity"].ToString());

            //}
            productDetails.Pk_ProductId = int.Parse(Crypto.Decryption(Aeskey, id));
            productDetails.OpCode = 2;
            DataSet dataSet2 = productDetails.GetProductReview();
            productDetails.dtDetails = dataSet2.Tables[0];
            if (productDetails.dtDetails.Rows.Count > 0)
            {
                productDetails.AvgRating = productDetails.dtDetails.Rows[0]["AvgRating"].ToString();
                productDetails.ReviewCount = productDetails.dtDetails.Rows[0]["ReviewCount"].ToString();
            }


            return View(productDetails);
        }
        [HttpPost]
        public JsonResult ProductDetailsListView(ProductsDetail productsDetails, string Id)
        {
            Products products = new Products();
            string result = "";
            //DataTable dtImages = new DataTable();
            //dtImages.Columns.Add("Images");
            productsDetails.FkMemId = SessionManager.AssociateFk_MemId.ToString();
            products.Pk_ProductId = int.Parse(Id);
            products.objList = products.GetAllProductsDetail();

            //Result = int.Parse(model.DtDetails.Rows[0]["Quantity"].ToString());
            //productsDetails.ProductName= dataSet.Tables[0].Rows[0]["ProductName"].ToString();
            // productsDetails.ProductDescription= dataSet.Tables[0].Rows[0]["ProductDescription"].ToString();

            return Json(products.objList, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ProductDetails(ProductsDetail productsDetail, string Add, string Update, string AddCart)
        {
            Products products = new Products();
            if (!string.IsNullOrEmpty(Add))
            {
                Session["Pk_ProductId"] = productsDetail.Pk_ProductId;
                if (string.IsNullOrEmpty(SessionManager.LoginId))
                {
                    return RedirectToAction("Login", "Home");
                }
                // Session["Quantity"] = productsDetail.Quantity;
                return RedirectToAction("ShoppingCartList", "Home");

            }

            if (!string.IsNullOrEmpty(AddCart))
            {
                Session["Pk_ProductId"] = productsDetail.Pk_ProductId;

                if (string.IsNullOrEmpty(SessionManager.LoginId))
                {
                    return RedirectToAction("Login", "Home");
                }

                return RedirectToAction("ShoppingCartList", "Home");
            }
            Session["Pk_ProductId"] = productsDetail.Pk_ProductId;
            productsDetail.OpCode = 2;
            DataSet dataSet2 = productsDetail.GetProductReview();
            productsDetail.dtDetails = dataSet2.Tables[0];
            if (productsDetail.dtDetails.Rows.Count > 0)
            {
                productsDetail.AvgRating = productsDetail.dtDetails.Rows[0]["AvgRating"].ToString();
                productsDetail.ReviewCount = productsDetail.dtDetails.Rows[0]["ReviewCount"].ToString();
            }

            return View(productsDetail);
        }
        [HttpPost]
        public JsonResult ProductRating(ProductsDetail productsDetail, string Rating, string Review, string btnReview, string ProductId)
        {
            productsDetail.FkMemId = SessionManager.AssociateFk_MemId.ToString();
            productsDetail.Pk_ProductId = int.Parse(ProductId);
            //productsDetail.Pk_ProductId = int.Parse(Crypto.Decryption(Aeskey, id));
            if (!string.IsNullOrEmpty(btnReview))
            {
                productsDetail.OpCode = 1;
                productsDetail.Rating = Rating;

                productsDetail.Review = Review;
            }
            else
            {
                productsDetail.OpCode = 2;
            }
            //productsDetail.OpCode = 1;
            //productsDetail.Rating = int.Parse(Rating);
            //productsDetail.Review = Review;
            DataSet dataSet2 = productsDetail.GetProductReview();
            productsDetail.Message = dataSet2.Tables[0].Rows[0]["Msg"].ToString();
            productsDetail.Status = dataSet2.Tables[0].Rows[0]["Flag"].ToString();
            return Json(productsDetail);
        }
        [HttpPost]
        //public JsonResult CheckLogin(string Pk_ProductId, string Quantity)
        //{
        //    //if (string.IsNullOrEmpty(SessionManager.LoginId))
        //    //{
        //    //    return RedirectToAction("Login", "Home");
        //    //}
        //    string Status = Crypto.Encryption("AddToCart", "Home");

        //    string URl = HttpContext.Session.GetString("Redirect");

        //    //string URl = HttpContext.Session.GetString("Redirect");

        //    return Json(Status);
        //}        
        public ActionResult AddProductCart(Products products, string id, string Update, string Checkout, string Status)
        {
            DataSet dataSet = new DataSet();
            products.FK_MemId = int.Parse(SessionManager.AssociateFk_MemId.ToString());
            products.Status = Session["Status"].ToString();
            if (string.IsNullOrEmpty(SessionManager.LoginId))
            {
                return RedirectToAction("Login", "Home");
            }

            if (!string.IsNullOrEmpty(products.Status))
            {
                products.Pk_ProductId = int.Parse(Session["Pk_ProductId"].ToString());
                products.Quantity = int.Parse(Session["Quantity"].ToString());

                products.OpCode = 1;
                products.TokenNo = SessionManager.TokenNo;
                dataSet = products.ManageShoppingCart();
                products.DtDetails = dataSet.Tables[0];
            }
            if (!string.IsNullOrEmpty(Checkout))
            {


                Session["Checkout"] = "Checkout";

                return RedirectToAction("GetAddress");
            }
            products.TokenNo = SessionManager.TokenNo;
            products.OpCode = 2;
            dataSet = products.ManageShoppingCart();
            products.DtDetails = dataSet.Tables[0];
            if (products.DtDetails.Rows.Count > 0)
            {
                SessionManager.TotalItems = Convert.ToInt32(products.DtDetails.Compute("SUM(TotalItems)", string.Empty));
                Session["ShoppingPrice"] = products.DtDetails.Compute("SUM(SubTotal)", string.Empty);
                SessionManager.ProductCount = Convert.ToInt32(products.DtDetails.Compute("SUM(ProductCount)", string.Empty));
            }
            else
            {
                SessionManager.TotalItems = 0;
                SessionManager.ProductCount = 0;
            }
            //return View(products);
            return Redirect("ShoppingCartList");
        }
        public ActionResult ShoppingCartList(Products products, string id, string Update, string Checkout, string Status)
        {
            Update = products.Update;
            Session["Pk_ProductId"] = products.id;
            DataSet dataSet = new DataSet();
            products.FK_MemId = int.Parse(SessionManager.AssociateFk_MemId.ToString());
            //products.Status = Session["Status"].ToString();            
            if (string.IsNullOrEmpty(SessionManager.LoginId))
            {
                return RedirectToAction("Login", "Home");
            }

            //if (!string.IsNullOrEmpty(products.Status))
            //{
            //    products.Pk_ProductId = int.Parse(Session["Pk_ProductId"].ToString());
            //    products.Quantity = int.Parse(Session["Quantity"].ToString());

            //    products.OpCode = 1;
            //    products.TokenNo = SessionManager.TokenNo;
            //    dataSet = products.ManageShoppingCart();
            //    products.DtDetails = dataSet.Tables[0];
            //}

            if (!string.IsNullOrEmpty(Update))
            {
                products.Quantity = products.Qty;
                products.OpCode = 4;
                products.TokenNo = SessionManager.TokenNo;
                dataSet = products.ManageShoppingCart();
                products.DtDetails = dataSet.Tables[0];
            }
            if (!string.IsNullOrEmpty(Checkout))
            {


                Session["Checkout"] = "Checkout";

                return RedirectToAction("GetAddress");
            }
            List<Products> productsList1 = new List<Products>();
            products.TokenNo = SessionManager.TokenNo;
            products.OpCode = 2;
            dataSet = products.ManageShoppingCart();
            products.DtDetails = dataSet.Tables[0];
            if (products.DtDetails.Rows.Count > 0)
            {
                SessionManager.TotalItems = Convert.ToInt32(products.DtDetails.Compute("SUM(TotalItems)", string.Empty));
                Session["ShoppingPrice"] = products.DtDetails.Compute("SUM(SubTotal)", string.Empty);
                SessionManager.ProductCount = Convert.ToInt32(products.DtDetails.Compute("SUM(ProductCount)", string.Empty));
                if (products.DtDetails != null)
                {
                    for (int i = 0; i < products.DtDetails.Rows.Count; i++)
                    {
                        Products listdata1 = new Products();
                        if (!string.IsNullOrEmpty(dataSet.Tables[0].Rows[i]["ProductImage"].ToString()))
                        {
                            listdata1.ProductImage = baseurl + dataSet.Tables[0].Rows[i]["ProductImage"].ToString();
                        }
                        else
                        {
                            listdata1.ProductImage = baseurlNoImg;
                        }
                        listdata1.Fk_ProductId = int.Parse(dataSet.Tables[0].Rows[i]["Fk_ProductId"].ToString());
                        listdata1.ProductName = dataSet.Tables[0].Rows[i]["ProductName"].ToString();
                        listdata1.SubTotal = dataSet.Tables[0].Rows[i]["SubTotal"].ToString();
                        listdata1.SalesPrice = dataSet.Tables[0].Rows[i]["SalesPrice"].ToString();
                        listdata1.Quantity = int.Parse(dataSet.Tables[0].Rows[i]["Quantity"].ToString());
                        listdata1.Pk_Id = int.Parse(dataSet.Tables[0].Rows[i]["Pk_Id"].ToString());
                        productsList1.Add(listdata1);
                    }
                    DataTable dataTable = DALCommon.ToDataTable(productsList1);
                    products.DtDetails = dataTable;
                    //for (int i = 0; i < products.DtDetails.Rows.Count; i++)
                    //{
                    //    products.ProductImage = baseurl + dataSet.Tables[0].Rows[i]["ProductImage"].ToString();
                    //}
                }
            }
            else
            {
                SessionManager.TotalItems = 0;
                SessionManager.ProductCount = 0;
            }
            Session["Status"] = null;
            return View(products);
        }
        public ActionResult DeleteShoppingCart(string id)
        {
            try
            {
                Products products = new Products();

                if (!string.IsNullOrEmpty(id))
                {
                    products.OpCode = 3;
                    products.TokenNo = SessionManager.TokenNo;
                    products.Pk_Id = int.Parse(id);
                    DataSet dataSet = products.ManageShoppingCart();
                    products.DtDetails = dataSet.Tables[0];
                }
                return RedirectToAction("ShoppingCartList", "Home");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public ActionResult GetAddress()
        {
            Reports report = new Reports();
            report.FK_MemId = int.Parse(SessionManager.AssociateFk_MemId.ToString());
            DataSet dataSet = report.GetAssociateAddress();
            report.dtaddressdetails = dataSet.Tables[0];
            DataSet dataset1 = report.GetWalletAmount();
            report.WalletAmount = decimal.Parse(dataset1.Tables[0].Rows[0]["WalletAmount"].ToString());
            report.TotalAmount = decimal.Parse(Session["ShoppingPrice"].ToString());
            if (report.TotalAmount > report.WalletAmount)
            {
                report.GatewayAmount = 0;
                //ViewBag.InsufficientBalance = "InsufficientBalance";
            }
            else
            {
                report.WalletAmount = report.TotalAmount;
                report.GatewayAmount = 0;
                //ViewBag.InsufficientBalance = "Valid";
            }

            return View(report);
        }
        public ActionResult HomeAddressDelete(string id)
        {
            try
            {
                Reports report = new Reports();
                if (!string.IsNullOrEmpty(id))
                {
                    report.OpCode = 3;
                    report.Pk_AddressId = id;
                    report.FK_MemId = int.Parse(SessionManager.AssociateFk_MemId.ToString());
                    DataSet ds = report.AssociateAddressDelete();

                }
            }
            catch (Exception)
            {
                throw;
            }

            return Redirect("/Home/GetAddress");
        }

        public ActionResult HomeAddress(string id)
        {

            Reports report = new Reports();
            if (id != null)
            {
                report.Pk_AddressId = id;
                try
                {
                    report.FK_MemId = int.Parse(SessionManager.AssociateFk_MemId.ToString());
                    DataSet dataSet = report.GetAssociateAddress();
                    report.dtaddressdetails = dataSet.Tables[0];
                    report.Name = dataSet.Tables[0].Rows[0]["Name"].ToString();
                    report.MobileNo = dataSet.Tables[0].Rows[0]["MobileNo"].ToString();
                    report.Landmark = dataSet.Tables[0].Rows[0]["Landmark"].ToString();
                    report.Address = dataSet.Tables[0].Rows[0]["Address"].ToString();
                    report.State = dataSet.Tables[0].Rows[0]["State"].ToString();
                    report.City = dataSet.Tables[0].Rows[0]["City"].ToString();
                    report.PinCode = dataSet.Tables[0].Rows[0]["PinCode"].ToString();
                    report.AddressType = dataSet.Tables[0].Rows[0]["AddressType"].ToString();
                    report.IsDefault = bool.Parse(dataSet.Tables[0].Rows[0]["IsDefault"].ToString());
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return View(report);
        }

        [HttpPost]
        public ActionResult HomeAddress(Reports report, string Save)
        {
            try
            {
                DataSet _result = new DataSet();
                report.FK_MemId = int.Parse(SessionManager.AssociateFk_MemId.ToString());
                if (Save == "Save")
                {
                    _result = report.AssociateAddAddress(report);
                }
                else
                {
                    _result = report.AssociateAddressEdit(report);
                }
                if (_result != null)
                {
                    return RedirectToAction("GetAddress", "Home");
                }

                else
                {
                    TempData["code"] = "0";
                    TempData["msg"] = "Can not process the request";

                }

            }
            catch (Exception)
            {
                throw;
            }

            return View();
        }



        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetLocationByPin(int? pin)
        {
            CustomerDb objcusdb = new CustomerDb();
            LifeOne.Models.API.MPincode modelRequest = new LifeOne.Models.API.MPincode();
            modelRequest.PinCode = pin.ToString();
            var res = objcusdb.GetPincodeDetais(modelRequest);
            var data1 = res.Distinct().GroupBy(y => new { y.DistrictName, y.StateName }).Select(y => new
            {
                State = y.FirstOrDefault().StateName,
                District = y.FirstOrDefault().DistrictName,
                DistrictDetail = y.ToList().Distinct().GroupBy(x => x.DistrictName).Select(x => new
                {
                    Taluka = x.FirstOrDefault().Taluka,
                    Circle = x.FirstOrDefault().CircleName,
                    Zone = x.FirstOrDefault().DivisionName
                    //Region = x.FirstOrDefault().Region
                }).ToList()
            }).Distinct().ToList();
            return Json(data1, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult CustomerRegistration()
        {
            //if (Fk_MemId != 0)
            //{
            //    CustomerRegService _objService = new CustomerRegService();
            //    MCustomerRegistration objres = new MCustomerRegistration();
            //    objres = CustomerRegService.InvokeCustomerDetailsById(Fk_MemId);
            //    return View(objres);
            //}


            MCustomerRegistration _obj = new MCustomerRegistration();
            return View(_obj);
        }

        [HttpPost]
        public JsonResult CustomerPrimaryDetails(LifeOne.Models.API.Registration Obj)
        {
            try
            {
                Obj.Normalpassword = Obj.Password;
                CustomerRegService _objService = new CustomerRegService();
                string _normalPassword = Obj.Password;
                if (Obj.Password != null)
                {
                    Obj.Normalpassword = Obj.Password;
                    Obj.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(Obj.Password, "md5");
                }
                else
                {
                    Obj.Normalpassword = "123456";
                    Obj.Password = "123456";
                    Obj.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(Obj.Password, "md5");
                }
                CustomerDb objcusdb = new CustomerDb();
                var res = objcusdb.WebRegistrationByReferral(Obj);
                Session["AssociateLoginId"] = res.LoginId;
                Session["AssociatePassword"] = res.Password;
                Session["AssociateName"] = res.Name;
                //var res = CustomerRegService.InvokeCustomerPrimaryDetails(Obj);
                if (res.Flag == 1)
                {
                    //string message = LifeOne.Models.API.SMS.PlotHolderRegistrationSMS(Obj.FirstName, Obj.MobileNo, _normalPassword);
                    //LifeOne.Models.API.SMS.SendSMSWitmtemp(Obj.MobileNo, message, ConfigurationManager.AppSettings["TEMP-SMS-PLOTHOLDER-REGISTRATION"].ToString());   //open it on the development server   
                    // LifeOne.Models.API.Common.SendEmailByAPICommonVerification(Obj.EmailId, Obj.Name, Obj.MobileNo, Obj.NormalPassword);


                    LifeOne.Models.API.MCareRegistration objPost = new LifeOne.Models.API.MCareRegistration();
                    objPost.SponsorCode = Obj.InvitedBy;
                    objPost.Address = Obj.Address;
                    objPost.City = Obj.City;
                    objPost.DOB = Obj.DOB;
                    objPost.Iskyc = "false";
                    objPost.EmailId = Obj.Email;
                    objPost.FirstName = Obj.FirstName;
                    objPost.Gender = Obj.Gender;
                    objPost.LastName = Obj.LastName;
                    objPost.ParentCode = Obj.PlaceUnderId;
                    objPost.Leg = Obj.Leg;
                    objPost.MobileNo = Obj.MobileNo;
                    objPost.PinCode = Obj.Pincode;
                    objPost.Tehsil = Obj.City;
                    objPost.LoginId = res.LoginId;
                    objPost.Password = Obj.Normalpassword;
                    objPost.FatherName = Obj.FatherName;
                    objPost.IsLifeoneWellnessRegistration = 1;
                    string _body = JsonConvert.SerializeObject(objPost);
                    // sendSCareRegistration("http://sgsquare.in/api/LifeoneWellnessRegistrationAPI/Registration", "POST", _body);


                }
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public JsonResult SendOTP(LifeOne.Models.API.Registration Obj)
        {
            int _min = 0000;
            int _max = 9999;
            Random _rdm = new Random();
            int pass = _rdm.Next(_min, _max);

            string Message2 = Models.API.SMS.OTPMemberMessage2(Obj.FirstName, pass.ToString(), "");
            string mobile = Obj.MobileNo;
            Models.API.SMS.SendSMSTempOTP(mobile, Message2, ConfigurationManager.AppSettings["TEMP-SMS-OTP-Member"].ToString());
            Obj.OTP = pass.ToString();
            Obj.Flag = "1";
            return Json(Obj, JsonRequestBehavior.AllowGet);

        }


        public string sendSCareRegistration(string url, string Method, string body)
        {
            string responseText = "0";
            try
            {

                LifeOne.Models.API.MCareRegistration requestModel = new LifeOne.Models.API.MCareRegistration();
                string _modaldata = LifeOne.Models.BillAvenueUtility.Entity.ApiEncrypt_Decrypt.EncryptString("LifeWellAP1234@Z", body);
                LifeOne.Models.BillAvenueUtility.Entity.RequestModel requestModel1 = new LifeOne.Models.BillAvenueUtility.Entity.RequestModel();
                requestModel1.body = _modaldata;

                var client = new HttpClient();
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Add("contentType", "application/json");
                //HTTP POST
                var responseTask = client.PostAsJsonAsync(url, requestModel1);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    string response = result.Content.ReadAsStringAsync().Result;
                    var Apiresponse = result.Content.ReadAsStringAsync().Result;
                }
                return responseText;
            }
            catch (Exception e)
            {
                return responseText;
            }

        }



        [HttpPost]
        public JsonResult CustomerAddressDetails(MCustomerRegistration Obj)
        {
            CustomerRegService _objService = new CustomerRegService();
            var res = CustomerRegService.InvokeCustomerAddressDetails(Obj);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CustomerFamilyDetails(string SpouseName, string SpouseAadharNo, string SpousePanNo, string SpouseAadharImages, string Fk_MemId, List<ChildDetails> data)
        {
            try
            {
                CustomerRegService _objService = new CustomerRegService();
                MCustomerRegistration Obj = new MCustomerRegistration();
                Obj.SpouseName = SpouseName;
                Obj.SpouseAadharNo = SpouseAadharNo;
                Obj.SpousePanNo = SpousePanNo;
                Obj.SpouseAadharImages = SpouseAadharImages;
                Obj.Fk_MemId = Convert.ToInt32(Fk_MemId);
                var xmlFamily = "<Result>";
                for (var i = 0; i < data.Count; i++)
                {
                    xmlFamily += "<data><ChildName>" + data[i].ChildName + "</ChildName><Relation>" + data[i].Relation + "</Relation><ChildAadhar>" + data[i].ChildAadhar + "</ChildAadhar><ChildAadharImage>" + data[i].ChildAadharImage + "</ChildAadharImage><ChildPanNo>" + data[i].ChildPanNo + "</ChildPanNo></data>";
                }
                xmlFamily += "</Result>";
                var res = CustomerRegService.InvokeCustomerFamilyDetails(Obj, xmlFamily);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public JsonResult CustomerFarmDetails(MCustomerRegistration Obj)
        {
            try
            {
                CustomerRegService _objService = new CustomerRegService();
                var res = CustomerRegService.InvokeCustomerFarmDetails(Obj);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public JsonResult GetReferDetails(string Fk_SponserId)
        {
            CustomerRegService _objService = new CustomerRegService();
            var res = CustomerRegService.InvokeGetSponserDetails(Fk_SponserId);
            return Json(res);
        }


        [HttpPost]
        public JsonResult GetReferDetailsWithPlaceHolderId(string Fk_SponserId)
        {
            CustomerRegService _objService = new CustomerRegService();
            var res = CustomerRegService.InvokePlaceHolderId(Fk_SponserId);
            return Json(res);
        }
        [HttpPost]
        public JsonResult GetPalceUnderId(string Fk_SponserId, string Fk_PlaceId)
        {
            CustomerRegService _objService = new CustomerRegService();
            var res = CustomerRegService.InvokeGetPlaceholderWithPArentid(Fk_SponserId, Fk_PlaceId);
            return Json(res);
        }


        [HttpPost]
        public JsonResult GetStateCityTehsilByPincode(string Pincode)
        {
            CustomerRegService _objService = new CustomerRegService();
            var res = CustomerRegService.InvokeGetDistrictDetails(Pincode);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Uploadfile(HttpPostedFileBase file, string subdir)
        {
            try
            {
                var objres = (object)null;
                if (file != null && file.ContentLength > 0)
                {
                    var ImagePath = "~/ReadWriteData/" + subdir + "/";
                    var fileName = Path.GetFileName(file.FileName);
                    var extension = Path.GetExtension(file.FileName);
                    if (extension.ToUpper() == ".PNG" || extension.ToUpper() == ".JPG" || extension.ToUpper() == ".JPEG" || extension.ToUpper() == ".BMP" || extension.ToUpper() == ".PDF" || extension.ToUpper() == ".DOCX" || extension.ToUpper() == ".XLSX")
                    {
                        if (!Directory.Exists(Server.MapPath(ImagePath)))
                        {
                            Directory.CreateDirectory((Server.MapPath(ImagePath)));
                        }
                        //Guid.NewGuid().ToString().Replace("_", "") + "_" + InputFileName
                        fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + fileName;


                        file.SaveAs(Path.Combine(Server.MapPath(ImagePath), fileName));
                        objres = new { Msg = "file upload successfully!", Path = ImagePath + "" + fileName, result = "success" };
                    }
                    else
                    {
                        objres = new { Msg = "Only jpg,jpeg,png,bmp,pdf,docx,xlsx files are allow !", result = "warning" };
                    }
                }
                else
                {
                    objres = new { Msg = "Please select file to upload!", result = "warning" };
                }
                return Json(objres, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        public ActionResult Login()
        {
            MLogin model = new MLogin();

            //if (!string.IsNullOrEmpty(Status))
            //{
            //    if (Status == "AddToCart")
            //    {
            //        int FK_MemId = int.Parse(SessionManager.AssociateFk_MemId.ToString()) == null ? 0 :
            //           int.Parse(SessionManager.AssociateFk_MemId.ToString());
            //        if (FK_MemId == 0)
            //        {
            //            Session["Pk_ProductId"] = Pk_ProductId;
            //            Session["Quantity"] = Quantity;
            //            Session["Status"] = Status;
            //        }
            //    }
            //}
            return View();
        }
        [HttpPost]

        public ActionResult Login(MLogin _model)
        {
            //int FK_MemId = int.Parse(SessionManager.AssociateFk_MemId.ToString()) == null ? 0 :
            //       int.Parse(SessionManager.AssociateFk_MemId.ToString());
            //if (FK_MemId == null)
            //{
            //    _model.ProductId = Session["Pk_ProductId"].ToString();
            //    _model.Quantity = Session["Quantity"].ToString();
            //    _model.Status = Session["Status"].ToString();
            //}

            DALLogin _obj = new DALLogin();
            LoginResponse _objres = new LoginResponse();
            if (ModelState.IsValid)
            {
                string _P = _model.Password;
                _model.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(_model.Password, "md5");
                _objres = _obj.CheckLoginUsers(_model);
                // SessionManager.Fk_UserTypeId = Convert.ToInt32(_objres.UserType);
                TempData["Msg"] = _objres.response;
                if (_objres.UserType == "1")  //Associate Login
                {
                    if (_objres.IsMultiAccount)
                    {
                        Session["_P"] = _P;
                        Session["MobileNo"] = _objres.Mobile;
                        return Redirect("/home/UserList");
                    }
                    if (!_objres.IsPasswordChange)
                    {
                        CustomerDb _CustomerService = new CustomerDb();
                        LifeOne.Models.API.OTP _Otp = new LifeOne.Models.API.OTP();
                        _Otp.Mobile = _objres.Mobile;
                        _Otp.Fk_memId = _objres.Fk_MemId;
                        _Otp.Purpose = "Set New Password";
                        _Otp.ProcId = 4;

                        LifeOne.Models.API.DetailsForOtp _Response = _CustomerService.GenerateOTP(_Otp);
                        ViewBag.ResponseMsg = _Response.Msg;
                        string Message2 = LifeOne.Models.API.SMS.OTPMemberMessage2(_objres.Name, _Response.OTP, _Otp.Purpose);
                        string mobile = _objres.Mobile;
                        LifeOne.Models.API.SMS.SendSMSTempOTP(mobile, Message2, ConfigurationManager.AppSettings["TEMP-SMS-OTP-Member"].ToString());
                        System.Web.HttpContext.Current.Session["LoginUserId"] = _model.Login;
                        System.Web.HttpContext.Current.Session["MobileNo"] = _objres.Mobile;
                        return RedirectToAction("SetPassword", "Home");
                    }

                    SessionManager.AssociateFk_MemId = Convert.ToInt64(_objres.Fk_MemId);
                    SessionManager.Name = _objres.Name;
                    SessionManager.Usertype = _objres.UserType;
                    SessionManager.LoginId = _objres.LoginId;
                    SessionManager.SponsorFk_MemId = _objres.SponsorFk_MemId;
                    SessionManager.ProfilePic = _objres.ProfilePic;
                    SessionManager.RecognitionPic = _objres.RecognitionPic;
                    SessionManager.PerformanceLevel = _objres.PerformanceLevel;
                    SessionManager.PerformanceLevelId = _objres.FK_ProductID;
                    SessionManager.IsBusinessId = _objres.IsBusinessId;
                    SessionManager.TopupLevel = _objres.TopUpLevel;
                    SessionManager.AssociateFk_MemId = Convert.ToInt64(_objres.Fk_MemId);
                    SessionManager.Recognition = _objres.Recognition;
                    SessionManager.TotalItems = int.Parse(_objres.TotalItems.ToString());

                    if (Session["Pk_ProductId"] != null)
                    {
                        // return RedirectToAction("AddToCart");
                        //return RedirectToAction("AddProductCart");
                        return RedirectToAction("ShoppingCartList");

                    }
                    return RedirectToAction("../Associate-Dashboard");

                }
                //else if (_objres.UserType == "3" || _objres.UserType == "5") //Admin Login
                //{
                //    SessionManager.Name = _objres.Name;
                //    SessionManager.Usertype = _objres.UserType;
                //    SessionManager.LoginId = _objres.LoginId;
                //    SessionManager.SponsorFk_MemId = _objres.SponsorFk_MemId;
                //    SessionManager.ProfilePic = _objres.ProfilePic;
                //    SessionManager.PerformanceLevel = _objres.PerformanceLevel;
                //    SessionManager.IsBusinessId = _objres.IsBusinessId;
                //    //  SessionManager.Fk_UserTypeId = Convert.ToInt32(_objres.UserType);
                //    TempData["Msg"] = _objres.response;

                //    SessionManager.Fk_MemId = Convert.ToInt64(_objres.Fk_MemId);
                //    //SessionManager.UserPermissionDt = _obj.GetFormPermissionDetails(Convert.ToInt32(SessionManager.Fk_MemId));
                //    //SessionManager._UserPermissionList = _obj.GetForm_Permission(SessionManager.Fk_MemId);
                //    return RedirectToAction("../Admin-Dashboard");
                //}
                else if (_objres.UserType == "5")
                {
                    SessionManager.Fk_MemId = Convert.ToInt64(_objres.Fk_MemId);
                    MForgotPassword mPin = new MForgotPassword();
                    ViewBag.UserType = _objres.UserType;
                    TempData["LoginId"] = _model.Login;
                    //GetOTPForEmployee(_objres.Mobile);
                    return RedirectToAction("MatchOTPEmployee", "Home");
                }
                else if (_objres.UserType == "2") //Franchise Login
                {
                    SessionManager.Name = _objres.Name;
                    SessionManager.Usertype = _objres.UserType;
                    SessionManager.LoginId = _objres.LoginId;
                    SessionManager.SponsorFk_MemId = _objres.SponsorFk_MemId;
                    SessionManager.ProfilePic = _objres.ProfilePic;
                    SessionManager.PerformanceLevel = _objres.PerformanceLevel;
                    SessionManager.IsBusinessId = _objres.IsBusinessId;
                    // SessionManager.Fk_UserTypeId = Convert.ToInt32(_objres.UserType);
                    TempData["Msg"] = _objres.response;

                    SessionManager.FranchiseFk_MemId = Convert.ToInt64(_objres.Fk_MemId);
                    return RedirectToAction("../Franchise-Dashboard");

                }
                //else if (_objres.UserType == "4") //Franchise Child Login
                //{
                //    SessionManager.Name = _objres.Name;
                //    SessionManager.Usertype = _objres.UserType;
                //    SessionManager.LoginId = _objres.LoginId;
                //    SessionManager.SponsorFk_MemId = _objres.SponsorFk_MemId;
                //    SessionManager.ProfilePic = _objres.ProfilePic;
                //    SessionManager.PerformanceLevel = _objres.PerformanceLevel;
                //    SessionManager.IsBusinessId = _objres.IsBusinessId;
                //    // SessionManager.Fk_UserTypeId = Convert.ToInt32(_objres.UserType);
                //    TempData["Msg"] = _objres.response;
                //    return RedirectToAction("Franchise/FranchiseChildDashboard", "Franchise");
                //}
                else
                {
                    TempData["Msg"] = "Invalid LoginId Or Password";
                }

            }
            else
            {
                TempData["Msg"] = "All Field Required";
            }
            return View();
        }
        [HttpGet]
        public ActionResult MemberLogin()
        {
            MLogin model = new MLogin();
            ViewBag.LoginId = null;
            return View(model);
        }

        public ActionResult UserList(string Login)
        {
            string Mobile = !string.IsNullOrEmpty(Login) ? Login : (string)Session["MobileNo"];
            //  ViewBag._Mobile = Mobile.Substring(0, 2) + "******" + Mobile.Substring(7, 2);
            DALLogin _obj = new DALLogin();
            MultiUserIdList _response = new MultiUserIdList();
            _response = _obj.GetUserListByMobileNo(Mobile);
            if (_response == null || _response._UserList.Count() < 1)
            {

                //TempData["Msg"] = "Invalid login id!";
                //updated on 06022021
                return View("CustomerRegistration");
                ////return View("_RegCommingSoon");
            }
            else if (_response != null && _response._UserList.Count() == 1)
            {
                ViewBag.LoginId = Login;
                MLogin obj = new MLogin();
                obj.Login = Login;
                return View("Login", obj);
            }
            else if (_response != null && _response._UserList.Count() > 1)
            {
                _response._UserList.ForEach(x => x._LoginId = Crypto.Encryption(Aeskey, x.LoginId));
                Session["MultiUserList"] = _response;
            }

            return View(_response);

        }


        public ActionResult _SetNewPassword(int? i)
        {
            int I = i ?? 0;
            MultiUserIdList UserList = (MultiUserIdList)Session["MultiUserList"];
            CustomerDb _CustomerService = new CustomerDb();
            LifeOne.Models.API.OTP _Otp = new LifeOne.Models.API.OTP();
            _Otp.Mobile = UserList._UserList[I].Mobile;
            _Otp.Fk_memId = UserList._UserList[I].FK_MemId;
            _Otp.Purpose = "Set New Password";
            _Otp.ProcId = 4;
            LifeOne.Models.API.DetailsForOtp _Response = _CustomerService.GenerateOTP(_Otp);
            ViewBag.ResponseMsg = _Response.Msg;
            string Message2 = LifeOne.Models.API.SMS.OTPMemberMessage2(UserList._UserList[I].Name, _Response.OTP, _Otp.Purpose);
            string mobile = UserList._UserList[I].Mobile;
            LifeOne.Models.API.SMS.SendSMSTempOTP(mobile, Message2, ConfigurationManager.AppSettings["TEMP-SMS-OTP-Member"].ToString());
            Session["LoginUserId"] = UserList._UserList[I].LoginId;
            Session["MobileNo"] = UserList._UserList[I].Mobile;
            return RedirectToAction("SetPassword", "Home");
        }
        public ActionResult _Login(string _)
        {
            _ = _.Replace(" ", "+");
            _ = Crypto.Decryption(Aeskey, _);
            MLogin obj = new MLogin();
            obj.Login = _;
            return View("Login", obj);
        }

        public ActionResult MatchOTPEmployee(MLogin obj)
        {
            try
            {
                if (TempData["LoginId"].ToString() != null)
                {
                    obj.Login = TempData["LoginId"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View(obj);
        }

        public ActionResult CustomErrorPage()
        {
            return View();
        }
        public ActionResult ForgotPassword(string _)
        {
            _ = _.Replace(" ", "+");
            MForgotPassword _obj = new MForgotPassword();
            _obj.UserId = Crypto.Decryption(Aeskey, _);
            return View(_obj);
        }

        public JsonResult GetOTP(string UserId)
        {
            MForgotPassword mPin = new MForgotPassword();
            mPin.UserId = UserId;
            mPin = ForgotPasswordService.SendOTP(mPin);
            if (mPin.Flag == 1)
            {
                string Message2 = LifeOne.Models.API.SMS.OTPMemberMessage2(mPin.MemberName, mPin.OTP, "OTP");
                string mobile = mPin.MobileNo;
                LifeOne.Models.API.SMS.SendSMSTempOTP(mobile, Message2, ConfigurationManager.AppSettings["TEMP-SMS-OTP-Member"].ToString());
            }
            return Json(mPin, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetOTPForEmployee(string UserId)
        {
            MForgotPassword mPin = new MForgotPassword();
            mPin.UserId = UserId;
            mPin = ForgotPasswordService.SendOTPForEmployee(mPin);
            if (mPin.Flag == 1)
            {
                string Message2 = LifeOne.Models.API.SMS.OTPMemberMessage2(mPin.MemberName, mPin.OTP, "OTP");
                string mobile = mPin.MobileNo;
                LifeOne.Models.API.SMS.SendSMSTempOTP(mobile, Message2, ConfigurationManager.AppSettings["TEMP-SMS-OTP-Member"].ToString());
            }
            return Json(mPin, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MatchOTP(string UserId, string MobileNo, string OTP)
        {
            LifeOne.Models.API.ResultSet res = new LifeOne.Models.API.ResultSet();
            MForgotPassword mPin = new MForgotPassword();
            mPin.UserId = UserId;
            mPin.MobileNo = MobileNo;
            mPin.OTP = OTP;
            if (Session["PageNo"] != null)
                mPin.ProdID = Convert.ToInt32(Session["PageNo"].ToString());
            res = ForgotPasswordService.MatchOTP(mPin);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult VerifyOTP(string UserId, string MobileNo, string OTP)
        {
            LifeOne.Models.API.ResultSet res = new LifeOne.Models.API.ResultSet();
            MForgotPassword mPin = new MForgotPassword();
            mPin.UserId = UserId;
            mPin.MobileNo = MobileNo;
            mPin.OTP = OTP;
            res = ForgotPasswordService.MatchOTPForEmployee(mPin);

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ForgetPassword(string UserId, string NewPassword)
        {
            int prodId = 0;
            if (Session["PageNo"] != null)
                prodId = Convert.ToInt32(Session["PageNo"].ToString());
            LifeOne.Models.API.ResultSet res = new LifeOne.Models.API.ResultSet();
            MForgotPassword objForgetPassword = new MForgotPassword();
            objForgetPassword.UserId = UserId;
            objForgetPassword.NormalPass = NewPassword;
            objForgetPassword.ProdID = prodId;
            objForgetPassword.NewPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(NewPassword, "md5");
            res = ForgotPasswordService.ForgotPassword(objForgetPassword);
            return Json(res, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Logout()
        {
            Session.Abandon();
            Session.Clear();
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        public ActionResult UnAuthorized()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult SetPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SetPassword(string OTP, string Password, string RePassword)
        {



            if (string.IsNullOrEmpty(OTP))
            {
                ViewBag.Status = "0";
                ViewBag.Msg = "Enter 6 digit valid OTP!";
                return View("SetPassword");
            }
            else if (!string.IsNullOrEmpty(OTP) && OTP.Length != 6)
            {
                ViewBag.Status = "0";
                ViewBag.Msg = "Enter 6 digit valid OTP!";
                return View("SetPassword");
            }
            else if (!string.IsNullOrEmpty(Password) && Password != RePassword)
            {
                ViewBag.Status = "0";
                ViewBag.Msg = "Password does not match!!!";
                return View("SetPassword");
            }
            else if (!string.IsNullOrEmpty(Password) && Password.Length < 6)
            {
                ViewBag.Status = "0";
                ViewBag.Msg = "You have to enter at least 6 digit password!";
                return View("SetPassword");
            }
            else if (!string.IsNullOrEmpty(Password) && Password.ToLower() == "password")
            {
                ViewBag.Status = "0";
                ViewBag.Msg = "Don’t use that password…any password consisting of a easy to guess word would be easy for a password cracking tool to determine.!";
                return View("SetPassword");
            }
            else if (Session["LoginUserId"] != null)
            {
                LifeOne.Models.API.ResultSet _res = new LifeOne.Models.API.ResultSet();
                MForgotPassword mPin = new MForgotPassword();
                mPin.UserId = Session["LoginUserId"].ToString();
                mPin.MobileNo = Session["MobileNo"].ToString();
                mPin.OTP = OTP;
                _res = ForgotPasswordService.MatchOTP(mPin);
                if (_res.Flag == 1)
                {
                    string ENCPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(Password, "md5");
                    LifeOne.Models.API.ResultSet res = ForgotPasswordService.SetNewPassword(Session["LoginUserId"].ToString(), Password, ENCPassword);
                    if (res.Flag == 1)
                    {
                        Session["MultiUserList"] = null;
                        Session["LoginUserId"] = null;
                        Session["MobileNo"] = null;
                        ViewBag.Status = "1";
                        ViewBag.Msg = res.Msg;
                        MLogin _model = new MLogin();
                        _model.Login = mPin.UserId;
                        _model.Password = Password;
                        HomeController _Home = new HomeController();
                        Session["PageNo"] = 3;
                        _Home.Login(_model);
                        DALLogin _obj = new DALLogin();
                        LoginResponse _objres = new LoginResponse();
                        _objres = _obj.CheckLogin(_model);
                        ViewBag.Status = "1";
                        ViewBag.Msg = res.Msg;
                        string Route = string.Empty;
                        if (_objres.IsBusinessId == "True")
                        {
                            Route = "../Associate-Dashboard";
                            //  return RedirectToAction("../Associate-Dashboard");
                        }
                        else
                        {
                            Route = "/Home/MemberLogin";
                            // return RedirectToAction("../UID-Dashboard");
                        }
                        ViewBag.Route = Route;
                        return View("SetPassword");
                    }
                    else
                    {
                        ViewBag.Status = "0";
                        ViewBag.Msg = res.Msg;
                        return View("SetPassword");
                    }
                }
                else
                {
                    ViewBag.Status = "0";
                    ViewBag.Msg = _res.Msg;
                    return View("SetPassword");
                }
            }
            else if (Session["LoginUserId"] == null)
            {
                ViewBag.Status = "0";
                ViewBag.Msg = "OTP has been expired";
                return View("SetPassword");
            }
            return View();
        }


        public ActionResult comingsoon()
        {
            return View();
        }
        public ActionResult OtherPage()
        {
            return View();
        }

        public JsonResult GetPincodeDetails(string PinCode)
        {
            CustomerDb objcusdb = new CustomerDb();
            LifeOne.Models.API.MPincode _bj = new LifeOne.Models.API.MPincode();
            _bj.PinCode = PinCode;
            return Json(objcusdb.GetPincodeDetais(_bj));
        }

        public JsonResult GetMemberDetailsForFranchiseApplicationForm(string BID)
        {
            return Json(CustomerRegService.GetMemberDetailsForFranchiseApplicationForm(BID));
        }
        [HttpGet]
        public ActionResult FranchiseApplicationForm()
        {
            return View();
        }
        [HttpPost]
        public ActionResult FranchiseApplicationForm(FranchiseApplicationFormModel req)
        {
            FranchiseRegistrationResponse _response = CustomerRegService.SaveFranchiseApplicationForm(req);
            ViewBag.Status = _response.Flag == 1 ? true : false;
            ViewBag.Msg = _response.Remark;
            return View();
        }

        [HttpPost]
        public JsonResult _FranchiseApplicationForm(FranchiseApplicationFormModel req)
        {
            try
            {
                List<FranchiseRequestDocumentModel> _req = new List<FranchiseRequestDocumentModel>();
                List<FranchiseRequestBankModel> _BNKObjList = new List<FranchiseRequestBankModel>();
                FranchiseRequestDocumentModel _obj = new FranchiseRequestDocumentModel();
                _obj.DocType = req.DocType;
                _obj.DocNumber = req.DocNo;
                _obj.DocFile = req.AttachmentFile;
                _obj.RDID = 0;
                _obj.FK_FranchiseId = 0;
                _obj.IsDelete = false;
                _obj.CreateDate = DateTime.Now;
                _req.Add(_obj);
                req.FranchiseRequestDocumentList = _req;
                FranchiseRequestBankModel _BNKObj = new FranchiseRequestBankModel();
                _BNKObj.AccountNo = req.AccountNo;
                _BNKObj.BankName = req.BankName;
                _BNKObj.Branch = req.Branch;
                _BNKObj.IFSC_CODE = req.IFSC_CODE;
                _BNKObj.GSTNo = req.GSTNo;
                _BNKObjList.Add(_BNKObj);
                req.FranchiseRequestBankList = _BNKObjList;
                FranchiseRegistrationResponse _response = CustomerRegService.SaveFranchiseApplicationForm(req);
                return Json(_response);
            }
            catch (Exception e)
            {
                return Json(new FranchiseRegistrationResponse { Flag = 0, message = "#Error" });
            }

        }

        public ActionResult _RegCommingSoon()
        {
            return View();
        }

        //public void CheckRegistrationAllowOrNot()
        //{
        //    CheckCampingModel _response = DALLogin.CheckCamping();
        //    if (_response.Status)
        //    {
        //        Session["RegistrationAllow"] = true;
        //    }
        //    else
        //    {
        //        Session["RegistrationAllow"] = false;
        //    }
        //}
        [HttpGet]
        public ActionResult FranchiseLogin()
        {
            MLogin m = new MLogin();
            return View(m);
        }
        [HttpGet]
        public ActionResult FranchiseLoginV1()
        {
            MLogin m = new MLogin();
            return View(m);
        }

        [HttpPost]
        public ActionResult FranchiseLogin(MLogin _model)
        {
            DALLogin _obj = new DALLogin();
            LoginResponse _objres = new LoginResponse();
            if (ModelState.IsValid)
            {
                string _P = _model.Password;
                _model.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(_model.Password, "md5");
                _objres = _obj.CheckFranchiseLogin(_model);
                // SessionManager.Fk_UserTypeId = Convert.ToInt32(_objres.UserType);
                if (_objres.UserType == "2") //Franchise Login
                {
                    SessionManager.Name = _objres.Name;
                    SessionManager.Usertype = _objres.UserType;
                    SessionManager.LoginId = _objres.LoginId;
                    SessionManager.SponsorFk_MemId = _objres.SponsorFk_MemId;
                    SessionManager.ProfilePic = _objres.ProfilePic;
                    SessionManager.PerformanceLevel = _objres.PerformanceLevel;
                    SessionManager.IsBusinessId = _objres.IsBusinessId;
                    // SessionManager.Fk_UserTypeId = Convert.ToInt32(_objres.UserType);
                    TempData["Msg"] = _objres.response;

                    SessionManager.FranchiseFk_MemId = Convert.ToInt64(_objres.Fk_MemId);
                    return RedirectToAction("../Franchise-Dashboard");

                }
                else if (_objres.UserType == "4") //Franchise Child Login
                {
                    SessionManager.Name = _objres.Name;
                    SessionManager.Usertype = _objres.UserType;
                    SessionManager.LoginId = _objres.LoginId;
                    SessionManager.SponsorFk_MemId = _objres.SponsorFk_MemId;
                    SessionManager.ProfilePic = _objres.ProfilePic;
                    SessionManager.PerformanceLevel = _objres.PerformanceLevel;
                    SessionManager.IsBusinessId = _objres.IsBusinessId;
                    // SessionManager.Fk_UserTypeId = Convert.ToInt32(_objres.UserType);
                    TempData["Msg"] = _objres.response;
                    return RedirectToAction("Franchise/FranchiseChildDashboard", "Franchise");
                }

            }
            else
            {
                TempData["Msg"] = "All Field Required";
            }
            return View();
        }

        public JsonResult Franchise_GetOTP(string UserId)
        {
            MForgotPassword mPin = new MForgotPassword();
            mPin.ProdID = 2;
            mPin.UserId = UserId;
            mPin = ForgotPasswordService.SendOTP(mPin);
            if (mPin.Flag == 1)
            {
                string Message2 = LifeOne.Models.API.SMS.OTPMemberMessage2(mPin.MemberName, mPin.OTP, "OTP");
                string mobile = mPin.MobileNo;
                LifeOne.Models.API.SMS.SendSMSTempOTP(mobile, Message2, ConfigurationManager.AppSettings["TEMP-SMS-OTP-Member"].ToString());
            }
            return Json(mPin, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Franchise_MatchOTP(string UserId, string MobileNo, string OTP)
        {
            LifeOne.Models.API.ResultSet res = new LifeOne.Models.API.ResultSet();
            MForgotPassword mPin = new MForgotPassword();
            mPin.UserId = UserId;
            mPin.MobileNo = MobileNo;
            mPin.OTP = OTP;
            mPin.ProdID = 2;
            res = ForgotPasswordService.MatchOTP(mPin);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Franchise_ForgetPassword(string UserId, string NewPassword)
        {
            LifeOne.Models.API.ResultSet res = new LifeOne.Models.API.ResultSet();
            MForgotPassword objForgetPassword = new MForgotPassword();
            objForgetPassword.UserId = UserId;
            objForgetPassword.NormalPass = NewPassword;
            objForgetPassword.ProdID = 2;
            objForgetPassword.NewPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(NewPassword, "md5");
            res = ForgotPasswordService.ForgotPassword(objForgetPassword);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Franchise_ForgotPassword(string _)
        {
            _ = _.Replace(" ", "+");
            MForgotPassword _obj = new MForgotPassword();
            _obj.UserId = Crypto.Decryption(Aeskey, _);
            return View(_obj);
        }
        /*Admin Login*/
        [HttpGet]
        public ActionResult AdminLogin()
        {
            MLogin m = new MLogin();
            return View(m);
        }
        [HttpPost]
        public ActionResult AdminLogin(MLogin _model)
        {
            DALLogin _obj = new DALLogin();
            LoginResponse _objres = new LoginResponse();
            if (ModelState.IsValid)
            {
                string _P = _model.Password;
                _model.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(_model.Password, "md5");
                _objres = _obj.CheckLogin(_model);
                TempData["Msg"] = _objres.response;
                if (_objres.UserType == "3" || _objres.UserType == "5") //Admin Login
                {
                    SessionManager.Name = _objres.Name;
                    SessionManager.Usertype = _objres.UserType;
                    SessionManager.LoginId = _objres.LoginId;
                    SessionManager.SponsorFk_MemId = _objres.SponsorFk_MemId;
                    SessionManager.ProfilePic = _objres.ProfilePic;
                    TempData["Msg"] = _objres.response;

                    SessionManager.Fk_MemId = Convert.ToInt64(_objres.Fk_MemId);
                    SessionManager.Fk_UserTypeId = Convert.ToInt32(_objres.UserType);
                    SessionManager.UserPermissionDt = _obj.GetFormPermissionDetails(Convert.ToInt32(SessionManager.Fk_MemId));
                    // SessionManager._UserPermissionList = _obj.GetForm_Permission(SessionManager.Fk_MemId);
                    if (_objres.UserType == "3")
                    {
                        // AdminRenderMenu();
                        return RedirectToAction("../Admin-Dashboard");
                    }
                    // return RedirectToAction("../Admin/FranchiseOrderRequest/ViewNewRequest");

                    // return RedirectToAction("../Agriculture-Master");
                    else if (_objres.UserType == "5")
                    {
                        //AdminRenderMenu();
                        return RedirectToAction("../Employee-Dashboard");
                    }
                }
                else
                {
                    TempData["Msg"] = "Invalid LoginId Or Password";
                }
            }
            else
            {
                TempData["Msg"] = "All Field Required";
            }
            return View();
        }

        [HttpGet]
        public ActionResult MagentProduct()
        {
            MagentProductOrderLs bj = new MagentProductOrderLs();
            bj.SaveMegentoStock();

            return View();

        }

        [HttpGet]
        public ActionResult PrintInvoice(string OrderNo)
        {
            PrintInvoice obj = new PrintInvoice();


            PrintInvoice model = obj.PrintInvoiceDetail(OrderNo, null);


            return View(model);

        }

        [HttpGet]
        public ActionResult PrintInvoiceOld(string OrderNo)
        {
            PrintInvoice obj = new PrintInvoice();


            PrintInvoice model = obj.PrintInvoiceDetailOld(OrderNo, null);


            return View(model);

        }

        [HttpGet]
        public ActionResult PrintAssociateInvoice(string OrderNo)
        {
            PrintInvoice obj = new PrintInvoice();
            long Fk_memid = Convert.ToInt64(SessionManager.AssociateFk_MemId);

            PrintInvoice model = obj.PrintInvoiceDetail(OrderNo, Fk_memid.ToString());

            return View(model);

        }

        [HttpGet]
        public ActionResult privacy()
        {
            return View();
        }
        [HttpGet]
        public ActionResult ReturnPolicy()
        {
            return View();
        }
        public ActionResult Congratulations()
        {
            ViewBag.LoginId = Session["AssociateLoginId"];
            ViewBag.Password = Session["AssociatePassword"];
            ViewBag.Name = Session["AssociateName"];
            return View();
        }

        public ActionResult PaymentGatewayIntegration(Reports reports)
        {
            //TotalAmount = "1";
            //Reports reports = new Reports();
            reports.OpCode = 1;
            reports.FK_MemId = int.Parse(SessionManager.AssociateFk_MemId.ToString());
            Session["Pk_AddressId"] = reports.Pk_AddressId;
            Session["WalletAmount"] = reports.WalletAmount;
            // DataSet dataSet = reports.GetCartList();
            //string TotalAmount = double.Parse(dataSet.Tables[0].Compute("sum(price)", "").ToString()).ToString("0.00");
            if (reports.GatewayAmount > 0)
            {
                Session["RazorPayTotalAmt"] = reports.GatewayAmount;

                string orderId = "";

                Razorpay.Api.Order objorder = null;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                RazorpayClient client = null;
                client = new RazorpayClient(RazorPayLocalKey, RazorPayLocalSecret);
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", (reports.GatewayAmount) * 100);
                options.Add("receipt", "");
                options.Add("currency", "INR");
                options.Add("payment_capture", 1);
                objorder = client.Order.Create(options);
                orderId = objorder["id"].ToString();


                reports.OrderNo = orderId;
                reports.RazorPayAmount = ((reports.GatewayAmount) * 100).ToString();
            }
            else
            {
                return RedirectToAction("../AssociateReport/PlaceOrder");

            }
            return View(reports);

        }
        [HttpPost]
        public ActionResult GenerateShoppingToken()
        {
            Products _objService = new Products();
            DataSet res = _objService.GenerateShoppingToken();
            string Result = "";
            if (res.Tables.Count > 0 && res.Tables[0].Rows.Count > 0)
            {
                Result = res.Tables[0].Rows[0]["TokenNo"].ToString();
            }
            // Session["Checkout"] = "";

            return new JsonResult { Data = Result, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet };
        }
        [HttpPost]
        public ActionResult AddToCart(Products model)
        {
            //string Status = "AddToCart";
            //if (string.IsNullOrEmpty(SessionManager.LoginId))
            //{
            //    return RedirectToAction("Login", "Home", new { Pk_ProductId = model.Pk_ProductId, Quantity = model.Quantity, Status = Status });
            //}
            // Session["Checkout"] = "";
            long Result = 0;
            //products.Quantity = 1;
            //products.OpCode = 1;           
            // model.FK_MemId = int.Parse(SessionManager.AssociateFk_MemId.ToString());
            DataSet dataSet1 = model.ManageShoppingCart();
            if (dataSet1.Tables.Count > 0 && dataSet1.Tables[0].Rows.Count > 0)
            {
                model.DtDetails = dataSet1.Tables[0];
                Result = int.Parse(model.DtDetails.Rows[0]["Quantity"].ToString());
                SessionManager.TotalItems = int.Parse(model.DtDetails.Rows[0]["Quantity"].ToString());
                SessionManager.TokenNo = model.TokenNo;
            }
            return new JsonResult { Data = Result, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet };
        }

        public ActionResult TermsandConditions()
        {
            TermsCondition obj = new TermsCondition();
            try
            {
                List<TermsCondition> lstTermsCondition1 = new List<TermsCondition>();
                DataSet ds = obj.getTermsandCondition();

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        TermsCondition lstTermsCondition = new TermsCondition();
                        lstTermsCondition.TermsandCondition = ds.Tables[0].Rows[i]["TermsandCondition"].ToString();
                        lstTermsCondition1.Add(lstTermsCondition);
                    }
                    DataTable dataTable = DALCommon.ToDataTable(lstTermsCondition1);
                    obj.dtDetails = dataTable;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return View(obj);
        }

        public ActionResult MDMessage()
        {
            return View();
        }
        public ActionResult VisionMission()
        {
            return View();
        }

        public ActionResult SOPandBankDetails()
        {
            return View();
        }
        public ActionResult SocialMedioGuideline()
        {
            return View();
        }
        public ActionResult Disclaimer()
        {
            return View();
        }
        public ActionResult ProductsDisclaimer()
        {
            return View();
        }
        public ActionResult ProductReturnPolicy()
        {
            return View();
        }


        public virtual PartialViewResult AdminRenderMenu()
        {
            //Menu model = new Menu();

            //List<Menu> list = new List<Menu>();
            //List<Menu> list1 = new List<Menu>();
            //DataSet dataset = new DataSet();

            //dataset = model.GetMenuDetails();
            //if (dataset != null && dataset.Tables.Count > 0 && dataset.Tables[0].Rows.Count > 0)
            //{
            //    for (var i = 0; i < dataset.Tables[0].Rows.Count; i++)
            //    {
            //        Menu ListData = new Menu();
            //        ListData.MenuName = dataset.Tables[0].Rows[i]["MenuName"].ToString();
            //        ListData.MenuId = dataset.Tables[0].Rows[i]["MenuId"].ToString();
            //        ListData.Url = dataset.Tables[0].Rows[i]["MenuURL"].ToString();
            //        ListData.Icon = dataset.Tables[0].Rows[i]["MenuIcons"].ToString();
            //        ListData.IsDropdown = dataset.Tables[0].Rows[i]["Isdropdown"].ToString();
            //        //ListData.Icon = dataset.Tables[0].Rows[i]["MenuIcon"].ToString();
            //        list.Add(ListData);
            //    }
            //    for (int j = 0; j <= dataset.Tables[1].Rows.Count - 1; j++)
            //    {
            //        Menu SubList = new Menu();
            //        SubList.SubMenuName = dataset.Tables[1].Rows[j]["SubMenuName"].ToString();
            //        SubList.IsDropdown = dataset.Tables[1].Rows[j]["IsDropDown"].ToString();
            //        SubList.Fk_MenuId = dataset.Tables[1].Rows[j]["Fk_MenuId"].ToString();
            //        SubList.SubMenuId = int.Parse(dataset.Tables[1].Rows[j]["SubMenuId"].ToString());
            //        SubList.Url = dataset.Tables[1].Rows[j]["SubMenuUrl"].ToString();
            //       // SubList.Icon = dataset.Tables[1].Rows[j]["SubMenuIcon"].ToString();
            //        list1.Add(SubList);
            //    }

            //}
            //model.menuList = list;
            //model.subMenuList = list1;
            //HttpContext.Session.SetObjectAsJson("_menu", model);
            //HttpContext.Session.SetObjectAsJson("_submenu", model);
            return PartialView("_AdminPartialLayout");
        }

        [HttpPost]
        public JsonResult UpdateShoppingQuantity(Products products, string Id, string Qty)
        {
            List<Products> dtList = new List<Products>();
            products.Pk_ProductId = int.Parse(Id);
            products.Quantity = products.Qty;
            products.OpCode = 4;
            products.TokenNo = SessionManager.TokenNo;
            DataSet ds = products.ManageShoppingCart();
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Products list = new Products();
                    //list.Fk_ProductId = Convert.ToInt32(ds.Tables[0].Rows[i]["Fk_ProductId"].ToString());
                    list.Quantity = Convert.ToInt32(ds.Tables[0].Rows[i]["Quantity"].ToString());
                    list.SubTotal = ds.Tables[0].Rows[i]["SubTotal"].ToString();
                    list.TotalAmount = ds.Tables[0].Rows[0]["TotalAmount"].ToString();
                    dtList.Add(list);
                }
            }
            return Json(dtList, JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        public ActionResult FooterRender()
        {
            Contact obj = new Contact();
            try
            {
                DataSet ds = obj.getInfgo();
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        obj.Address1 = ds.Tables[0].Rows[i]["CompanyAddress"].ToString();
                        obj.Address2 = ds.Tables[0].Rows[i]["CompanyAddress1"].ToString();
                        obj.Mobile2 = ds.Tables[0].Rows[i]["contactNo1"].ToString();
                        obj.Mobile = ds.Tables[0].Rows[i]["ContactNo"].ToString();
                        obj.Email = ds.Tables[0].Rows[i]["companyEmail"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return PartialView("_FooterPartialLayout", obj);

        }
        public ActionResult CmdDesk()
        {
            return View();
        }
        public ActionResult ImageGallery()
        {
            UploadImageVideo obj = new UploadImageVideo();
            try
            {
                List<UploadImageVideo> ImageList1 = new List<UploadImageVideo>();
                DataSet ds = obj.getImagegallery();

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        UploadImageVideo ImageList = new UploadImageVideo();
                        if (!string.IsNullOrEmpty(ds.Tables[0].Rows[i]["ImageUrl"].ToString()))
                        {
                            ImageList.ImageUrl = baseurl + ds.Tables[0].Rows[i]["ImageUrl"].ToString();
                        }
                        else
                        {
                            ImageList.ImageUrl = baseurlNoImg;
                        }
                        //ImageList.ImageUrl = ds.Tables[0].Rows[i]["ImageUrl"].ToString();
                        ImageList1.Add(ImageList);                                               
                    }
                    DataTable dataTable = DALCommon.ToDataTable(ImageList1);
                    obj.dtDetails = dataTable;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return View(obj);
        }

        public ActionResult Video()
        {
            UploadImageVideo obj = new UploadImageVideo();
            try
            {
                List<UploadImageVideo> ImageList1 = new List<UploadImageVideo>();
                DataSet ds = obj.getVideogallery();

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        UploadImageVideo ImageList = new UploadImageVideo();
                        ImageList.Videolink = ds.Tables[0].Rows[i]["Videolink"].ToString();
                        ImageList1.Add(ImageList);
                    }
                    DataTable dataTable = DALCommon.ToDataTable(ImageList1);
                    obj.dtDetails = dataTable;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return View(obj);
            //  return View();
        }
    }
}