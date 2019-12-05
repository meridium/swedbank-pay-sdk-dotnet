﻿namespace Sample.AspNetCore.Controllers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    using Sample.AspNetCore.Models;
    using Sample.AspNetCore.Data;

    using SwedbankPay.Sdk;
    using SwedbankPay.Sdk.PaymentOrders;
    using SwedbankPay.Sdk.Transactions;

    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class PaymentController : Controller
    {
        private Cart cartService;
        private readonly StoreDbContext context;
        private readonly SwedbankPayClient swedbankPayClient;

        public PaymentController( Cart cartService, StoreDbContext context, SwedbankPayClient swedbankPayClient)
        {
            this.cartService = cartService;
            this.context = context;
            this.swedbankPayClient = swedbankPayClient;
        }


        // GET: Payment
        public async Task<ActionResult> Index(Cart cart)
        {
            return View();
        }

        // GET: Payment/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Payment/Create
        public ActionResult Create(Cart cart)
        {
            //var swedbankPayClient.PaymentOrders.CreatePaymentOrder();
            return View();
        }

        // POST: Payment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Payment/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }


        [HttpGet]

        public async Task<ActionResult> CancelPayment(string paymentOrderId)
        {
            try
            {
                var transactionRequestObject = new TransactionRequest
                {
                    PayeeReference = DateTime.Now.Ticks.ToString(),
                    Description = "Cancelling parts of the total amount"
                };
               
                var paymentOrder = await this.swedbankPayClient.PaymentOrder.Get(paymentOrderId);
                var container = new TransactionRequestContainer(transactionRequestObject);
                if (paymentOrder.Operations.Cancel != null)
                {
                    var response = await paymentOrder.Operations.Cancel.Execute(container);
                    TempData["CancelMessage"] = $"Payment has been cancelled: {response.Cancellation.Transaction.Id}";
                    this.cartService.PaymentOrderLink = null;
                }
                else
                {
                    TempData["ErrorMessage"] = $"Operation not available";
                }
                return RedirectToAction(nameof(Details), "Orders");
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $"Something unexpected happened. {e.Message}";
                return RedirectToAction(nameof(Details), "Orders");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AbortPaymentOrder(string paymentOrderId)
        {
            try
            {
                var paymentOrder = await this.swedbankPayClient.PaymentOrder.Get(paymentOrderId);
                
                var response = await paymentOrder.Operations.Abort.Execute();
                
                TempData["AbortMessage"] = $"Payment Order: {response.PaymentOrder.Id} has been {response.PaymentOrder.State}";
                this.cartService.PaymentOrderLink = null;

                return RedirectToAction(nameof(Index), "Products");
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $"Something unexpected happened. {e.Message}";
                return RedirectToAction(nameof(Index), "Orders");
            }

        }
        
        public async Task<IActionResult> CapturePayment(string paymentOrderId)
        {
            try
            {
                var transActionRequestObject = await GetTransactionRequest("Capturing the authorized payment");
                var paymentOrder = await this.swedbankPayClient.PaymentOrder.Get(paymentOrderId);

                var container = new TransactionRequestContainer(transActionRequestObject);
                var response = await paymentOrder.Operations.Capture.Execute(container);
                
                TempData["CaptureMessage"] = $"{response.Capture.Transaction.Id}, {response.Capture.Transaction.State}, {response.Capture.Transaction.Type}"; //TODO this looks awkward?
                
                this.cartService.PaymentOrderLink = null;

                    return RedirectToAction(nameof(Details), "Orders");
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $"Something unexpected happened. {e.Message}";
                return RedirectToAction(nameof(Details), "Orders");
            }
        }

        public async Task<IActionResult> ReversalPayment(string paymentOrderId)
        {
            try
            {
                var transActionRequestObject = await GetTransactionRequest("Reversing the capture amount");
                //var paymentOrder = await this.swedbankPayClient.GetPaymentOrder(paymentOrderId);
                var paymentOrder = await this.swedbankPayClient.PaymentOrder.Get(paymentOrderId);
                var container = new TransactionRequestContainer(transActionRequestObject);
                var response = await paymentOrder.Operations.Reversal.Execute(container);

                TempData["ReversalMessage"] = $"{response.Reversal.Transaction.Id}, {response.Reversal.Transaction.Type}, {response.Reversal.Transaction.State}";
                this.cartService.PaymentOrderLink = null;

                return RedirectToAction(nameof(Details), "Orders");
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $"Something unexpected happened. {e.Message}";
                return RedirectToAction(nameof(Details), "Orders");
            }
        }

        private async Task<TransactionRequest> GetTransactionRequest(string description)
        {
            var order = await this.context.Orders.Include(l => l.Lines).ThenInclude(p => p.Product).FirstOrDefaultAsync();

            var orderItems = order.Lines.Select(line => new OrderItem
            {
                Amount = (int?)line.CalculateTotal() * 100,
                Quantity = line.Quantity,
                Reference = line.Product.Reference,
                Name = line.Product.Name,
                Type = line.Product.Type,
                Class = line.Product.Class,
                ItemUrl = "https://example.com/products/123",
                ImageUrl = "https://example.com/products/123.jpg",
                Description = "Product 1 description",
                QuantityUnit = "pcs",
                UnitPrice = line.Product.Price * 100,
                VatPercent = 0, //TODO Correct VatPercent
                VatAmount = 0, //TODO Correct VatAmount
            }).ToList();

            var transActionRequestObject = new TransactionRequest
            {
                PayeeReference = DateTime.Now.Ticks.ToString(),
                Amount = order.Lines.Sum(e => e.Quantity * e.Product.Price) * 100,
                VatAmount = 0, //TODO Correct amount
                Description = description,
                OrderItems = orderItems
            };

            return transActionRequestObject;

        }


        [HttpPost]
        public void OnCompleted(string paymentLinkId)
        {
            var products = this.cartService.CartLines.Select(p => p.Product);
            this.context.Products.AttachRange(products);

            this.context.Orders.Add(new Order
            {
                PaymentOrderLink = this.cartService.PaymentOrderLink,
                PaymentLink = paymentLinkId,
                Lines = this.cartService.CartLines.ToList()
            });
            this.context.SaveChanges(true);
        }





    }
}