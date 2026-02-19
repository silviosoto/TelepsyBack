using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TelePsy.BLL.Interfaces;
using TelePsy.BLL.Services;
using TelePsy.DAL.Repositories;
using TelePsy.Domain.Entities;
using TelePsy.Domain.Enums;
using Xunit;

namespace TelePsy.Tests
{
    public class InvoiceServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IGenericRepository<Invoice>> _mockInvoiceRepository;
        private readonly Mock<IGenericRepository<Payment>> _mockPaymentRepository;
        private readonly Mock<IGenericRepository<Appointment>> _mockAppointmentRepository;
        private readonly Mock<IGenericRepository<GlobalConfiguration>> _mockConfigRepository;
        private readonly InvoiceService _invoiceService;

        public InvoiceServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockInvoiceRepository = new Mock<IGenericRepository<Invoice>>();
            _mockPaymentRepository = new Mock<IGenericRepository<Payment>>();
            _mockAppointmentRepository = new Mock<IGenericRepository<Appointment>>();
            _mockConfigRepository = new Mock<IGenericRepository<GlobalConfiguration>>();

            _mockUnitOfWork.Setup(u => u.Repository<Invoice>()).Returns(_mockInvoiceRepository.Object);
            _mockUnitOfWork.Setup(u => u.Repository<Payment>()).Returns(_mockPaymentRepository.Object);
            _mockUnitOfWork.Setup(u => u.Repository<Appointment>()).Returns(_mockAppointmentRepository.Object);
            _mockUnitOfWork.Setup(u => u.Repository<GlobalConfiguration>()).Returns(_mockConfigRepository.Object);

            _invoiceService = new InvoiceService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GeneratePatientInvoiceAsync_ShouldCreateInvoiceWithPaidStatus()
        {
            // Arrange
            var paymentId = 1;
            var payment = new Payment
            {
                Id = paymentId,
                Amount = 150000,
                AppointmentId = 10,
                PatientInvoiceId = null
            };

            var appointment = new Appointment
            {
                Id = 10,
                PatientId = 5
            };

            _mockPaymentRepository.Setup(r => r.GetByIdAsync(paymentId)).ReturnsAsync(payment);
            _mockAppointmentRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(appointment);

            // Act
            var result = await _invoiceService.GeneratePatientInvoiceAsync(paymentId);

            // Assert
            result.Should().NotBeNull();
            result.TotalAmount.Should().Be(150000);
            result.Status.Should().Be(InvoiceStatus.Paid);
            result.Type.Should().Be(InvoiceType.ClientPurchase);
            
            _mockInvoiceRepository.Verify(r => r.AddAsync(It.IsAny<Invoice>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetGlobalCommissionAsync_ShouldReturnDefaultWhenNotFound()
        {
            // Arrange
            _mockConfigRepository.Setup(r => r.GetAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<GlobalConfiguration, bool>>>(),
                null, ""))
                .ReturnsAsync(new List<GlobalConfiguration>());

            // Act
            var result = await _invoiceService.GetGlobalCommissionAsync();

            // Assert
            result.Should().Be(0.30m); // Default hardcoded in Service
        }
    }
}
