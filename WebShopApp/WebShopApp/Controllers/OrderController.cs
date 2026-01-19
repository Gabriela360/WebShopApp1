using Microsoft.AspNetCore.Mvc;

using System.Globalization;
using System.Security.Claims;

using WebShopApp.Core.Contacts;
using WebShopApp.Infrastructure.Data.Domain;
using WebShopApp.Models.Order;

namespace WebShopApp.Controllers
{
    public class OrderController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public OrderController(IProductService productService, IOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
        }

        public ActionResult Create(int id)
        {
            Product product = _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            //ако има продукт - създава се поръчка във формата за поръчка
            OrderCreateVM order = new OrderCreateVM()
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                QuantityInStock = product.Quantity,
                Price = product.Price,
                Discount = product.Discount,
                Picture = product.Picture
            };
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(OrderCreateVM bindingModel)
        {
            string currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = this._productService.GetProductById(bindingModel.ProductId);
            if (currentUserId == null || product == null || product.Quantity < bindingModel.Quantity)
            {
                return RedirectToAction("Denied", "Order");
            }

            // ако потребителят не съществува или продуктът не съществува или няма достатъчно наличност
            if (ModelState.IsValid)
            {
                _orderService.Create(bindingModel.ProductId, currentUserId, bindingModel.Quantity);
            }

            // при успешна поръчка се връща списъка на продуктите
            return this.RedirectToAction("Index", "Product");
        }


        public ActionResult Denied()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MyOrders()
        {
            string currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            // var user = _context.Users.SingleOrDefault(u => u.Id == userId);

            List<OrderIndexVM> orders = _orderService.GetOrdersByUser(currentUserId)
                .Select(x => new OrderIndexVM
                {
                    Id = x.Id,
                    OrderDate = x.OrderDate.ToString("dd-MMM-yyyy hh:mm", CultureInfo.InvariantCulture),
                    UserId = x.UserId,
                    User = x.User.UserName,
                    ProductId = x.ProductId,
                    Product = x.Product.ProductName,
                    Picture = x.Product.Picture,
                    Quantity = x.Quantity,
                    Price = x.Price,
                    Discount = x.Discount,
                    TotalPrice = x.TotalPrice
                }).ToList();

            return View(orders);
        }
    }
}
