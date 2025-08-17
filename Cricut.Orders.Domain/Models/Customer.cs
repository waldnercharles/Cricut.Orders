﻿namespace Cricut.Orders.Domain.Models
{
    public class Customer
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string Address { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;
    }
}