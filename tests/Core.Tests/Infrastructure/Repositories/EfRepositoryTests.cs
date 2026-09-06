using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Core.Tests.Infrastructure.Repositories;

public class EfRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IRepository<Customer> _repository;
    private readonly Email _email;
    private readonly Address _address;

    public EfRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new EfRepository<Customer>(_context);
        _email = Email.Create("test@example.com");
        _address = new Address("123 Test St", "Test City", "TS", "12345", "Test Country");
    }

    [Fact]
    public async Task AddAsync_ShouldAddCustomerToDatabase()
    {
        // Arrange
        var customer = new Customer("John", "Doe", _email, _address);

        // Act
        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();

        // Assert
        var result = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be(_email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCustomer_WhenExists()
    {
        // Arrange
        var customer = new Customer("Jane", "Smith", _email, _address);
        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customer.Id);
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCustomers()
    {
        // Arrange
        var customer1 = new Customer("John", "Doe", _email, _address);
        var customer2 = new Customer("Jane", "Smith", _email, _address);
        await _repository.AddAsync(customer1);
        await _repository.AddAsync(customer2);
        await _repository.SaveChangesAsync();

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(c => c.FirstName == "John" && c.LastName == "Doe");
        results.Should().Contain(c => c.FirstName == "Jane" && c.LastName == "Smith");
    }

    [Fact]
    public async Task GetAllAsync_WithPredicate_ShouldReturnFilteredCustomers()
    {
        // Arrange
        var customer1 = new Customer("John", "Doe", _email, _address);
        var customer2 = new Customer("Jane", "Smith", Email.Create("jane@example.com"), _address);
        await _repository.AddAsync(customer1);
        await _repository.AddAsync(customer2);
        await _repository.SaveChangesAsync();

        // Act
        var results = await _repository.GetAllAsync(c => c.FirstName == "Jane");

        // Assert
        results.Should().HaveCount(1);
        results.First().FirstName.Should().Be("Jane");
        results.First().LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        for (int i = 1; i <= 10; i++)
        {
            var email = Email.Create($"user{i}@example.com");
            var customer = new Customer($"User{i}", $"LastName{i}", email, _address);
            await _repository.AddAsync(customer);
        }
        await _repository.SaveChangesAsync();

        // Act
        var page1 = await _repository.GetPagedAsync(1, 3);
        var page2 = await _repository.GetPagedAsync(2, 3);
        var page3 = await _repository.GetPagedAsync(3, 3);
        var page4 = await _repository.GetPagedAsync(4, 3);

        // Assert
        page1.Should().HaveCount(3);
        page2.Should().HaveCount(3);
        page3.Should().HaveCount(3);
        page4.Should().HaveCount(1);
        page1.Select(c => c.Id).Should().NotIntersectWith(page2.Select(c => c.Id));
        page2.Select(c => c.Id).Should().NotIntersectWith(page3.Select(c => c.Id));
        page3.Select(c => c.Id).Should().NotIntersectWith(page4.Select(c => c.Id));
    }

    [Fact]
    public async Task GetPagedAsync_ShouldThrowException_WhenInvalidPageNumber()
    {
        // Act
        Func<Task> act = async () => await _repository.GetPagedAsync(0, 10);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*Page number must be at least 1*");
    }

    [Fact]
    public async Task GetPagedAsync_ShouldThrowException_WhenInvalidPageSize()
    {
        // Act
        Func<Task> act = async () => await _repository.GetPagedAsync(1, 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*Page size must be at least 1*");
    }

    [Fact]
    public async Task CountAsync_ShouldReturnTotalCount()
    {
        // Arrange
        var customer1 = new Customer("John", "Doe", _email, _address);
        var customer2 = new Customer("Jane", "Smith", Email.Create("jane@example.com"), _address);
        await _repository.AddAsync(customer1);
        await _repository.AddAsync(customer2);
        await _repository.SaveChangesAsync();

        // Act
        var count = await _repository.CountAsync();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
    {
        // Arrange
        var customer1 = new Customer("John", "Doe", _email, _address);
        var customer2 = new Customer("Jane", "Smith", Email.Create("jane@example.com"), _address);
        await _repository.AddAsync(customer1);
        await _repository.AddAsync(customer2);
        await _repository.SaveChangesAsync();

        // Act
        var count = await _repository.CountAsync(c => c.FirstName == "John");

        // Assert
        count.Should().Be(1);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenCustomerExists()
    {
        // Arrange
        var customer = new Customer("John", "Doe", _email, _address);
        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(c => c.Id == customer.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenCustomerNotExists()
    {
        // Act
        var exists = await _repository.ExistsAsync(c => c.Id == Guid.NewGuid());

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Update_ShouldUpdateCustomerInDatabase()
    {
        // Arrange
        var customer = new Customer("John", "Doe", _email, _address);
        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();

        // Act
        customer.UpdateName("Jane", "Smith");
        _repository.Update(customer);
        await _repository.SaveChangesAsync();

        // Assert
        var result = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_ShouldRemoveCustomerFromDatabase()
    {
        // Arrange
        var customer = new Customer("John", "Doe", _email, _address);
        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();

        // Act
        _repository.Delete(customer);
        await _repository.SaveChangesAsync();

        // Assert
        var result = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var customer = new Customer("John", "Doe", _email, _address);
        await _repository.AddAsync(customer);

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        var result = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        result.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
