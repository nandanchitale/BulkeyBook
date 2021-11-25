using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace BulkyBook.web.Areas.Customer.Controllers
{
    public class CartController : Controller
    {
        //       Severity	Code	Description	Project	File	Line	Suppression State
        //       Error MSB3027 Could not copy "obj\Debug\net6.0\BulkyBook.web.dll" to "bin\Debug\net6.0\BulkyBook.web.dll". Exceeded retry count of 10. Failed.The file is locked by: "Microsoft Visual Studio 2022 (3844)"	BulkyBook.web C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\Microsoft.Common.CurrentVersion.targets	4635	


        public readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartViewModel ShoppingCartViewModel { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {

            ShoppingCartViewModel shoppingCartViewModel = new ShoppingCartViewModel()
            {
                CartListItems = _unitOfWork.ShoppingCart.GetAll(includeProperties: "product"),
                OrderHeader = new()
            };

            foreach (var cart in shoppingCartViewModel.CartListItems)
            {
                cart.price = getPriceBasedOnQuantity(cart.Count, cart.product.Price,
                    cart.product.Price50, cart.product.Price100);
                shoppingCartViewModel.OrderHeader.OrderTotal += (cart.price * cart.Count);
            }

            return View(shoppingCartViewModel);
        }

        // Get
        public IActionResult Summary()
        {
            ShoppingCartViewModel = new ShoppingCartViewModel()
            {
                CartListItems = _unitOfWork.ShoppingCart.GetAll(includeProperties: "product"),
                OrderHeader = new()
            };

            ShoppingCartViewModel.OrderHeader.Name = ShoppingCartViewModel.OrderHeader.Name;
            ShoppingCartViewModel.OrderHeader.PhoneNumber = ShoppingCartViewModel.OrderHeader.PhoneNumber;
            ShoppingCartViewModel.OrderHeader.StreetAddress = ShoppingCartViewModel.OrderHeader.StreetAddress;
            ShoppingCartViewModel.OrderHeader.City = ShoppingCartViewModel.OrderHeader.City;
            ShoppingCartViewModel.OrderHeader.State = ShoppingCartViewModel.OrderHeader.State;
            ShoppingCartViewModel.OrderHeader.PostalCode = ShoppingCartViewModel.OrderHeader.PostalCode;

            foreach (var cart in ShoppingCartViewModel.CartListItems)
            {
                cart.price = getPriceBasedOnQuantity(cart.Count, cart.product.Price,
                    cart.product.Price50, cart.product.Price100);
                ShoppingCartViewModel.OrderHeader.OrderTotal += (cart.price * cart.Count);
            }

            return View(ShoppingCartViewModel);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST()
        {

            ShoppingCartViewModel.CartListItems = _unitOfWork.ShoppingCart.GetAll(includeProperties:"product");

            ShoppingCartViewModel.OrderHeader.OrderDate = System.DateTime.Now;

            foreach (var cart in ShoppingCartViewModel.CartListItems)
            {
                cart.price = getPriceBasedOnQuantity(cart.Count, cart.product.Price,
                    cart.product.Price50, cart.product.Price100);
                ShoppingCartViewModel.OrderHeader.OrderTotal += (cart.price * cart.Count);
            }

            //if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            //{
            //    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            //    ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            //}
            //else
            //{
            //    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            //    ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            //}

            _unitOfWork.OrderHeader.Add(ShoppingCartViewModel.OrderHeader);
            _unitOfWork.Save();
            foreach (var cart in ShoppingCartViewModel.CartListItems)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartViewModel.OrderHeader.Id,
                    Price = cart.price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            //stripe settings 
            var domain = "https://localhost:44357/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                  "card",
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartViewModel.OrderHeader.Id}",
                CancelUrl = domain + $"Customer/Cart/Index",
            };

            foreach (var item in ShoppingCartViewModel.CartListItems)
            {

                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.price * 100),//20.00 -> 2000
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.product.Title
                        },

                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);

            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.updateStripePaymentId(ShoppingCartViewModel.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            // Check the stripe status
            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                _unitOfWork.Save();

            }

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(includeProperties: "product").ToList();

            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartViewModel.CartListItems);
            _unitOfWork.Save();
            TempData["Success"] = "Payment Successful";
            return View(id);

        }

        public double getPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price;
            }
            else
            {
                if (quantity <= 100)
                {
                    return price50;
                }
                return price100;
            }
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

    }
}
