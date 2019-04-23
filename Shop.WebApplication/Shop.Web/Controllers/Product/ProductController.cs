﻿using Shop.Business.Services;
using Shop.Shared.Entities;
using Shop.Shared.Entities.Authorize;
using Shop.Web.Attributes;
using Shop.Web.Models.ProductViewModels;
using System;
using System.Web.Mvc;

namespace Shop.Web.Controllers.Product
{
    public class ProductController : Controller
    {
        private ProductService _productService = new ProductService();
        private PurchaseService _purchaseService = new PurchaseService();
        private CategoryService _categoryService = new CategoryService();
        private UserService _userService = new UserService();
        private StateService _stateService = new StateService();
        private LocationService _locationService = new LocationService();

        [User]
        public ActionResult AddNewProduct()
        {
            ViewBag.Categories = _categoryService.GetAll();
            ViewBag.States = _stateService.GetAll();
            return View(new ProductViewModel());
        }

        [User]
        [HttpPost]
        public ActionResult AddNewProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _categoryService.GetAll();
                ViewBag.States = _stateService.GetAll();
                return View(model);
            }
            var user = User as UserPrinciple;
            if (!_locationService.IsExists(model.LocationOfProduct))
            {
                _locationService.Save(model.LocationOfProduct);
            }
            _productService.Save(new Shared.Entities.Product
            {
                Name = model.Name,
                Category = model.Category,
                CreationDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                Description = model.Description,
                LocationOfProduct = model.LocationOfProduct,
                Price = model.Price,
                State = new State
                {
                    Id = model.State.Id,
                },
                Author = new User
                {
                    Id = user.UserId
                }
            });
            ViewBag.Message = $"Товар \"{model.Name}\" добавлен в каталог и будет отображаться у всех дользователей!";
            return View("~/Views/Shared/Notification.cshtml");
        }

        public ActionResult ShowProductList()
        {
            ViewBag.Message = "Список всех товаров";
            ViewBag.Products = _productService.GetAll();
            return View();
        }

        public ActionResult ShowByUserId(int id)
        {
            ViewBag.Products = _productService.GetByUserId(id);
            ViewBag.Message = $"Товары пользователя {_userService.GetById(id).Login}";
            return View("~/Views/Product/ShowProductList.cshtml");
        }

        public ActionResult ShowProductInfo(int id)
        {
            ViewBag.Product = _productService.GetProductById(id);
            return View();
        }

        [User]
        [HttpGet]
        public ActionResult BuyProduct(int id)
        {
            ViewBag.Product = _productService.GetProductById(id);
            return View();
        }

        [User]
        [HttpPost]
        public ActionResult BuyProduct(string address, int productId)
        {
            var user = User as UserPrinciple;
            Shared.Entities.Product product = _productService.GetProductById(productId);
            Purchase purchase = new Purchase
            {
                Seller = product.Author,
                Customer = _userService.GetByLogin(user.Name),
                Product = product,
                Address = address,
                Date = DateTime.Now
            };
            _purchaseService.Save(purchase);
            _productService.DeleteById(purchase.Product.Id);
            ViewBag.Purchase = purchase;
            return View("~/Views/Product/ShowPurchaseInfo.cshtml");
        }


        public ActionResult OpenCategoryMenu()
        {
            ViewBag.Сategories = _categoryService.GetAll();
            return View();
        }

        public ActionResult ShowProductsByCategory(int categoryId)
        {
            ViewBag.Products = _productService.GetProductsByCategoryId(categoryId);
            return View("~/Views/Product/ShowProductList.cshtml");
        }

        [User]
        public ActionResult Delete(int id)
        {
            ViewBag.Message = $"Товар \"{_productService.GetProductById(id).Name}\" удален успешно!";
            _productService.DeleteById(id);
            return View("~/Views/Shared/Notification.cshtml");
        }

        [User]
        public ActionResult EditProduct(int id)
        {
            var user = User as UserPrinciple;
            var product = _productService.GetProductById(id);

            if(product.Author.Id != user.UserId)
            {
                ViewBag.ErrorText = "Данный товар вам не принадлежит!";
                return View("~/Shared/Error.cshtml");
            }

            ViewBag.Categories = _categoryService.GetAll();
            ViewBag.States = _stateService.GetAll();

            return View(new EditProductViewModel
            {
                Id = id,
                Name = product.Name,
                Description = product.Description,
                Category = product.Category,
                Author = product.Author,
                Price = product.Price,
                State = product.State,
                LocationOfProduct = product.LocationOfProduct
            });
        }

        [User]
        [HttpPost]
        public ActionResult EditProduct(EditProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _categoryService.GetAll();
                ViewBag.States = _stateService.GetAll();
                return View(model);
            }
            if (!_locationService.IsExists(model.LocationOfProduct))
            {
                _locationService.Save(model.LocationOfProduct);
            }
            _productService.Edit(new Shared.Entities.Product
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                LocationOfProduct = model.LocationOfProduct,
                LastModifiedDate = DateTime.Now,
                State = model.State,
                Category = model.Category,
                Author = model.Author
            });
            ViewBag.Message = $"Товар \"{model.Name}\" изменён успешно!";
            return View("~/Views/Shared/Notification.cshtml");
        }
    }
}
