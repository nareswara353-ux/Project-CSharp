using Application.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs;

public class DailyReportJob
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<DailyReportJob> _logger;

    public DailyReportJob(
        IRepository<Customer> customerRepository,
        IRepository<Order> orderRepository,
        IRepository<Product> productRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<DailyReportJob> logger)
    {
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("DailyReportJob started at {Time}", _dateTimeProvider.UtcNow);

        try
        {
            var today = _dateTimeProvider.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var totalCustomers = await _customerRepository.CountAsync();
            var activeCustomers = await _customerRepository.CountAsync(c => c.IsActive);
            var totalProducts = await _productRepository.CountAsync();
            var activeProducts = await _productRepository.CountAsync(p => p.IsActive);
            var totalOrders = await _orderRepository.CountAsync();
            var confirmedOrders = await _orderRepository.CountAsync(o => o.Status == OrderStatus.Confirmed);
            var shippedOrders = await _orderRepository.CountAsync(o => o.Status == OrderStatus.Shipped);

            var ordersToday = await _orderRepository.CountAsync(
                o => o.OrderDate >= today && o.OrderDate < tomorrow);

            _logger.LogInformation(
                "Daily Report | Customers: {TotalCustomers} (Active: {ActiveCustomers}) | " +
                "Products: {TotalProducts} (Active: {ActiveProducts}) | " +
                "Orders: {TotalOrders} (Confirmed: {ConfirmedOrders}, Shipped: {ShippedOrders}) | " +
                "Orders Today: {OrdersToday}",
                totalCustomers,
                activeCustomers,
                totalProducts,
                activeProducts,
                totalOrders,
                confirmedOrders,
                shippedOrders,
                ordersToday);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DailyReportJob failed");
            throw;
        }
    }
}
