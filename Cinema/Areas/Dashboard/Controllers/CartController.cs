using Cinema.Models;
using Cinema.Repositories;
using Cinema.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace Cinema.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Movie> _movieRepository; private readonly IRepository<Promotion> _promotionRepository;

        public CartController(
            UserManager<ApplicationUser> userManager,
            IRepository<Cart> cartRepository,
            IRepository<Movie> movieRepository,
            IRepository<Promotion> promotionRepository) // <-- جديد
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _movieRepository = movieRepository;
            _promotionRepository = promotionRepository;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var moviesInCart = await _cartRepository.GetAsync(
                e => e.ApplicationUserId == user.Id,
                includeProperties: "Movie"
            );

            return View(moviesInCart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int movieId, int count = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var movie = await _movieRepository.GetOneAsync(e => e.Id == movieId);
            if (movie == null) return NotFound();

            var cartItem = await _cartRepository.GetOneAsync(
                e => e.MovieId == movieId && e.ApplicationUserId == user.Id
            );

            if (cartItem != null)
                cartItem.Count += count;
            else
                await _cartRepository.CreateAsync(new Cart
                {
                    ApplicationUserId = user.Id,
                    MovieId = movie.Id,
                    Count = count,
                    Price = movie.Price
                });

            await _cartRepository.CommitAsync();
            TempData["success-notification"] = $"{movie.Name} added to cart!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> IncrementCount(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var cartItem = await _cartRepository.GetOneAsync(
                e => e.MovieId == movieId && e.ApplicationUserId == user.Id
            );
            if (cartItem == null) return NotFound();

            cartItem.Count += 1;
            await _cartRepository.CommitAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DecrementCount(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var cartItem = await _cartRepository.GetOneAsync(
                e => e.MovieId == movieId && e.ApplicationUserId == user.Id
            );
            if (cartItem == null) return NotFound();

            if (cartItem.Count > 1)
                cartItem.Count -= 1;

            await _cartRepository.CommitAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var cartItem = await _cartRepository.GetOneAsync(
                e => e.MovieId == movieId && e.ApplicationUserId == user.Id
            );
            if (cartItem == null) return NotFound();

            await _cartRepository.DeleteAsync(cartItem.Id);
            await _cartRepository.CommitAsync();

            TempData["success-notification"] = "Movie removed from cart.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> ApplyPromo(string code)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var cart = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includeProperties: "Movie");
            if (!cart.Any())
            {
                TempData["error-notification"] = "Cart is empty.";
                return RedirectToAction("Index");
            }

            var promo = await _promotionRepository.GetOneAsync(
                p => p.Code == code && p.IsValid && p.ValidTo >= DateTime.Now
            );

            if (promo == null || promo.UsedCount >= promo.MaxUsage)
            {
                TempData["error-notification"] = "Invalid or expired code.";
                return RedirectToAction("Index");
            }

            // تطبيق الخصم على كل الأفلام أو على الفيلم المحدد
            foreach (var item in cart)
            {
                if (promo.MovieId == 0 || item.MovieId == promo.MovieId) // 0 يعني الكود عام لكل الأفلام
                {
                    decimal discountValue = item.Price * promo.Discount / 100;
                    item.Price -= discountValue;
                }
            }

            promo.UsedCount++;
            await _promotionRepository.CommitAsync();
            await _cartRepository.CommitAsync();

            TempData["success-notification"] = $"Promo applied! Discount: {promo.Discount}%";
            return RedirectToAction("Index");
        }


        // دفع عبر Stripe
        [HttpPost]
        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var cart = await _cartRepository.GetAsync(
                e => e.ApplicationUserId == user.Id,
                includeProperties: "Movie"
            );

            if (!cart.Any())
            {
                TempData["error-notification"] = "Cart is empty.";
                return RedirectToAction("Index");
            }

            // Calculate total price after discounts
            decimal total = cart.Sum(x => x.Price * x.Count);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Dashboard/Cart/Success",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Dashboard/Cart/Cancel",
            };

            foreach (var item in cart)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Movie.Name,
                            Description = item.Movie.Description,
                        },
                        UnitAmount = (long)(item.Price * 100), // price after promo discount
                    },
                    Quantity = item.Count,
                });
            }

            var service = new SessionService();
            var session = service.Create(options);
            return Redirect(session.Url);
        }


        public async Task<IActionResult> Success()
        {
            var user = await _userManager.GetUserAsync(User);

            var cart = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id);
            foreach (var item in cart)
                await _cartRepository.DeleteAsync(item.Id);

            await _cartRepository.CommitAsync();

            TempData["success-notification"] = "Payment completed successfully!";
            return RedirectToAction("Index");
        }

        public IActionResult Cancel()
        {
            TempData["error-notification"] = "Payment cancelled.";
            return RedirectToAction("Index");
        }
    }
}
