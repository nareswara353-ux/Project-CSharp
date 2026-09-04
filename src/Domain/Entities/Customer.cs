using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Customer : Entity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public Address BillingAddress { get; private set; }
    public Address? ShippingAddress { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Customer() { } // For EF Core

    public Customer(
        string firstName,
        string lastName,
        Email email,
        Address billingAddress,
        Address? shippingAddress = null)
        : base()
    {
        SetName(firstName, lastName);
        Email = email ?? throw new ArgumentNullException(nameof(email));
        BillingAddress = billingAddress ?? throw new ArgumentNullException(nameof(billingAddress));
        ShippingAddress = shippingAddress;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string firstName, string lastName)
    {
        SetName(firstName, lastName);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEmail(Email newEmail)
    {
        Email = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateBillingAddress(Address newAddress)
    {
        BillingAddress = newAddress ?? throw new ArgumentNullException(nameof(newAddress));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateShippingAddress(Address? newAddress)
    {
        ShippingAddress = newAddress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public string FullName => $"{FirstName} {LastName}";
}
