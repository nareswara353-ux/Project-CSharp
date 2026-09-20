using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;

namespace Core.Tests.Domain.Entities;

public class OrderTests
{
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Money _price = new(50m, "USD");

    [Fact]
    public void Constructor_ShouldCreateOrder_WithDraftStatus()
    {
        var order = new Order(_customerId, "Test notes");

        order.CustomerId.Should().Be(_customerId);
        order.Status.Should().Be(OrderStatus.Draft);
        order.Notes.Should().Be("Test notes");
        order.Lines.Should().BeEmpty();
        order.OrderDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenCustomerIdEmpty()
    {
        Action act = () => new Order(Guid.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Customer ID cannot be empty*");
    }

    [Fact]
    public void AddLine_ShouldAddLine_WhenValidData()
    {
        var order = new Order(_customerId);

        order.AddLine(_productId, "Widget", _price, new Quantity(2));

        order.Lines.Should().HaveCount(1);
        order.Lines.First().ProductId.Should().Be(_productId);
        order.Lines.First().Quantity.Value.Should().Be(2);
    }

    [Fact]
    public void AddLine_ShouldMergeQuantities_WhenProductAlreadyExists()
    {
        var order = new Order(_customerId);
        order.AddLine(_productId, "Widget", _price, new Quantity(2));

        order.AddLine(_productId, "Widget", _price, new Quantity(3));

        order.Lines.Should().HaveCount(1);
        order.Lines.First().Quantity.Value.Should().Be(5);
    }

    [Fact]
    public void AddLine_ShouldThrowException_WhenOrderNotDraft()
    {
        var order = new Order(_customerId);
        order.AddLine(_productId, "Widget", _price, new Quantity(1));
        order.Confirm();

        Action act = () => order.AddLine(Guid.NewGuid(), "Other", _price, new Quantity(1));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only Draft*");
    }

    [Fact]
    public void GetTotalAmount_ShouldSumLineSubtotals()
    {
        var order = new Order(_customerId);
        order.AddLine(_productId, "Widget", new Money(10m, "USD"), new Quantity(2));
        order.AddLine(Guid.NewGuid(), "Gadget", new Money(20m, "USD"), new Quantity(3));

        var total = order.GetTotalAmount();

        total.Amount.Should().Be(80m);
        total.Currency.Should().Be("USD");
    }

    [Fact]
    public void GetTotalAmount_ShouldThrowException_WhenEmpty()
    {
        var order = new Order(_customerId);

        Action act = () => order.GetTotalAmount();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*empty order*");
    }

    [Fact]
    public void Confirm_ShouldSetStatusConfirmed_WhenValid()
    {
        var order = new Order(_customerId);
        order.AddLine(_productId, "Widget", _price, new Quantity(1));

        order.Confirm();

        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public void Confirm_ShouldThrowException_WhenNoLines()
    {
        var order = new Order(_customerId);

        Action act = () => order.Confirm();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*no line items*");
    }

    [Fact]
    public void Ship_ShouldSetStatusShipped_WhenConfirmed()
    {
        var order = new Order(_customerId);
        order.AddLine(_productId, "Widget", _price, new Quantity(1));
        order.Confirm();

        order.Ship();

        order.Status.Should().Be(OrderStatus.Shipped);
        order.ShippedAt.Should().NotBeNull();
    }

    [Fact]
    public void Ship_ShouldThrowException_WhenNotConfirmed()
    {
        var order = new Order(_customerId);
        order.AddLine(_productId, "Widget", _price, new Quantity(1));

        Action act = () => order.Ship();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Only Confirmed*");
    }

    [Fact]
    public void Cancel_ShouldSetStatusCancelled_WhenDraft()
    {
        var order = new Order(_customerId);

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenAlreadyShipped()
    {
        var order = new Order(_customerId);
        order.AddLine(_productId, "Widget", _price, new Quantity(1));
        order.Confirm();
        order.Ship();

        Action act = () => order.Cancel();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already been shipped*");
    }

    [Fact]
    public void GetTotalItemCount_ShouldSumQuantities()
    {
        var order = new Order(_customerId);
        order.AddLine(_productId, "Widget", _price, new Quantity(2));
        order.AddLine(Guid.NewGuid(), "Gadget", _price, new Quantity(3));

        var count = order.GetTotalItemCount();

        count.Should().Be(5);
    }

    [Fact]
    public void UpdateNotes_ShouldChangeNotes()
    {
        var order = new Order(_customerId, "Old");

        order.UpdateNotes("New notes");

        order.Notes.Should().Be("New notes");
    }
}
