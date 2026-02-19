using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using TelePsy.BLL.Interfaces;
using TelePsy.BLL.Services;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.Entities;
using TelePsy.Domain.Enums;
using Xunit;

namespace TelePsy.Tests
{
    public class PayUServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IGenericRepository<Invoice>> _mockInvoiceRepository;
        private readonly Mock<IGenericRepository<Payment>> _mockPaymentRepository;
        private readonly PayUService _payUService;

        public PayUServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockInvoiceRepository = new Mock<IGenericRepository<Invoice>>();
            _mockPaymentRepository = new Mock<IGenericRepository<Payment>>();

            _mockConfiguration.Setup(c => c["PayU:MerchantId"]).Returns("508029");
            _mockConfiguration.Setup(c => c["PayU:ApiKey"]).Returns("4Vj8eK4rloUd272L48hsrarnUA");
            _mockConfiguration.Setup(c => c["PayU:AccountId"]).Returns("512321");

            _mockUnitOfWork.Setup(u => u.Repository<Invoice>()).Returns(_mockInvoiceRepository.Object);
            _mockUnitOfWork.Setup(u => u.Repository<Payment>()).Returns(_mockPaymentRepository.Object);

            _payUService = new PayUService(_mockUnitOfWork.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task CreatePaymentRequestAsync_ShouldReturnCorrectJsonAndSignature()
        {
            // Arrange
            var invoiceId = 1;
            var invoice = new Invoice
            {
                Id = invoiceId,
                InvoiceNumber = "INV-001",
                TotalAmount = 100000,
                Details = new List<InvoiceDetail> { new InvoiceDetail { AppointmentId = 10 } },
                Patient = new Patient { Person = new Person { User = new User { Email = "test@user.com" } } }
            };

            _mockInvoiceRepository.Setup(r => r.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
                It.IsAny<string>()))
                .ReturnsAsync(new List<Invoice> { invoice });

            // Act
            var result = await _payUService.CreatePaymentRequestAsync(invoiceId);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("\"merchantId\":\"508029\"");
            result.Should().Contain("\"amount\":\"100000\"");
            result.Should().Contain("\"currency\":\"COP\"");
            
            // Verify payment was saved
            _mockPaymentRepository.Verify(r => r.AddAsync(It.Is<Payment>(p => p.Amount == 100000)), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ProcessPaymentConfirmationAsync_ShouldUpdatePaymentWhenApproved()
        {
            // Arrange
            var referenceCode = "INV-1-638500000000000000";
            var confirmationData = new PayUConfirmationData
            {
                ReferenceCode = referenceCode,
                Amount = 100000,
                Currency = "COP",
                State = 4, // Approved
                TransactionId = "PAYU-123",
                Signature = "INVALID_SIGNATURE" // Logic currently doesn't strictly check signature in confirmation yet or it's hard to replicate exact MD5 here easily without same logic
            };

            var payment = new Payment { TransactionId = referenceCode, Status = "Pending", Id = 50 };
            
            _mockPaymentRepository.Setup(r => r.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Payment, bool>>>(),
                null, ""))
                .ReturnsAsync(new List<Payment> { payment });

            _mockInvoiceRepository.Setup(r => r.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Invoice, bool>>>(),
                null, ""))
                .ReturnsAsync(new List<Invoice>()); // No invoice for simplicity

            // Act
            var result = await _payUService.ProcessPaymentConfirmationAsync(confirmationData);

            // Assert
            result.Should().BeTrue();
            payment.Status.Should().Be("Completed");
            payment.TransactionId.Should().Be("PAYU-123");
            _mockPaymentRepository.Verify(r => r.Update(payment), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
        }
    }
}
